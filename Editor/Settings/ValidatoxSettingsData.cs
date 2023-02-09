using Newtonsoft.Json;
using UnityEngine;

namespace Validatox.Editor.Settings
{
    [System.Serializable]
    public class ValidatoxSettingsData
    {
        public const string Location = ".ox_settings.json";
        public static readonly string DefaultLogColor = new Color(0.8F, 0.8F, 0.8F).ToHex();
        public static readonly string DefaultWarningColor = new Color(0.85F, 0.70F, 0.2F).ToHex();
        public static readonly string DefaultErrorColor = new Color(0.75F, 0.225F, 0.3F).ToHex();
        public static readonly string DefaultSuccessColor = new Color(0.28F, 0.8F, 0.25F).ToHex();
        public static readonly int DefaultFontSize = 12;

        [JsonProperty("guard_validator_guid")]
        public string GuardValidatorGuid { get; set; }

        [JsonProperty("secure_build_pipeline")]
        public bool SecureBuildPipeline { get; set; }

        [JsonProperty("build_validate")]
        public bool BuildValidate { get; set; }

        [JsonProperty("build_validate_guards")]
        public bool BuildValidateGuards { get; set; }

        [JsonProperty("warning_color")]
        public string WarningColor { get; set; }

        [JsonProperty("error_color")]
        public string ErrorColor { get; set; }

        [JsonProperty("success_color")]
        public string SuccessColor { get; set; }

        [JsonProperty("log_color")]
        public string LogColor { get; set; }

        [JsonProperty("log_font_size")]
        public int LogFontSize { get; set; }

        public ValidatoxSettingsData()
        {
            GuardValidatorGuid = string.Empty;
            SecureBuildPipeline = false;
            BuildValidate = true;
            BuildValidateGuards = true;

            LogColor = DefaultLogColor;
            WarningColor = DefaultWarningColor;
            ErrorColor = DefaultErrorColor;
            SuccessColor = DefaultSuccessColor;

            LogFontSize = DefaultFontSize;
        }
    }
}