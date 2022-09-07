using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SBMAPIInterface
{
    public class APIInterface
    {
        private HttpClient m_client = new HttpClient();
        private string m_address;
        private int m_repeatKey = 1312321;

        public APIInterface(string serverAddress)
        {
            m_address = serverAddress;
        }

        /// <summary>
        /// Open the connection to receive the authentication token
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Open(string userName, string password)
        {
            m_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            bool authenticated = getAuthToken(m_client, userName, password, out string token);
            if (!authenticated)
                return false;

            m_client.DefaultRequestHeaders.Add("alfssoauthntoken", token);

            Debug.WriteLine("Received Auth Token");
            return true;
        }

        private bool getAuthToken(HttpClient client, string userName, string password, out string token)
        {
            token = "";

            string message = $"{{\"credentials\": {{ \"username\" : \"{userName}\", \"password\":\"{password}\"}}}}";
            var request = new HttpRequestMessage(HttpMethod.Post, $"{m_address}:8085/idp/services/rest/TokenService/")
            {
                Content = new StringContent(message, Encoding.UTF8, "application/json")
            };

            var result = client.SendAsync(request).Result.Content.ReadAsStringAsync();

            if (result.IsFaulted)
            {
                Debug.WriteLine("Failed to authenticate");
                return false;
            }

            var tokenResponse = JsonDocument.Parse(result.Result);
            var error = tokenResponse.RootElement.GetProperty("status");
            if (error.GetString() == "Error")
            {
                var errorDetails = tokenResponse.RootElement.GetProperty("error").GetProperty("detail").GetString();
                Debug.WriteLine($"Failed to authenticate: {errorDetails}");
                return false;
            }
            tokenResponse.RootElement.GetProperty("token").TryGetProperty("value", out JsonElement val);

            var value = val.GetString();

            if (value == null)
                return false;

            token = value;
            return true;
        }

        /// <summary>
        /// Read the version of SBM being interfaced
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, $"{m_address}/jsonapi/getversion");
            var result = m_client.SendAsync(postRequest).Result.Content.ReadAsStringAsync();

            var versionResponse = JsonDocument.Parse(result.Result);
            var version = versionResponse.RootElement.GetProperty("version").GetString();
            if (version == null)
                return string.Empty;
            return version;
        }

        /// <summary>
        /// Function to read the first 'range' items from the table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="range"></param>
        public List<WorkItem> ReadItems(int table, int range)
        {
            List<WorkItem> items = new List<WorkItem>();
            List<int> integerList = Enumerable.Range(0, range).ToList();
            Parallel.ForEach(integerList, i =>
            {
                try
                {
                    var getItemRequest = new HttpRequestMessage(HttpMethod.Post, $"{m_address}/jsonapi/getItem/{table}/{i}");
                    getItemRequest.Content = new StringContent("{fixedFields: false, includeNotes: true}");

                    var getItemResult = m_client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

                    var itemResponse = JsonDocument.Parse(getItemResult.Result);
                    var type = itemResponse.RootElement.GetProperty("result").GetProperty("type");
                    if (type.GetString() == "ERROR")
                        return;

                    WorkItem item = new WorkItem();
                    item.ParseFromJson(itemResponse.RootElement.GetProperty("item"), true);

                    lock (items)
                        items.Add(item);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            });

            return items;
        }

        /// <summary>
        /// Returns the names of all available transitions for a work item
        /// </summary>
        /// <param name="table">Table ID</param>
        /// <param name="id">ID of the work item</param>
        /// <returns></returns>
        public IEnumerable<string> GetItemTransitions(int table, int id)
        {
            var transitions = new List<string>();
            var getItemRequest = new HttpRequestMessage(HttpMethod.Post, $"{m_address}/jsonapi/getitemTransitions/{table}/{id}");

            var getItemResult = m_client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

            var itemResponse = JsonDocument.Parse(getItemResult.Result).RootElement;
            var type = itemResponse.GetProperty("result").GetProperty("type");
            if (type.GetString() == "ERROR")
                return transitions;

            var jsonItems = itemResponse.GetProperty("transitions").EnumerateArray();
            transitions.AddRange(jsonItems.Select(transition => transition.GetProperty("name").GetString()));

            return transitions;
        }

        /// <summary>
        /// Function to read all items from the table. Caused by paging issues, can only read the first 1000 items
        /// </summary>
        /// <param name="table"></param>
        public List<WorkItem> ReadAllItems(int table)
        {
            List<WorkItem> items = new List<WorkItem>();
            try
            {
                var getItemRequest = new HttpRequestMessage(HttpMethod.Post, $"{m_address}/workcenter/tmtrack.dll?JSONPage&command=jsonapi&JSON_Func=getitemsbyitemid&JSON_P1={table}&JSON_P2=*&pagesize=1000");
                getItemRequest.Content = new StringContent("{fixedFields: false, includeNotes: true}");

                var getItemResult = m_client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

                var itemResponse = JsonDocument.Parse(getItemResult.Result);
                var type = itemResponse.RootElement.GetProperty("result").GetProperty("type");
                if (type.GetString() == "ERROR")
                    return items;

                foreach (var item in itemResponse.RootElement.GetProperty("items").EnumerateArray())
                {
                    WorkItem workItem = new WorkItem();
                    workItem.ParseFromJson(item, true);
                    items.Add(workItem);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return items;
        }

        /// <summary>
        /// Read all items from a preconfigured report
        /// </summary>
        /// <param name="reportID"></param>
        public List<WorkItem> ReadItemsFromReport(int reportID)
        {
            List<WorkItem> items = new List<WorkItem>();
            try
            {
                int startID = 0;
                const int increment = 100;
                m_repeatKey++;
                while (true)
                {
                    var getItemRequest = new HttpRequestMessage(HttpMethod.Post, $"{m_address}/jsonapi/getitemsbylistingreport/{reportID}?pagesize={increment}&rptkey={m_repeatKey}&recno={startID}");
                    getItemRequest.Content = new StringContent("{fixedFields: false, includeNotes: true}");

                    var getItemResult = m_client.SendAsync(getItemRequest).Result.Content.ReadAsStringAsync();

                    var itemResponse = JsonDocument.Parse(getItemResult.Result).RootElement;
                    var type = itemResponse.GetProperty("result").GetProperty("type");
                    if (type.GetString() == "ERROR")
                        return items;

                    var jsonItems = itemResponse.GetProperty("items").EnumerateArray();
                    foreach (var item in jsonItems)
                    {
                        WorkItem workItem = new WorkItem();
                        workItem.ParseFromJson(item, true);
                        items.Add(workItem);
                    }

                    if (jsonItems.Count() < increment)
                        break;

                    startID += increment;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return items;
        }
    }
}
