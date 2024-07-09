using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using Microsoft.VisualBasic.FileIO;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class SchedulerFileSerializer
    {
        public static void SerializeSchedulerFile(string path, SchedulerFile schedulerFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SchedulerFile));
                    serializer.Serialize(writer, schedulerFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }


        // Takes a path and returns the deserialized TypesFile
        public static SchedulerFile? DeserializeSchedulerFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                        return (SchedulerFile?)serializer.Deserialize(reader);
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
