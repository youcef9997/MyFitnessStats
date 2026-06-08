namespace assessment2526.ViewModels;


using assessment2526.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public partial class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string name = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<string> Themes { get; } =
        new() { "System", "Light", "Dark" };

    private string _selectedTheme = "System"; 

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                OnPropertyChanged();
                ApplyTheme();
            }
        }
    }

    public ObservableCollection<string> FontSizeOptions { get; } =
    new() { "Small", "Medium", "Large" };

    private string _selectedFontSize = AppSettingsService.FontSizeOption;

    public string SelectedFontSize
    {
        get => _selectedFontSize;
        set
        {
            if (_selectedFontSize != value)
            {
                _selectedFontSize = value;
                AppSettingsService.FontSizeOption = value;
                AccessibilityService.ApplyFontSize(value);
                OnPropertyChanged();
            }
        }
    }



    public SettingsViewModel()
    {
        SelectedTheme = ThemeService.GetCurrentTheme();

        SelectedFontSize = AppSettingsService.FontSizeOption;
        AccessibilityService.ApplyFontSize(SelectedFontSize);

        SelectedDistanceUnit = AppSettingsService.DistanceUnit;
    }

    private bool _isAutoPauseEnabled = AppSettingsService.AutoPauseEnabled;

    public bool IsAutoPauseEnabled
    {
        get => _isAutoPauseEnabled;
        set
        {
            if (_isAutoPauseEnabled != value)
            {
                _isAutoPauseEnabled = value;
                AppSettingsService.AutoPauseEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    private void ApplyTheme()
    {
        ThemeService.ApplyTheme(SelectedTheme);
    }

    public ObservableCollection<string> DistanceUnits { get; } =
        new() { "Kilometres", "Miles" };

    private string _selectedDistanceUnit = AppSettingsService.DistanceUnit;

    public string SelectedDistanceUnit
    {
        get => _selectedDistanceUnit;
        set
        {
            if (_selectedDistanceUnit != value)
            {
                _selectedDistanceUnit = value;
                AppSettingsService.DistanceUnit = value;
                OnPropertyChanged();
            }
        }
    }
}