using System.Globalization;
using System.Text.RegularExpressions;
using Application.IRepository;
using Application.IService;
using Domain.ServerConfig;

namespace Infrastructure.Repository;

public class ServerConfigSerializerRepository : IServerConfigSerializerRepository
{
    public ServerConfig Deserialize(string config)
    {
        var cfg = new ServerConfig();

        const string pattern = @"[^\n\w\""]*(?'propertyName'[a-zA-Z0-9\[\]]+)\s*=\s*(?'value'((\""[^\""\n]*\"")|([0-9]+\.[0-9]+)|([0-9]+)|([Ff]alse|[Tt]rue)|(\{(\""[^\n\""]*\""(,\""[^\n\""]*\""))?\})));(\s*\/\/\s*(?'comment'[^\n]*))?";
        var reg = new Regex(pattern);
        var matches = reg.Matches(config);

        foreach (Match match in matches)
        {
            string propertyName = match.Groups["propertyName"].Value;
            string comment = "";
            if (match.Groups["comment"].Success)
            {
                comment = match.Groups["comment"].Value.Trim();
            }

            if (match.Groups["value"].Success)
            {
                cfg.Properties.Add(new PropertyValue(propertyName, match.Groups["stringValue"].Value, comment));
            }
        }

        if (cfg.Properties.Count == 0)
        {
            cfg.SetDefaultValues();
        }

        return cfg;
    }

    public string Serialize(ServerConfig cfg)
    {
        string returnString = "";

        foreach (PropertyValue property in cfg.Properties)
        {
            if (property.PropertyName != "template")
            {
                returnString += $"{Environment.NewLine}{property.PropertyName} = {property.Value};";

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

        return returnString;
    }
}