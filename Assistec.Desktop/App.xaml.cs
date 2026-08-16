namespace Assistec.Desktop;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _ = Services.ApiLauncher.GarantirApiRodandoAsync();

        var window = new Window(new MainPage())
        {
            Title = "AssisTec"
        };

        window.Width = 1280;
        window.Height = 800;
        window.MinimumWidth = 960;
        window.MinimumHeight = 600;

        window.Destroying += (_, _) => Services.ApiLauncher.Encerrar();

        return window;
    }
}