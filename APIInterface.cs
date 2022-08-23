using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SBMAPIInterface
{
    public class APIInterface
    {
        private HttpClient m_client = new HttpClient();
        private string m_address;

        public APIInterface(string serverAddress)
        {
            m_address = serverAddress;
        }

        public void Open(string userName, string password)
        {
            m_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            bool authenticated = getAuthToken(m_client, userName, password, out string token);
            if (!authenticated)
                return;

            m_client.DefaultRequestHeaders.Add("alfssoauthntoken", token);

            Console.WriteLine("Received Auth Token");
        }

        private bool getAuthToken(HttpClient client, string userName, string password, out string token)
        {
            token = "";

            string message = "{\"credentials\": { \"username\" : \"" + userName + "\", \"password\":\"" + password + "\"}}";
            var request = new HttpRequestMessage(HttpMethod.Post, m_address + ":8085/idp/services/rest/TokenService/")
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

        public string GetVersion()
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, m_address + "/jsonapi/getversion");
            var result = m_client.SendAsync(postRequest).Result.Content.ReadAsStringAsync();

            var versionResponse = JsonDocument.Parse(result.Result);
            var version = versionResponse.RootElement.GetProperty("version").GetString();
            return version;
        }

        /// <summary>
        /// Function to read the first 'range' items from the table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="range"></param>
        public void ReadItems(int table, int range)
        {
            int counter = 0;

            List<int> integerList = Enumerable.Range(0, range).ToList();
            Parallel.ForEach(integerList, i =>
            {
                try
                {
                    Interlocked.Increment(ref counter);
                    var getItemRequest = new HttpRequestMessage(HttpMethod.Post, m_address + "/jsonapi/getItem/" + table + "/" + i);
                    var getItemResult = m_client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

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

        /// <summary>
        /// Function to read all items from the table. Caused by paging issues, can only read the first 1000 items
        /// </summary>
        /// <param name="table"></param>
        public void ReadAllItems(int table)
        {
            try
            {
                var getItemRequest = new HttpRequestMessage(HttpMethod.Post, m_address + "/workcenter/tmtrack.dll?JSONPage&command=jsonapi&JSON_Func=getitemsbyitemid&JSON_P1=" + table + "&JSON_P2=*&pagesize=1000");
                var getItemResult = m_client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

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

        /// <summary>
        /// Read all items from a preconfigured report
        /// </summary>
        /// <param name="reportID"></param>
        public void ReadItemsFromReport(int reportID)
        {
            try
            {
                var getItemRequest = new HttpRequestMessage(HttpMethod.Post, m_address + "/jsonapi/getitemsbylistingreport/" + reportID + "?pagesize=100&rptkey=1312321&recno=2");
                var getItemResult = m_client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

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
