using System;
using System.Text.Json;

namespace SBMAPIInterface
{
    public class WorkItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Submitter { get; set; }
        public string State { get; set; }
        public string Severity { get; set; }
        public string? Project { get; set; }
        public DateTime? SubmitDate { get; set; }

        public void ParseFromJson(JsonElement itemElement)
        {
            itemElement.GetProperty("id").TryGetProperty("itemId", out JsonElement valItem);
            ID = Convert.ToInt32(valItem.GetString());

            JsonElement fields = itemElement.GetProperty("fields");
            fields.TryGetProperty("TITLE", out valItem);
            Title = valItem.GetProperty("value").GetString();

            fields.TryGetProperty("SUBMITTER", out valItem);
            Submitter = valItem.GetProperty("name").GetString();

            fields.TryGetProperty("DESCRIPTION", out valItem);
            Description = valItem.GetProperty("value").GetString();

            fields.TryGetProperty("STATE", out valItem);
            State = valItem.GetProperty("value").GetString();

            fields.TryGetProperty("PROJECTID", out valItem);
            Project = valItem.GetProperty("name").GetString();

            fields.TryGetProperty("SEVERITY", out valItem);
            Severity = valItem.GetProperty("name").GetString();

            try
            {
                fields.TryGetProperty("SUBMITDATE", out valItem);
                SubmitDate = toDate(valItem.GetProperty("svalue").GetString());
            }
            catch (Exception e)
            {
                //TODO: Handle cases where datetime parsing fails - localization
                Console.WriteLine(e);
            }
        }

        private static DateTime? toDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "(None)" || s == "&nbsp;")
                return null;
            return DateTime.Parse(s);
        }
    }
}
