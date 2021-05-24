using System;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;


namespace MyPlugins
{
    public class PriceCalculate : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity rent = (Entity)context.InputParameters["Target"];

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {

                    DateTime reservedPickup = DateTime.MinValue;
                    DateTime reservedHandover = DateTime.MinValue;
                    int pickupLocation = 0;
                    int returnLocation = 0;
                    Guid carClassId = Guid.Empty;

                    if (context.PreEntityImages.Contains("PreRentImage"))
                    {
                        Entity preRentImage = (Entity)context.PreEntityImages["PreRentImage"];
                        reservedPickup = preRentImage.Contains("new_reservedpickup") ?
                            (DateTime)preRentImage.Attributes["new_reservedpickup"] : DateTime.MinValue;
                        reservedHandover = preRentImage.Contains("new_reservedhandover") ?
                            (DateTime)preRentImage.Attributes["new_reservedhandover"] : DateTime.MinValue;
                        pickupLocation = preRentImage.Contains("new_pickuplocation") ?
                            ((OptionSetValue)preRentImage.Attributes["new_pickuplocation"]).Value : 0;
                        returnLocation = preRentImage.Contains("new_returnlocation") ?
                            ((OptionSetValue)preRentImage.Attributes["new_returnlocation"]).Value : 0;
                        carClassId = preRentImage.Contains("new_carclass") ?
                            ((EntityReference)preRentImage.Attributes["new_carclass"]).Id : Guid.Empty;
                    }

                    if (rent.Attributes.Contains("new_reservedpickup"))
                    {
                        reservedPickup = (DateTime)rent.Attributes["new_reservedpickup"];
                    }

                    if (rent.Attributes.Contains("new_reservedhandover"))
                    {
                        reservedHandover = (DateTime)rent.Attributes["new_reservedhandover"];
                    }

                    if (rent.Attributes.Contains("new_pickuplocation"))
                    {
                        pickupLocation = ((OptionSetValue)rent.Attributes["new_pickuplocation"]).Value;
                    }

                    if (rent.Attributes.Contains("new_returnlocation"))
                    {
                        returnLocation = ((OptionSetValue)rent.Attributes["new_returnlocation"]).Value;
                    }

                    if (rent.Attributes.Contains("new_carclass"))
                    {
                        carClassId = ((EntityReference)rent.Attributes["new_carclass"]).Id;
                    }

                    if (reservedPickup != DateTime.MinValue && reservedHandover != DateTime.MinValue && pickupLocation != 0 && returnLocation != 0 && carClassId != Guid.Empty)
                    {
                        int pickupLocationFee = pickupLocation == 279640002 ? 0 : 100;
                        int returnLocationFee = returnLocation == 279640002 ? 0 : 100;
                        int rentDays = (reservedHandover - reservedPickup).Days;

                        Entity carClass = service.Retrieve("new_carclass", carClassId, new ColumnSet("new_price"));
                        int carPricePerDay = Decimal.ToInt32(((Money)carClass.Attributes["new_price"]).Value);
                        int price = (carPricePerDay * rentDays) + pickupLocationFee + returnLocationFee;

                        rent.Attributes.Add("new_price", new Money(price));
                    }

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
