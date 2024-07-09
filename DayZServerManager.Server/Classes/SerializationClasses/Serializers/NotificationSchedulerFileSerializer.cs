using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using Microsoft.VisualBasic.FileIO;
using System.Text.Json;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class NotificationSchedulerFileSerializer
    {
        public static void SerializeNotificationSchedulerFile(string path, NotificationSchedulerFile schedulerFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(schedulerFile, options);
                    writer.Write(json);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }


        // Takes a path and returns the deserialized RarityFile
        public static NotificationSchedulerFile? DeserializeNotificationSchedulerFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<NotificationSchedulerFile>(json);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return null;
            }
        }
    }
}
