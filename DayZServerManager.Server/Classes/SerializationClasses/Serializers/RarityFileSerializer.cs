using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using Microsoft.VisualBasic.FileIO;
using System.Text.Json;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class RarityFileSerializer
    {
        public static void SerializeRarityFile(string path, RarityFile rarityFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(rarityFile, options);
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
        public static RarityFile? DeserializeRarityFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<RarityFile>(json);
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
