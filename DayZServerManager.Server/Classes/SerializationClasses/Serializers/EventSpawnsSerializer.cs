using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EventSpawnClasses;
using Microsoft.VisualBasic.FileIO;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class EventSpawnsSerializer
    {
        public static void SerializeEventSpawns(string path, EventSpawnsFile eventSpawnsFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                    serializer.Serialize(writer, eventSpawnsFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public static EventSpawnsFile? DesererializeEventSpawns(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                        return (EventSpawnsFile?)serializer.Deserialize(reader);
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
