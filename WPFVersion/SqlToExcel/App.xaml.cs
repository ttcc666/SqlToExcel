using Microsoft.Extensions.DependencyInjection;
using SqlToExcel.Services;
using SqlToExcel.ViewModels;
using System.Windows;

namespace SqlToExcel;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ThemeService>();
        services.AddSingleton<ExcelExportService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<BatchExportViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize database first
        DatabaseService.Instance.Initialize();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;
        mainWindow.Loaded += (sender, args) => mainViewModel.CheckDatabaseConfiguration();
        mainWindow.Show();
    }
}
