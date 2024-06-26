using Newtonsoft.Json;
using LabFusion.Utilities;

namespace LabFusion.Data
{
    public static class DataSaver
    {
        public static void WriteJson(string path, object value)
        {
            string fullPath = PersistentData.GetPath(path);
            string directoryName = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            string jsonText = JsonConvert.SerializeObject(value, Formatting.Indented);

            File.WriteAllText(fullPath, jsonText);
        }

        public static T ReadJson<T>(string path)
        {
            string fullPath = PersistentData.GetPath(path);

            if (!File.Exists(fullPath))
                return default;

            string jsonText;

            try
            {
                jsonText = File.ReadAllText(fullPath);
            }
            catch (UnauthorizedAccessException e)
            {
                FusionLogger.LogException($"reading save data at {path}", e);
                return default;
            }

            try
            {
                T obj = (T)JsonConvert.DeserializeObject(jsonText);
                return obj;
            }
            catch
            {
                File.Delete(fullPath);
                return default;
            }
        }
    }
}
