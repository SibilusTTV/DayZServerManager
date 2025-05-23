﻿namespace DayZServerManager.Server.Classes.SerializationClasses.Serializers
{
    public static class InitFileSerializer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
                Logger.Error(ex, "Error when serializing init file");
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
                Logger.Error(ex, "Error when deserializing init file");
                return string.Empty;
            }
        }
    }
}
