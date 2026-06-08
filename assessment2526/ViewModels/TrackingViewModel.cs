using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using assessment2526.Models;
using assessment2526.Services;

namespace assessment2526.ViewModels;

public partial class TrackingViewModel : INotifyPropertyChanged
{
    //Fields
    private bool _isRunning = false;
    private int _seconds = 0;

    //Location fields
    private double _latitude;
    private double _longitude;

    //Live disance and speed updates
    private Location? _lastLocation;
    private double _totalDistanceKm = 0;

    //Connectivity and geolocation
    private readonly IConnectivity _connectivity;
    private readonly IGeolocation _geolocation;

    //Accelerometer fields
    private bool _isAccelerometerRunning = false;
    private bool _isPaused = false;
    private int _noMovementSeconds = 0;

    //Seconds of no movement to consider pause
    private const int PauseThreshold = 5;
    private bool _timerRunning = false;

    //Compass fields
    private bool _isCompassRunning = false;

    //Live tracking on map
    private bool _locationTimerRunning = false;


    //Properties
    private string _timeText = "Time: 00:00:00";
    public string TimeText
    {
        get => _timeText;
        set { _timeText = value; OnPropertyChanged(); }
    }


    private string _distanceText = "Distance: 0.00 km";
    public string DistanceText
    {
        get => _distanceText;
        set { _distanceText = value; OnPropertyChanged(); }
    }


    private string _speedText = "Speed: 0.0 km/h";
    public string SpeedText
    {
        get => _speedText;
        set { _speedText = value; OnPropertyChanged(); }
    }

        
    private string _statusText = "Status: Waiting to start...";
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    
    public double Latitude
    {
        get => _latitude;
        set { _latitude = value; OnPropertyChanged(); }
    }

    
    public double Longitude
    {
        get => _longitude;
        set { _longitude = value; OnPropertyChanged(); }
    }

    
    private bool _isStatsVisible = true;
    public bool IsStatsVisible
    {
        get => _isStatsVisible;
        set { _isStatsVisible = value; OnPropertyChanged(); }
    }

    
    private bool _isMapVisible = false;
    public bool IsMapVisible
    {
        get => _isMapVisible;
        set { _isMapVisible = value; OnPropertyChanged(); }
    }
    public Location? CurrentLocation
    { 
        get;
        set;
    }

    //view switching properties
    private void ShowStatsView()
    {
        IsStatsVisible = true;
        IsMapVisible = false;
        IsCompassVisible = false;
    }

    private void ShowMapView()
    {
        IsStatsVisible = false;
        IsMapVisible = true;
        IsCompassVisible = false;
    }

    private void ShowCompassView()
    {
        IsStatsVisible = false;
        IsMapVisible = false;
        IsCompassVisible = true;
    }

    //Compass feature
    private string _compassHeadingText = "Heading: 0°";
    public string CompassHeadingText
    {
        get => _compassHeadingText;
        set
        {
            _compassHeadingText = value;
            OnPropertyChanged();
        }
    }

    private string _compassDirectionText = "Direction: Waiting...";
    public string CompassDirectionText
    {
        get => _compassDirectionText;
        set
        {
            _compassDirectionText = value;
            OnPropertyChanged();
        }
    }

    private bool _isCompassVisible = false;
    public bool IsCompassVisible
    {
        get => _isCompassVisible;
        set
        {
            _isCompassVisible = value;
            OnPropertyChanged();
        }
    }

    //Route event for live tracking on map
    public event Action<Location>? RouteLocationAdded;
    public event Action? RouteResetRequested;


    //Commands for start and stop buttons
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    //Command for pause buttor
    public ICommand PauseResumeCommand { get; }
    
    //Commands for view switching
    public ICommand ShowStatsCommand { get; }
    public ICommand ShowMapCommand { get; }
    public ICommand ShowCompassCommand { get; }

    //Gesture commands for swiping between views
    public ICommand NextViewCommand { get; }
    public ICommand PreviousViewCommand { get; }

    public TrackingViewModel(IConnectivity connectivity, IGeolocation geolocation)
    {
        StartCommand = new Command(async () => await StartRun());
        StopCommand = new Command(async () => await StopRun());
        PauseResumeCommand = new Command(TogglePause);
        ShowStatsCommand = new Command(ShowStatsView);
        ShowMapCommand = new Command(ShowMapView);
        ShowCompassCommand = new Command(ShowCompassView);
        NextViewCommand = new Command(ShowNextView);
        PreviousViewCommand = new Command(ShowPreviousView);

        _connectivity = connectivity;
        _geolocation = geolocation;
        LoadLocation();
    }

