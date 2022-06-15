using Newtonsoft.Json;

namespace Validatox.Editor.Settings
{
    [System.Serializable]
    public class ValidatoxSettingsData
    {
        public const string Location = ".ox_settings.json";

        [JsonProperty("guard_validator_guid")]
        public string GuardValidatorGuid { get; set; }

        public ValidatoxSettingsData()
        {
            GuardValidatorGuid = string.Empty;
        }
    }
}