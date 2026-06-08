using Microsoft.Maui.Storage;

namespace assessment2526.Services
{
    public static class AppSettingsService
    {
        // Auto-pause option
        private const string AutoPauseKey = "AutoPauseEnabled";

        public static bool AutoPauseEnabled
        {
            get => Preferences.Get(AutoPauseKey, true);
            set => Preferences.Set(AutoPauseKey, value);
        }

        // Font size options
        private const string FontSizeKey = "FontSizeOption";

        public static string FontSizeOption
        {
            get => Preferences.Get(FontSizeKey, "Medium");
            set => Preferences.Set(FontSizeKey, value);
        }

        // Unit of meausure options
        private const string DistanceUnitKey = "DistanceUnit";

        public static string DistanceUnit
        {
            get => Preferences.Get(DistanceUnitKey, "Kilometres");
            set => Preferences.Set(DistanceUnitKey, value);
        }
    }
}
