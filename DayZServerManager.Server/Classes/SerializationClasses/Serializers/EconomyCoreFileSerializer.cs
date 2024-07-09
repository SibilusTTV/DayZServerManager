using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses;
using Microsoft.VisualBasic.FileIO;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class EconomyCoreFileSerializer
    {
        public static void SerializeEconomyCoreFile(string path, EconomyCoreFile economyCoreFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                    serializer.Serialize(writer, economyCoreFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public static EconomyCoreFile? DeserializeEconomyCoreFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                        return (EconomyCoreFile?)serializer.Deserialize(reader);
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
