using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assessment2526.Services
{
    public static class ThemeService
    {
        public static void ApplyTheme(string theme)
        {
            if (Application.Current == null)
                return;

            switch (theme)
            {
                case "Light":
                    Application.Current.UserAppTheme = AppTheme.Light;
                    break;

                case "Dark":
                    Application.Current.UserAppTheme = AppTheme.Dark;
                    break;

                default:
                    Application.Current.UserAppTheme = AppTheme.Unspecified;
                    break;
            }
        }

        public static string GetCurrentTheme()
        {
            if (Application.Current == null)
                return "System";

            return Application.Current.UserAppTheme switch
            {
                AppTheme.Light => "Light",
                AppTheme.Dark => "Dark",
                _ => "System"
            };
        }
    }
}
