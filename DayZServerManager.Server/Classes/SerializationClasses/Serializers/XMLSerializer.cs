using Microsoft.VisualBasic.FileIO;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public class XMLSerializer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void SerializeXMLFile<XMLFile>(string path, XMLFile? xmlFile)
        {
            try
            {
                if (xmlFile != null)
                {
                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(XMLFile));
                        serializer.Serialize(writer, xmlFile);
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when serializing xml file");
            }
        }


        // Takes a path and returns the deserialized TypesFile
        public static XMLFile? DeserializeXMLFile<XMLFile>(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(XMLFile));
                        return (XMLFile?)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return default(XMLFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when deserializing xml file");
                return default(XMLFile);
            }
        }
    }
}
