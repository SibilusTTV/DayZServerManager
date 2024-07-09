using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.GlobalsClasses;
using Microsoft.VisualBasic.FileIO;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class GlobalsFileSerializer
    {
        public static void SerializeGlobalsFile(string path, GlobalsFile globals)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GlobalsFile));
                    serializer.Serialize(writer, globals);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public static GlobalsFile? DeserializeGlobalsFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(GlobalsFile));
                        return (GlobalsFile?)serializer.Deserialize(reader);
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
