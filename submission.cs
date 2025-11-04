using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Text;


/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/

namespace ConsoleApp1
{
    public class Program
    {
        public static string xmlURL = "https://nttoma.github.io/CSE445-Assignment4/Hotels.xml";
        public static string xmlErrorURL = "https://nttoma.github.io/CSE445-Assignment4/HotelsErrors.xml";
        public static string xsdURL = "https://nttoma.github.io/CSE445-Assignment4/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            //return "No Error" if XML is valid. Otherwise, return the desired exception message.
            var sb = new StringBuilder();
            var settings = new XmlReaderSettings();

            using (var client = new WebClient())
            {
                var xsdContent = client.DownloadString(xsdUrl);
                using (var schemaReader = XmlReader.Create(new StringReader(xsdContent)))
                {
                    var schema = XmlSchema.Read(schemaReader, null);
                    var schemas = new XmlSchemaSet();
                    schemas.Add(schema);
                    settings.Schemas = schemas;
                    settings.ValidationType = ValidationType.Schema;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                    settings.ValidationEventHandler += (sender, e) =>
                    {
                        sb.AppendLine($"[{e.Severity}] {e.Message}");
                    };
                }

                var xmlContent = client.DownloadString(xmlUrl);
                using (var reader = XmlReader.Create(new StringReader(xmlContent), settings))
                {
                    try
                    {
                        while (reader.Read()) { }
                    }
                    catch (XmlException ex)
                    {
                        sb.AppendLine($"[Fatal] {ex.Message}");
                    }
                }
            }

            string output = sb.ToString().Trim();
            return string.IsNullOrEmpty(output) ? "No errors are found" : output;
        }

        public static string Xml2Json(string xmlUrl)
        {
            // The returned jsonText needs to be de-serializable by Newtonsoft.Json package. (JsonConvert.DeserializeXmlNode(jsonText))
            string xmlData;
            using (var client = new WebClient()) xmlData = client.DownloadString(xmlUrl);

            var doc = new XmlDocument();
            doc.LoadXml(xmlData);

            var hotels = doc.GetElementsByTagName("Hotel");
            var hotelArray = new JArray();

            foreach (XmlNode h in hotels)
            {
                var obj = new JObject();

                var name = h.SelectSingleNode("Name");
                if (name != null) obj["Name"] = name.InnerText;

                var phones = h.SelectNodes("Phone");
                var phoneArray = new JArray();
                foreach (XmlNode p in phones) phoneArray.Add(p.InnerText);
                obj["Phone"] = phoneArray;

                var addrNode = h.SelectSingleNode("Address");
                if (addrNode != null)
                {
                    var addr = new JObject
                    {
                        ["Number"] = addrNode.Attributes?["Number"]?.Value ?? "",
                        ["Street"] = addrNode.SelectSingleNode("Street")?.InnerText ?? "",
                        ["City"] = addrNode.SelectSingleNode("City")?.InnerText ?? "",
                        ["State"] = addrNode.SelectSingleNode("State")?.InnerText ?? "",
                        ["Zip"] = addrNode.SelectSingleNode("Zip")?.InnerText ?? "",
                        ["NearestAirport"] = addrNode.SelectSingleNode("NearestAirport")?.InnerText ?? ""
                    };
                    obj["Address"] = addr;
                }

                var rating = h.Attributes?["Rating"];
                if (rating != null) obj["_Rating"] = rating.Value;

                hotelArray.Add(obj);
            }

            var root = new JObject(new JProperty("Hotels", new JObject(new JProperty("Hotel", hotelArray))));
            string jsonText = JsonConvert.SerializeObject(root, Formatting.Indented);

            JsonConvert.DeserializeXmlNode(jsonText); // validation check
            return jsonText;
        }
    }
}
