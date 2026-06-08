using assessment2526.ViewModels;
namespace assessment2526.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage()
	{
		InitializeComponent();
		BindingContext = new HistoryViewModel();
	}
}