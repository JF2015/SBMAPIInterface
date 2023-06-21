using System;
using System.Text.Json;

namespace SBMAPIInterface
{
    public class WorkItem
    {
        //Fixed Fields
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Submitter { get; set; }
        public string Owner { get; set; }
        public string SecondaryOwner { get; set; }
        public string LastModifier { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string? Project { get; set; }
        public DateTime? SubmitDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string Link { get; set; }
        public bool IsActive { get; set; }

        //Custom Fields - these fields have to be adapted when used with different databases
        public string Severity { get; set; }
        public string SoftwareEngineer { get; set; }
        public string QAEngineer { get; set; }
        public string DevPhase { get; set; }
        public DateTime? CloseDate { get; set; }

        public void ParseFromJson(JsonElement itemElement, bool parseCustomFields)
        {
            itemElement.GetProperty("id").TryGetProperty("itemId", out JsonElement valItem);
            ID = Convert.ToInt32(valItem.GetString());

            itemElement.GetProperty("id").TryGetProperty("url", out valItem);
            Link = valItem.GetString();

            JsonElement fields = itemElement.GetProperty("fields");
            fields.TryGetProperty("TITLE", out valItem);
            Title = valItem.GetProperty("value").GetString();

            fields.TryGetProperty("SUBMITTER", out valItem);
            Submitter = valItem.GetProperty("name").GetString();

            fields.TryGetProperty("DESCRIPTION", out valItem);
            Description = valItem.GetProperty("value").GetString();

            fields.TryGetProperty("STATE", out valItem);
            State = valItem.GetProperty("value").GetString();

            fields.TryGetProperty("ACTIVEINACTIVE", out valItem);
            IsActive = valItem.GetProperty("name").GetString() != "Inactive";

            fields.TryGetProperty("ISSUETYPE", out valItem);
            Type = valItem.GetProperty("name").GetString();

            fields.TryGetProperty("PROJECTID", out valItem);
            Project = valItem.GetProperty("name").GetString();

            fields.TryGetProperty("OWNER", out valItem);
            Owner = valItem.GetProperty("name").GetString();

            fields.TryGetProperty("SECONDARYOWNER", out valItem);

            if (valItem.TryGetProperty("name", out var secondaryOwner))
                SecondaryOwner = secondaryOwner.GetString();

            fields.TryGetProperty("LASTMODIFIER", out valItem);
            LastModifier = valItem.GetProperty("name").GetString();

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

            try
            {
                fields.TryGetProperty("LASTMODIFIEDDATE", out valItem);
                LastModified = toDate(valItem.GetProperty("svalue").GetString());
            }
            catch (Exception e)
            {
                //TODO: Handle cases where datetime parsing fails - localization
                Console.WriteLine(e);
            }

            if (parseCustomFields)
            {
                fields.TryGetProperty("SEVERITY", out valItem);
                Severity = valItem.GetProperty("name").GetString();

                fields.TryGetProperty("SWE", out valItem);
                SoftwareEngineer = valItem.GetProperty("name").GetString();

                fields.TryGetProperty("SQA", out valItem);
                QAEngineer = valItem.GetProperty("name").GetString();

                fields.TryGetProperty("DEVPHASE", out valItem);
                DevPhase = valItem.GetProperty("name").GetString();

                try
                {
                    fields.TryGetProperty("CLOSEDATE", out valItem);
                    CloseDate = toDate(valItem.GetProperty("svalue").GetString());
                }
                catch (Exception e)
                {
                    //TODO: Handle cases where datetime parsing fails - localization
                    Console.WriteLine(e);
                }
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
