using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesChangesClasses;
using Microsoft.VisualBasic.FileIO;
using System.Text.Json;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class TypesChangesFileSerializer
    {
        public static void SerializeTypesChangesFile(string path, TypesChangesFile typesChangesFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(typesChangesFile, options);
                    writer.Write(json);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }

        }

        public static TypesChangesFile? DeserializeTypesChangesFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<TypesChangesFile>(json);
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
