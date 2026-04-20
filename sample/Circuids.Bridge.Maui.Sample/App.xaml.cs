namespace Circuids.Bridge.Maui.Sample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "Circuids.Bridge.Maui.Sample" };
    }
}
