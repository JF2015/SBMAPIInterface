﻿using System;
using System.Linq;
using SBMAPIInterface;

namespace SBMAPIInterfaceTest
{
    class Program
    {
        private const string serverAdress = "http://servername";
        private const int tableID = 1004;
        private const string userName = "user";
        private const string password = "password";
        private const int reportIDForAllItems = 1076;

        static void Main()
        {
            Console.WriteLine("Connecting to SBM");

            APIInterface sbmApiInterface = new APIInterface(serverAdress);
            bool success = sbmApiInterface.Open(userName, password);
            if (!success)
                return;

            string version = sbmApiInterface.GetVersion();
            Console.WriteLine(version);
            Console.WriteLine();

            Console.WriteLine("Listing work items");
            //sbmApiInterface.ReadItems(tableID, 100000);
            //sbmApiInterface.ReadAllItems(tableID);
            var workItems = sbmApiInterface.ReadItemsFromReport(reportIDForAllItems);
            var sorted = workItems.OrderBy(p => p.ID);
            foreach (var item in sorted)
            {
                Console.WriteLine(item.ID + " " + item.Title);
            }
        }
    }
}
