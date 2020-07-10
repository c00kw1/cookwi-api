using Newtonsoft.Json;
using System;
using System.IO;

namespace Api.Hosting.Settings
{
    public abstract class SettingsBase<T>
    {
        public static T Get(string path)
        {
            if (path == null || path == "")
            {
                var status = path == null ? "null" : "empty";
                throw new NullReferenceException($"DB settings path is {status}");
            }
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                throw new ArgumentException("DB settings path given is not valid, cannot find the settings");
            }
            if (file.Extension != ".json")
            {
                throw new ArgumentException($"DB settings must be a json file. Given file : {path}");
            }

            using (var s = new StreamReader(path))
            {
                var content = s.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}
