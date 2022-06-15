using Newtonsoft.Json;
using System;
using System.IO;

namespace Validatox.Editor.Settings
{
    public class ValidatoxSettings
    {
        public static void Edit(Action<ValidatoxSettingsData> editor)
        {
            var data = Load();
            editor?.Invoke(data);
            Save(data);
        }

        internal static void Save(ValidatoxSettingsData data)
        {
            if (!File.Exists(ValidatoxSettingsData.Location))
            {
                var dir = Path.GetDirectoryName(ValidatoxSettingsData.Location);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            File.WriteAllText(ValidatoxSettingsData.Location, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        internal static ValidatoxSettingsData Load()
        {
            if (!File.Exists(ValidatoxSettingsData.Location))
            {
                var data = new ValidatoxSettingsData();
                Save(data);
                return data;
            }
            return JsonConvert.DeserializeObject<ValidatoxSettingsData>(File.ReadAllText(ValidatoxSettingsData.Location));
        }
    }
}