using Newtonsoft.Json;
using System.IO;

namespace HumanResources.Utilities
{
    class JsonUtil
    {
        public static bool TryRead<T>(string path, out T data)
        {
            data = default(T);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                data = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            return false;
        }

        public static bool TryWrite<T>(string path, T data, Formatting format = Formatting.None)
        {
            var json = JsonConvert.SerializeObject(data, format);
            File.WriteAllText(path, json);
            return json.Length > 0;
        }
    }
}
