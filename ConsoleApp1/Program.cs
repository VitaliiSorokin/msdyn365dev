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
            int iterations = Constants.TotalRecords / Constants.BatchSize;
            int remainder = Constants.TotalRecords % Constants.BatchSize;
            if (iterations == 0)
            {
                Generator.Run(Constants.TotalRecords);
            }
            else
            {
                for (int i = 0; i < iterations; i++)
                {
                    Console.WriteLine("iteration: " + i);
                    Generator.Run(Constants.BatchSize);
                }
                if (remainder != 0)
                {
                    Generator.Run(remainder);
                }
            }

        }
    }
}
