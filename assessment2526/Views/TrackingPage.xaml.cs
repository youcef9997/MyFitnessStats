using assessment2526.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;


namespace assessment2526.Views;


public partial class TrackingPage : ContentPage
{
    private TrackingViewModel? _viewModel;

    //Live tracking page, shows the users current location on a map and updates as they move
    private readonly Polyline _routeLine = new()
    {
        StrokeColor = Colors.DeepSkyBlue,
        StrokeWidth = 8
    };

    private bool _routeLineAdded = false;
    public TrackingPage()
	{
		InitializeComponent();

        _viewModel = new TrackingViewModel(Connectivity.Current, Geolocation.Default);
        BindingContext = _viewModel;

        _viewModel.RouteLocationAdded += OnRouteLocationAdded;
        _viewModel.RouteResetRequested += OnRouteResetRequested;
    }


    private void OnRouteResetRequested()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _routeLine.Geopath.Clear();
            MapView.MapElements.Clear();
            _routeLineAdded = false;
        });
    }

    //Live tracking method
    private void OnRouteLocationAdded(Location location)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_routeLineAdded)
            {
                MapView.MapElements.Add(_routeLine);
                _routeLineAdded = true;
            }

            _routeLine.Geopath.Add(location);

            MapView.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    location,
                    Distance.FromKilometers(0.5)));
        });
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //small delay to let UI load
        await Task.Delay(300);

        //UpdateMap();
    }
}