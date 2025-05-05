using Microsoft.VisualBasic.FileIO;
using System.Text.Json;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public class JSONSerializer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void SerializeJSONFile<JSONFile>(string path, JSONFile jsonfile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(jsonfile, options);
                    writer.Write(json);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when serializing json file", ex);
            }
        }

        // Takes a path and returns the deserialized class
        public static JSONFile? DeserializeJSONFile<JSONFile>(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<JSONFile>(json);
                    }
                }
                else
                {
                    return default(JSONFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when deserializing json file", ex);
                return default(JSONFile);
            }
        }
    }
}
