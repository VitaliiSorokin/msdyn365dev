using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {
            CrmServiceClient crmSvc = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString);
            Console.WriteLine(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString);

            using (svcContext context = new svcContext(crmSvc))
            {
                List<new_car> cars = context.new_carSet.ToList<new_car>();
                List<new_carclass> carClasses = context.new_carclassSet.ToList<new_carclass>();
                List<Contact> contacts = context.ContactSet.ToList<Contact>();

                try
                {
                    var createRentsReq = new ExecuteMultipleRequest()
                    {
                        Requests = new OrganizationRequestCollection(),
                        Settings = new ExecuteMultipleSettings
                        {
                            ContinueOnError = false,
                            ReturnResponses = true
                        }
                    };

                    Random random = new Random();
                    for (int i = 0; i < Constants.TotalRecords; i++)
                    {
                        DateTime reservedPickup =
                            Helpers.GenerateDateInRange(
                                new DateTime(Constants.RentDuration.YearFrom, Constants.RentDuration.MonthFrom, Constants.RentDuration.DayFrom),
                                new DateTime(Constants.RentDuration.YearTo, Constants.RentDuration.MonthTo, Constants.RentDuration.DayTo),
                                random);
                        DateTime reservedReturn = reservedPickup.Add(Helpers.GenerateRentDuraton(random));
                        Guid classId = Helpers.GetCarClassId(carClasses, random);
                        Guid carId = Helpers.GetCarIdByClassId(classId, cars, random);
                        Guid customerId = Helpers.GetCustomerId(contacts, random);
                        int pickupLocValue = Helpers.GetLocationValue(random);
                        int returnLocValue = Helpers.GetLocationValue(random);

                        new_rent rent = new new_rent
                        {
                            new_ReservedPickup = reservedPickup,
                            new_ReservedHandover = reservedReturn,
                            new_CarClass = new EntityReference(new_carclass.EntityLogicalName, classId),
                            new_Car = new EntityReference(new_car.EntityLogicalName, carId),
                            new_Customer = new EntityReference(Contact.EntityLogicalName, customerId),
                            new_PickupLocation = new OptionSetValue(pickupLocValue),
                            new_ReturnLocation = new OptionSetValue(returnLocValue)
                        };

                        var createRequest = new CreateRequest() { Target = rent };
                        createRentsReq.Requests.Add(createRequest);
                    }

                    var rentsResponse = (ExecuteMultipleResponse)crmSvc.Execute(createRentsReq);

                    //--------- CREATE LISTS WITH RENTS IDS-----
                    List<string> created = new List<string>();
                    int createdInd = (int)(Constants.TotalRecords * 0.05);
                    List<string> confirmed = new List<string>();
                    int confirmedInd = createdInd + (int)(Constants.TotalRecords * 0.05);
                    List<string> renting = new List<string>();
                    int rentingInd = confirmedInd + (int)(Constants.TotalRecords * 0.05);
                    List<string> returned = new List<string>();
                    int returnedInd = rentingInd + (int)(Constants.TotalRecords * 0.75);
                    List<string> canceled = new List<string>();
                    int canceledInd = returnedInd + (int)(Constants.TotalRecords * 0.1);

                    int recordsCounter = 0;
                    foreach (var r in rentsResponse.Responses)
                    {
                        if (r.Response != null)
                        {
                            Console.WriteLine("Success: " + r.Response.Results["id"].ToString());
                            if (recordsCounter < createdInd)
                            {
                                created.Add(r.Response.Results["id"].ToString());
                            }
                            else if (recordsCounter < confirmedInd)
                            {
                                confirmed.Add(r.Response.Results["id"].ToString());
                            }
                            else if (recordsCounter < rentingInd)
                            {
                                renting.Add(r.Response.Results["id"].ToString());
                            }
                            else if (recordsCounter < returnedInd)
                            {
                                returned.Add(r.Response.Results["id"].ToString());
                            }
                            else if (recordsCounter < canceledInd)
                            {
                                canceled.Add(r.Response.Results["id"].ToString());
                            }
                        }
                        else if (r.Fault != null)
                        {
                            Console.WriteLine(r.Fault);
                        }
                        recordsCounter++;
                    }

                    //------ UPDATE RENTS WITH REPORTS ----
                    var multipleRequest = new ExecuteMultipleRequest()
                    {
                        Requests = new OrganizationRequestCollection(),
                        Settings = new ExecuteMultipleSettings
                        {
                            ContinueOnError = false,
                            ReturnResponses = true
                        }
                    };

                    //------ CONFIRMED ----
                    for (int i = 0; i < confirmed.Count; i++)
                    {
                        Guid rentId = Guid.Parse(confirmed[i]);

                        new_rent newRent = new new_rent
                        {
                            Id = Guid.Parse(confirmed[i]),
                            new_Paid = i < Math.Round(confirmed.Count * 0.9),
                            statuscode = new OptionSetValue(Constants.RentStatus.Confirmed)
                        };

                        UpdateRequest updateRentRequest = new UpdateRequest { Target = newRent };
                        multipleRequest.Requests.Add(updateRentRequest);
                    }
                    //------ RENTING ----
                    for (int i = 0; i < renting.Count; i++)
                    {
                        Guid rentId = Guid.Parse(renting[i]);

                        new_rent currentRent = (new_rent)crmSvc.Retrieve(new_rent.EntityLogicalName, rentId, new ColumnSet(true));

                        new_cartransferreport newReport = new new_cartransferreport
                        {
                            new_Type = Constants.ReportType.Pickup,
                            new_Date = currentRent.new_ReservedPickup,
                            new_Car = currentRent.new_Car,
                        };

                        CreateRequest createReportRequest = new CreateRequest { Target = newReport };
                        CreateResponse cresp = (CreateResponse)crmSvc.Execute(createReportRequest);
                        Guid reportId = cresp.id;

                        new_rent newRent = new new_rent
                        {
                            Id = rentId,
                            new_Paid = i < Math.Round(renting.Count * 0.999),
                            statuscode = new OptionSetValue(Constants.RentStatus.Renting),
                            new_PickupReport = new EntityReference(new_cartransferreport.EntityLogicalName, reportId)
                        };

                        UpdateRequest updateRentRequest = new UpdateRequest { Target = newRent };
                        multipleRequest.Requests.Add(updateRentRequest);
                    }

                    //------ RETURNED -----
                    for (int i = 0; i < returned.Count; i++)
                    {
                        Guid rentId = Guid.Parse(returned[i]);

                        new_rent currentRent = (new_rent)crmSvc.Retrieve(new_rent.EntityLogicalName, rentId, new ColumnSet(true));

                        new_cartransferreport newPickupReport = new new_cartransferreport
                        {
                            new_Type = Constants.ReportType.Pickup,
                            new_Date = currentRent.new_ReservedPickup,
                            new_Car = currentRent.new_Car,
                        };

                        bool hasDamage = i < Math.Round(returned.Count * 0.5);
                        new_cartransferreport newReturnReport = new new_cartransferreport
                        {
                            new_Type = Constants.ReportType.Return,
                            new_Damages = hasDamage,
                            new_DamageDescription = hasDamage ? "damage" : null,
                            new_Date = currentRent.new_ReservedHandover,
                            new_Car = currentRent.new_Car,
                        };

                        CreateRequest createPickupReportRequest = new CreateRequest { Target = newPickupReport };
                        CreateResponse crespPickup = (CreateResponse)crmSvc.Execute(createPickupReportRequest);
                        Guid pickupReportId = crespPickup.id;

                        CreateRequest createReturnReportRequest = new CreateRequest { Target = newReturnReport };
                        CreateResponse crespReturn = (CreateResponse)crmSvc.Execute(createReturnReportRequest);
                        Guid returnReportId = crespReturn.id;

                        new_rent updRent = new new_rent
                        {
                            Id = rentId,
                            new_Paid = i < Math.Round(returned.Count * 0.9998),
                            statecode = new_rentState.Inactive,
                            statuscode = new OptionSetValue(Constants.RentStatus.Returned),
                            new_PickupReport = new EntityReference(new_cartransferreport.EntityLogicalName, pickupReportId),
                            new_ReturnReport = new EntityReference(new_cartransferreport.EntityLogicalName, returnReportId),
                        };

                        UpdateRequest updateRentRequest = new UpdateRequest { Target = updRent };
                        multipleRequest.Requests.Add(updateRentRequest);
                    }

                    //------ CANCELED -----
                    foreach (var id in canceled)
                    {
                        Guid rentId = Guid.Parse(id);

                        new_rent newRent = new new_rent
                        {
                            Id = rentId,
                            statecode = new_rentState.Inactive,
                            statuscode = new OptionSetValue(Constants.RentStatus.Canceled),
                        };

                        UpdateRequest updateRentRequest = new UpdateRequest { Target = newRent };
                        multipleRequest.Requests.Add(updateRentRequest);
                    }

                    var mltplResponse = (ExecuteMultipleResponse)crmSvc.Execute(multipleRequest);

                    foreach (var r in mltplResponse.Responses)
                    {
                        if (r.Response != null)
                        {
                            Console.WriteLine("succes");
                        }
                        else if (r.Fault != null)
                        {
                            Console.WriteLine(r.Fault);
                        }
                    }
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }

            }
        }
    }
}
