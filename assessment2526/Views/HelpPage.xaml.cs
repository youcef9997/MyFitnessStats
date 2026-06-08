using assessment2526.ViewModels;
namespace assessment2526.Views;

public partial class HelpPage : ContentPage
{
	public HelpPage()
	{
		InitializeComponent();
        BindingContext = new HelpViewModel();
    }
}