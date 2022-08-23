using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;

namespace SBMAPIInterface
{
    class Program
    {
        private const string serverAdress = "http://servername";
        private const int tableID = 1004;
        private const string userName = "user";
        private const string password = "password";
        private const int reportIDForAllItems = 1076;

        static void Main(string[] args)
        {
            Console.WriteLine("Connecting to SBM");

            APIInterface smbInterface = new APIInterface(serverAdress);
            smbInterface.Open(userName, password);

            string version = smbInterface.GetVersion();
            Console.WriteLine(version);
            Console.WriteLine();

            Console.WriteLine("Listing work items");
            smbInterface.ReadItems(tableID, 100000);
            //smbInterface.ReadAllItems(tableID);
            //smbInterface.ReadItemsFromReport(reportIDForAllItems);
        }
    }
}
