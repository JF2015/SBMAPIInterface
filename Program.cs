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

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            bool authenticated = getAuthToken(client, out string token);
            if (!authenticated)
                return;

            client.DefaultRequestHeaders.Add("alfssoauthntoken", token);

            Console.WriteLine("Received Auth Token");

            getVersion(client);

            Console.WriteLine("Listing SCRs");
            //readItems(client);
            //readAllItems(client);
            readItemsFromReport(client, reportIDForAllItems);
        }

        private static bool getAuthToken(HttpClient client, out string token)
        {
            token = "";

            string message = "{\"credentials\": { \"username\" : \"" + userName + "\", \"password\":\"" + password + "\"}}";
            var request = new HttpRequestMessage(HttpMethod.Post, serverAdress + ":8085/idp/services/rest/TokenService/")
            {
                Content = new StringContent(message, Encoding.UTF8, "application/json")
            };

            var result = client.SendAsync(request).Result.Content.ReadAsStringAsync();

            if (result.IsFaulted)
            {
                Console.WriteLine("Failed to authenticate");
                return false;
            }

            var tokenResponse = JsonDocument.Parse(result.Result);
            tokenResponse.RootElement.GetProperty("token").TryGetProperty("value", out JsonElement val);

            token = val.GetString();
            return true;
        }

        private static void getVersion(HttpClient client)
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, serverAdress + "/jsonapi/getversion");
            var result = client.SendAsync(postRequest).Result.Content.ReadAsStringAsync();

            var versionResponse = JsonDocument.Parse(result.Result);
            var version = versionResponse.RootElement.GetProperty("version").GetString();
            Console.WriteLine(version);
            Console.WriteLine();
        }

        private static void readItems(HttpClient client)
        {
            int counter = 0;

            List<int> integerList = Enumerable.Range(0, 100000).ToList();
            Parallel.ForEach(integerList, i =>
            {
                try
                {
                    Interlocked.Increment(ref counter);
                    var getItemRequest = new HttpRequestMessage(HttpMethod.Post, serverAdress + "/jsonapi/getItem/" + tableID + "/" + i);
                    var getItemResult = client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

                    var itemResponse = JsonDocument.Parse(getItemResult.Result);
                    var type = itemResponse.RootElement.GetProperty("result").GetProperty("type");
                    if (type.GetString() == "ERROR")
                        return;

                    itemResponse.RootElement.GetProperty("item").GetProperty("id").TryGetProperty("itemId", out JsonElement valItem);
                    string id = valItem.GetString();

                    itemResponse.RootElement.GetProperty("item").GetProperty("fields").TryGetProperty("TITLE", out valItem);
                    string title = valItem.GetProperty("value").GetString();

                    Console.WriteLine(counter + " " + id + " " + title);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private static void readAllItems(HttpClient client)
        {
            try
            {
                var getItemRequest = new HttpRequestMessage(HttpMethod.Post, serverAdress + "/workcenter/tmtrack.dll?JSONPage&command=jsonapi&JSON_Func=getitemsbyitemid&JSON_P1=1004&JSON_P2=*");
                var getItemResult = client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

                var itemResponse = JsonDocument.Parse(getItemResult.Result);
                var type = itemResponse.RootElement.GetProperty("result").GetProperty("type");
                if (type.GetString() == "ERROR")
                    return;

                //TODO: Parse list of items
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void readItemsFromReport(HttpClient client, int reportID)
        {
            try
            {
                var getItemRequest = new HttpRequestMessage(HttpMethod.Post, serverAdress + "/jsonapi/getitemsbylistingreport/" + reportID + "?pagesize=100&rptkey=1312321&recno=2");
                var getItemResult = client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

                var itemResponse = JsonDocument.Parse(getItemResult.Result);
                var type = itemResponse.RootElement.GetProperty("result").GetProperty("type");
                if (type.GetString() == "ERROR")
                    return;

                //TODO: Parse list of items
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