    //Start run
    private async Task StartRun()
    {
        if (_isRunning)
            return;

        var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (permission != PermissionStatus.Granted)
        {
            permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (permission != PermissionStatus.Granted)
        {
            StatusText = "Status: Cannot start without location permission";
            await ShowErrorMessage("Location permission is required to start tracking your run.");
            return;
        }

        _isRunning = true;
        _isPaused = false;
       
        _lastLocation = null;
        _totalDistanceKm = 0;

        UpdateDistanceAndSpeedText();

        StatusText = "Status: Running...";
        PauseButtonText = "Pause Run";

        RouteResetRequested?.Invoke();

        Vibrate();
        StartTimer();
        StartAccelerometer();
        StartCompass();

        //Live tracking
        StartLocationTracking();

        await SpeakAsync("Run started");
        //when distance updates text-to-speech to user
        await SpeakAsync($"Distance {DistanceText}");
    }

    //Stop run
    private async Task StopRun()
    {
        if (!_isRunning)
            return;
        
        _isRunning = false;
        _isPaused = false;

        StatusText = "Status: Stopped";
        PauseButtonText = "Pause Run";

        //stronger haptic feedback on stop
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(600));

        await SpeakAsync("Run stopped");

        StopAccelerometer();
        StopCompass();

        //save session stats to history
        HistoryService.Runs.Add(new SessionStats
        {
            Time = TimeText,
            Distance = DistanceText,
            Speed = SpeedText,
            Date = DateTime.Now
        });

        //display stats summary
        var page = Application.Current?.Windows[0]?.Page;

        if (page != null)
        {
            await page.DisplayAlert(
                "🏃 Run Summary",
                $"⏱ Time: {TimeText.Replace("Time: ", "")}\n" +
                $"📍 Distance: {DistanceText.Replace("Distance: ", "")}\n" +
                $"⚡ Speed: {SpeedText.Replace("Speed: ", "")}",
                "OK");
        }
    }

    //Pause button
    private string _pauseButtonText = "Pause Run";

    public string PauseButtonText
    {
        get => _pauseButtonText;
        set
        {
            _pauseButtonText = value;
            OnPropertyChanged();
        }
    }

    //Start run validation and error handling 
    private async Task ShowErrorMessage(string message)
    {
        var page = Application.Current?.Windows[0]?.Page;

        if (page == null)
            return;

        await page.DisplayAlert("Error", message, "OK");
    }


    //Toggle pause/resume
    private void TogglePause()
    {
        if (!_isRunning)
            return;

        _isPaused = !_isPaused;

        if (_isPaused)
        {
            StatusText = "Status: Paused";
            PauseButtonText = "Resume Run";
            Vibrate();
        }
        else
        {
            StatusText = "Status: Running...";
            PauseButtonText = "Pause Run";
            Vibrate();
        }
    }

    //Timer logic
    private void StartTimer()
    {
        if (_timerRunning)
            return; 

        _timerRunning = true;

        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null)
            return;

        dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            
            if (!_isRunning)
            {
                _timerRunning = false;
                return false;
            }

            //pause logic
            if (_isPaused)
                return true; //keep timer alive but DO NOT increment

            _seconds++;

            TimeText = $"Time: {TimeSpan.FromSeconds(_seconds):hh\\:mm\\:ss}";

            UpdateDistanceAndSpeedText();

