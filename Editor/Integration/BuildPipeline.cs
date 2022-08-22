using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Validatox.Editor.Settings;

namespace Validatox.Editor
{
    public class BuildPipeline : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = ValidatoxSettings.Load();
            if (!settings.SecureBuildPipeline) return;
            if (settings.BuildValidate)
            {
                var res = ValidatoxManager.ValidateGroupsAndSingles();
                if (!res)
                {
                    throw new BuildFailedException("[Validatox] Secure build pipeline interrupted the build. Validation failed, use the Logs window for more information");
                }
            }
            if (settings.BuildValidateGuards)
            {
                var res = ValidatoxManager.ValidateGuarded();
                if (!res)
                {
                    throw new BuildFailedException("[Validatox] Secure build pipeline interrupted the build. Validation failed, use the Logs window for more information");
                }
            }
        }
    }
}