using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Constants
    {
        public const int TotalRecords = 20;

        public static class Locations
        {
            public const int Airport = 279640000;
            public const int CityCenter = 279640001;
            public const int Office = 279640002;
        }
        public static class RentStatus
        {
            public const int Created = 279640000;
            public const int Confirmed = 279640001;
            public const int Renting = 279640002;
            public const int Returned = 279640003;
            public const int Canceled = 279640004;
        }
        public static class ReportType
        {
            public const bool Return = false;
            public const bool Pickup = true;
        }

        public static class RentDuration
        {
            public const int Min = 1;
            public const int Max = 29;
            public const int YearFrom = 2019;
            public const int MonthFrom = 1;
            public const int DayFrom = 1;
            public const int YearTo = 2020;
            public const int MonthTo = 12;
            public const int DayTo = 31;
        }
    }
}
