using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace ConsoleApp1
{
    class Helpers
    {
        public static DateTime GenerateDateInRange(DateTime startDate, DateTime endDate, Random random)
        {
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan randomTimeSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
            return startDate + randomTimeSpan;
        }

        public static TimeSpan GenerateRentDuraton(Random random)
        {
            int days = random.Next(Constants.RentDuration.Min, Constants.RentDuration.Max);
            int hours = random.Next(0, 24);
            return new TimeSpan(days, hours, 0, 0);
        }

        public static new_carclass GetCarClass(List<new_carclass> classes, Random random)
        {
            int index = random.Next(classes.Count);
            return classes[index];
        }

        public static Guid GetCarIdByClassId(Guid classId, List<new_car> cars, Random random)
        {
            List<new_car> filteredCars = (List<new_car>)cars
                .Where(c => c.new_CarClass.Id.Equals(classId))
                .ToList();
            int index = random.Next(filteredCars.Count);
            return filteredCars[index].Id;
        }

        public static Guid GetCustomerId(List<Contact> contacts, Random rnd)
        {
            int index = rnd.Next(contacts.Count);
            return contacts[index].Id;
        }

        public static int GetLocationValue(Random random)
        {
            List<int> locationValues = new List<int>() {
                Constants.Locations.Airport,
                Constants.Locations.CityCenter,
                Constants.Locations.Office
            };
            int index = random.Next(locationValues.Count);
            return locationValues[index];
        }

        public static Money GetPriceForRent(TimeSpan duration, new_carclass carClass)
        {
            int pricePerDay = Decimal.ToInt32(carClass.new_Price.Value);
            int totalDays = duration.Days;
            return new Money(pricePerDay * totalDays);
        }
    }
}
