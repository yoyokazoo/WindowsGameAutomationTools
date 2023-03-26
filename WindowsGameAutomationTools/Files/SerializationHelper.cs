using System.IO;
using System.Text.Json;

namespace WindowsGameAutomationTools.Files
{
    public static class SerializationHelper
    {
        public const string JSON_SUFFIX = ".json";

        public static void SerializeToJSON<TValue>(string fileName, TValue objectToBeSerialized)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;

            using (StreamWriter stream = new StreamWriter($"{fileName}{JSON_SUFFIX}"))
            {
                string jsonString = JsonSerializer.Serialize(objectToBeSerialized, options);
                stream.Write(jsonString);
            }
        }

        public static TValue DeserializeFromJSON<TValue>(string fileName)
        {
            using (StreamReader stream = new StreamReader($"{fileName}{JSON_SUFFIX}"))
            {
                TValue fileReadResult = JsonSerializer.Deserialize<TValue>(stream.ReadToEnd());
                return fileReadResult;
            }
        }
    }
}
