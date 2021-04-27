using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Helpers
    {
        //public static void DeleteWrongCars()
        //{
        //    CrmServiceClient crmSvc = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString);

        //    string query = @"
        //    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        //        <entity name='new_car'>
        //        <attribute name='new_vin' />
        //        <attribute name='new_purchase' />
        //        <attribute name='new_productiondate' />
        //        <attribute name='new_carmodel' />
        //        <attribute name='new_carmanufacturer' />
        //        <attribute name='new_carclass' />
        //        <attribute name='new_carid' />
        //        <order attribute='new_vin' descending='false' />
        //        <filter type='and'>
        //            <condition attribute='new_productiondate' operator='gt' valueof='new_purchase'/>
        //        </filter>
        //        </entity>
        //    </fetch>";

        //    EntityCollection entityCollection = crmSvc.RetrieveMultiple(new FetchExpression(query));

        //    int i = 0;
        //    foreach (Entity car in entityCollection.Entities)
        //    {
        //        i++;
        //        //crmSvc.Delete("new_car", new Guid(car.Attributes["new_carid"].ToString()));
        //        Console.WriteLine(car.Attributes["new_vin"].ToString());
        //        Console.WriteLine(car.Attributes["new_purchase"].ToString());
        //        Console.WriteLine(car.Attributes["new_productiondate"].ToString());
        //    }
        //    Console.WriteLine(i);
        //    Console.Read();
        //} // TODO: to delete?

        public static DateTime GenerateDateInRange(DateTime startDate, DateTime endDate, Random rnd)
        {
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, rnd.Next(0, (int)timeSpan.TotalMinutes), 0);
            return startDate + newSpan;
        }

        public static TimeSpan GenerateRentDuraton(Random rnd)
        {
            int days = rnd.Next(Constants.RentDuration.Min, Constants.RentDuration.Max);
            int hours = rnd.Next(0, 24);
            return new TimeSpan(days, hours, 0, 0);
        }

        public static Guid GetCarClassId(List<new_carclass> classes, Random rnd)
        {
            int index = rnd.Next(classes.Count);
            return classes[index].Id;
        }

        public static Guid GetCarIdByClassId(Guid classId, List<new_car> cars, Random rnd)
        {
            List<new_car> filteredCars = (List<new_car>)cars.Where(c => c.new_CarClass.Id.Equals(classId)).ToList();
            int index = rnd.Next(filteredCars.Count);
            return filteredCars[index].Id;
        }

        public static Guid GetCustomerId(List<Contact> contacts, Random rnd)
        {
            int index = rnd.Next(contacts.Count);
            return contacts[index].Id;
        }

        public static int GetLocationValue(Random rnd)
        {
            List<int> locValues = new List<int>() {
                Constants.Locations.Airport,
                Constants.Locations.CityCenter,
                Constants.Locations.Office
            };
            int index = rnd.Next(locValues.Count);
            return locValues[index];
        }
    }
}
