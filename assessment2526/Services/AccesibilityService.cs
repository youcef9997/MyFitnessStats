namespace assessment2526.Services;

public static class AccessibilityService
{
    public static void ApplyFontSize(string option)
    {
        double bodySize = option switch
        {
            "Small" => 14,
            "Medium" => 16,
            "Large" => 20,
            _ => 16
        };

        double titleSize = option switch
        {
            "Small" => 28,
            "Medium" => 32,
            "Large" => 36,
            _ => 32
        };

        if (Application.Current != null)
        {
            Application.Current.Resources["BodyFontSize"] = bodySize;
            Application.Current.Resources["TitleFontSize"] = titleSize;
        }
    }
}