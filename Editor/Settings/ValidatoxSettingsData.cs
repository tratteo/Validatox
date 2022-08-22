using Newtonsoft.Json;

namespace Validatox.Editor.Settings
{
    [System.Serializable]
    public class ValidatoxSettingsData
    {
        public const string Location = ".ox_settings.json";

        [JsonProperty("guard_validator_guid")]
        public string GuardValidatorGuid { get; set; }

        [JsonProperty("secure_build_pipeline")]
        public bool SecureBuildPipeline { get; set; }

        [JsonProperty("build_validate")]
        public bool BuildValidate { get; set; }

        [JsonProperty("build_validate_guards")]
        public bool BuildValidateGuards { get; set; }

        public ValidatoxSettingsData()
        {
            GuardValidatorGuid = string.Empty;
            SecureBuildPipeline = false;
            BuildValidate = true;
            BuildValidateGuards = true;
        }
    }
}