using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using Microsoft.VisualBasic.FileIO;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class TypesFileSerializer
    {
        public static void SerializeTypesFile(string path, TypesFile typesFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                    serializer.Serialize(writer, typesFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        // Takes a path and returns the deserialized TypesFile
        public static TypesFile? DeserializeTypesFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                        return (TypesFile?)serializer.Deserialize(reader);
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