            return true;
        });
    }

    //Gesture control
    private void ShowNextView()
    {
        if (IsStatsVisible)
        {
            ShowMapView();
        }
        else if (IsMapVisible)
        {
            ShowCompassView();
        }
        else
        {
            ShowStatsView();
        }
    }

    private void ShowPreviousView()
    {
        if (IsStatsVisible)
        {
            ShowCompassView();
        }
        else if (IsCompassVisible)
        {
            ShowMapView();
        }
        else
        {
            ShowStatsView();
        }
    }

    //Location with connectivity check
    private async void LoadLocation()
    {   
        //location permission request
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            StatusText = "Location permission denied";
            return;
        }
        try
        {
            //check network connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert(
                    "No connectivity!",
                    "Please check internet and try again.",
                    "OK");

                StatusText = "No internet connection";
                return;
            }

            //get last known location first
            var location = await _geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await _geolocation.GetLocationAsync(
                    new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(10)
                    });
            }

            if (location != null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;
                CurrentLocation = location;
            }
            else
            {
                StatusText = "Unable to get location";
            }
        }
        catch (Exception ex)
        {
            StatusText = "Error getting location";
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
        }

    }


    //Text-to-speech function
    public async Task SpeakAsync(string text)
    {
        try
        {
            await TextToSpeech.Default.SpeakAsync(text);
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("Text-to-speech is not supported on this device.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Text-to-speech error: {ex.Message}");
        }
    }

    //Haptic feedback function
    private static void Vibrate()
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("Vibration is not supported on this device.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Vibration error: {ex.Message}");
        }
        }

    //Accelerometer 
    private void StartAccelerometer()
    {
        if (_isAccelerometerRunning)
            return;

        Accelerometer.ReadingChanged += OnAccelerometerChanged;
        Accelerometer.Start(SensorSpeed.UI);

        _isAccelerometerRunning = true;
    }

    private void StopAccelerometer()
    {
        if (!_isAccelerometerRunning)
            return;

        Accelerometer.Stop();
        Accelerometer.ReadingChanged -= OnAccelerometerChanged;

        _isAccelerometerRunning = false;
    }

    private void OnAccelerometerChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        if (!AppSettingsService.AutoPauseEnabled)
            return;

        if (!_isRunning)
            return;

        var data = e.Reading;

        double movement =
             Math.Sqrt(
            data.Acceleration.X * data.Acceleration.X +
            data.Acceleration.Y * data.Acceleration.Y +
            data.Acceleration.Z * data.Acceleration.Z);

        // threshold to detect movement
        if (movement > 1.2)
        {
            _noMovementSeconds = 0;

            if (_isPaused)
            {
                _isPaused = false;
                StatusText = "Status: Running (resumed)";
                StartTimer();
            }
        }
        else
        {
            _noMovementSeconds++;

            if (_noMovementSeconds >= PauseThreshold && !_isPaused)
            {
                _isPaused = true;
                StatusText = "Status: Paused (no movement)";
               
            }
        }
    }

    //Compass feature
    private void StartCompass()
    {
        if (_isCompassRunning)
            return;

        if (!Compass.Default.IsSupported)
        {
            CompassHeadingText = "Heading: Not available";
            CompassDirectionText = "Direction: Compass not supported";
            return;
        }

        Compass.Default.ReadingChanged += OnCompassReadingChanged;
        Compass.Default.Start(SensorSpeed.UI);

        _isCompassRunning = true;
    }

    private void StopCompass()
    {
        if (!_isCompassRunning)
            return;

        Compass.Default.Stop();
        Compass.Default.ReadingChanged -= OnCompassReadingChanged;

        _isCompassRunning = false;
    }

    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        double heading = e.Reading.HeadingMagneticNorth;

        CompassHeadingText = $"Heading: {heading:F0}°";
        CompassDirectionText = $"Direction: {GetCompassDirection(heading)}";
    }

    private string GetCompassDirection(double heading)
    {
        if (heading >= 337.5 || heading < 22.5)
            return "North";

        if (heading >= 22.5 && heading < 67.5)
            return "North-East";

        if (heading >= 67.5 && heading < 112.5)
            return "East";

        if (heading >= 112.5 && heading < 157.5)
            return "South-East";

        if (heading >= 157.5 && heading < 202.5)
            return "South";

        if (heading >= 202.5 && heading < 247.5)
            return "South-West";

        if (heading >= 247.5 && heading < 292.5)
            return "West";

        return "North-West";
    }


    //Live tracking on map
    private void StartLocationTracking()
    {
        if (_locationTimerRunning)
            return;

        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null)
            return;

        _locationTimerRunning = true;

        dispatcher.StartTimer(TimeSpan.FromSeconds(3), () =>
        {
            if (!_isRunning)
            {
                _locationTimerRunning = false;
                return false;
            }

            if (!_isPaused)
            {
                _ = UpdateRouteLocationAsync();
            }

            return true;
        });
    }

    //Update location and calculate distance/speed
    private async Task UpdateRouteLocationAsync()
    {
        try
        {
            var location = await _geolocation.GetLocationAsync(
                new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(10)
                });

            if (location == null)
                return;

            Latitude = location.Latitude;
            Longitude = location.Longitude;

            // Calculate distance from previous GPS point
            if (_lastLocation != null)
            {
                double distanceFromLastPoint =
                    _lastLocation.CalculateDistance(location, DistanceUnits.Kilometers);

                // Ignore very tiny GPS jumps/noise
                if (distanceFromLastPoint > 0.005)
                {
                    _totalDistanceKm += distanceFromLastPoint;
                }
            }

            _lastLocation = location;

            UpdateDistanceAndSpeedText();

            //Send location to map route drawing
            RouteLocationAdded?.Invoke(location);
        }
        catch (FeatureNotSupportedException)
        {
            StatusText = "Status: GPS not supported on this device";
        }
        catch (FeatureNotEnabledException)
        {
            StatusText = "Status: GPS is disabled";
        }
        catch (PermissionException)
        {
            StatusText = "Status: Location permission denied";
        }
        catch (Exception ex)
        {
            StatusText = "Status: GPS update failed";
            System.Diagnostics.Debug.WriteLine($"GPS error: {ex.Message}");
        }
    }

    // km to miles conversion
    private void UpdateDistanceAndSpeedText()
    {
        double elapsedHours = _seconds / 3600.0;

        if (AppSettingsService.DistanceUnit == "Miles")
        {
            double totalMiles = _totalDistanceKm * 0.621371;

            DistanceText = $"Distance: {totalMiles:F2} miles";

            if (elapsedHours > 0)
            {
                double averageMph = totalMiles / elapsedHours;
                SpeedText = $"Speed: {averageMph:F1} mph";
            }
            else
            {
                SpeedText = "Speed: 0.0 mph";
            }
        }
        else
        {
            DistanceText = $"Distance: {_totalDistanceKm:F2} km";

            if (elapsedHours > 0)
            {
                double averageKmh = _totalDistanceKm / elapsedHours;
                SpeedText = $"Speed: {averageKmh:F1} km/h";
            }
            else
            {
                SpeedText = "Speed: 0.0 km/h";
            }
        }
    }

    //Property change notification
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}