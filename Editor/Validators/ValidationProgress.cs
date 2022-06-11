namespace Validatox.Editor.Validators
{
    public class ValidationProgress
    {
        public string Phase { get; private set; }

        public string Description { get; private set; }

        public float ProgressValue { get; private set; }

        public ValidationProgress(string phase, string description) : this(phase, description, 0)
        {
        }

        public ValidationProgress(string phase, string description, float progressValue)
        {
            Phase = phase;
            Description = description;
            ProgressValue = progressValue;
        }

        public ValidationProgress At(string phase)
        {
            Phase = phase;
            return this;
        }

        public ValidationProgress Doing(string description)
        {
            Description = description;
            return this;
        }

        public ValidationProgress WithProgress(float progressValue)
        {
            ProgressValue = progressValue;
            return this;
        }
    }
}