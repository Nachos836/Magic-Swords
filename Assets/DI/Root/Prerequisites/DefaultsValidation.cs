namespace MagicSwords.DI.Root.Prerequisites
{
    internal static class DefaultsValidation
    {
        public static bool ConfigIsProvided(Defaults candidate) => candidate.Equals(default) is false;
    }
}
