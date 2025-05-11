using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public class ServerConfigSerializer
    {
        public static ServerConfig Deserialize(string config)
        {
            ServerConfig cfg = new ServerConfig();

            string pattern = "[^\\n\\w]*(?'propertyName'[a-zA-Z0-9\\[\\]]+)\\s*=\\s*(?:\"(?'stringValue'[^\\n\"]*)\"|(?'numberValue'-?[0-9]+)|(?'decimalValue'-?[0-9]+\\.[0-9]+)|(?'boolValue'[Tt]rue|[Ff]alse)|(?:\\{\\s*(?'arrayValue'\"[^\\n\"]*\"(,\\s*\"[^\\n\"]*\")*)\\s*\\}));(?:[^\\S\\n]*\\/\\/(?'comment'[^\\n]*))?";
            Regex reg = new Regex(pattern);
            MatchCollection matches = reg.Matches(config);

            foreach (Match match in matches)
            {
                string propertyName = match.Groups["propertyName"].Value;
                string comment = "";
                if (match.Groups["comment"].Success)
                {
                    comment = match.Groups["comment"].Value.Trim();
                }

                if (match.Groups["stringValue"].Success)
                {
                    string propertyValue = match.Groups["stringValue"].Value;
                    switch (propertyName)
                    {
                        case "hostname":
                            cfg.Properties.Add(new PropertyValue(cfg.GetNextID(), propertyName, DataType.TextShort, propertyValue, comment));
                            break;
                        case "description":
                            cfg.Properties.Add(new PropertyValue(cfg.GetNextID(), propertyName, DataType.TextLong, propertyValue, comment));
                            break;
                        default:
                            cfg.Properties.Add(new PropertyValue(cfg.GetNextID(), propertyName, DataType.Text, propertyValue, comment));
                            break;
                    }
                }
                else if (match.Groups["boolValue"].Success && bool.TryParse(match.Groups["boolValue"].Value, out bool boolValue))
                {
                    cfg.Properties.Add(new PropertyValue(cfg.GetNextID(), propertyName, DataType.Boolean, boolValue, comment));
                }
                else if (match.Groups["numberValue"].Success && int.TryParse(match.Groups["numberValue"].Value, out int numberValue))
                {
                    cfg.Properties.Add(new PropertyValue(cfg.GetNextID(), propertyName, DataType.Number, numberValue, comment));
                }
                else if (match.Groups["decimalValue"].Success && float.TryParse(match.Groups["decimalValue"].Value, CultureInfo.InvariantCulture, out float decimalValue))
                {
                    cfg.Properties.Add(new PropertyValue(cfg.GetNextID(), propertyName, DataType.Decimal, decimalValue, comment));
                }
                else if (match.Groups["arrayValue"].Success)
                {
                    string[] stringArray = match.Groups["arrayValue"].Value.Split(",");
                    List<string> stringList = new List<string>();
                    foreach (string line in stringArray)
                    {
                        stringList.Add(line.TrimEnd('\"').TrimStart('\"'));
                    }
                    cfg.Properties.Add(new PropertyValue(cfg.GetNextID(), propertyName, DataType.Array, stringList, comment));
                }
            }

            if (cfg.Properties.Count == 0)
            {
                cfg.SetDefaultValues();
            }

            return cfg;
        }

        public static string Serialize(ServerConfig cfg)
        {
            string returnString = "";

            if (cfg != null)
            {
                foreach (PropertyValue property in cfg.Properties)
                {
                    if (property.PropertyName != "template")
                    {
                        if (property.DataType == DataType.Array)
                        {
                            returnString += $"{Environment.NewLine}{property.PropertyName} = {{";
                            foreach (string line in (List<string>)property.Value)
                            {
                                returnString += $"\"{line}\",";
                            }
                            returnString = returnString.TrimEnd(',');
                            returnString += $"}};";
                        }
                        else if (property.DataType == DataType.TextShort || property.DataType== DataType.Text || property.DataType == DataType.TextLong)
                        {
                            returnString += $"{Environment.NewLine}{property.PropertyName} = \"{property.Value}\";";
                        }
                        else if (property.DataType == DataType.Decimal)
                        {
                            returnString += $"{Environment.NewLine}{property.PropertyName} = {((float)property.Value).ToString(CultureInfo.InvariantCulture)};";
                        }
                        else
                        {
                            returnString += $"{Environment.NewLine}{property.PropertyName} = {property.Value};";
                        }

                        if (!string.IsNullOrEmpty(property.Comment))
                        {
                            returnString += $" // {property.Comment}";
                        }
                    }
                    else
                    {
                        returnString += Environment.NewLine;
                        returnString += Environment.NewLine;
                        returnString += $"{Environment.NewLine}class Missions";
                        returnString += $"{Environment.NewLine}{{";
                        returnString += $"{Environment.NewLine}    class DayZ";
                        returnString += $"{Environment.NewLine}    {{";
                        returnString += $"{Environment.NewLine}        template = \"{property.Value}\";";
                        if (!string.IsNullOrEmpty(property.Comment))
                        {
                            returnString += $" // {property.Comment}";
                        }
                        returnString += $"{Environment.NewLine}    }};";
                        returnString += $"{Environment.NewLine}}};";
                        returnString += Environment.NewLine;
                        returnString += Environment.NewLine;
                    }
                }
            }

            return returnString;
        }
    }
}
