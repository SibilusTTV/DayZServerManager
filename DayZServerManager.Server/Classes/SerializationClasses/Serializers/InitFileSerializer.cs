namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class InitFileSerializer
    {
        public static void SerializeInitFile(string path, string initFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(initFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
        public static string DeserializeInitFile(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return string.Empty;
            }
        }
    }
}
