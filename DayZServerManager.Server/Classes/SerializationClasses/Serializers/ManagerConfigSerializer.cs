using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using System.Text.Json;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class ManagerConfigSerializer
    {
        public static ManagerConfig? DeserializeManagerConfig()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Manager.MANAGERCONFIGNAME))
                {
                    string json = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<ManagerConfig>(json);
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return null;
            }
        }

        public static void SerializeManagerConfig(ManagerConfig config)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Manager.MANAGERCONFIGNAME))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(config, options);
                    writer.Write(json);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
    }
}
