using Microsoft.Extensions.DependencyInjection;
using SqlToExcel.Services;
using SqlToExcel.Services.Interfaces;
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
        // 注册接口和实现
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IConnectionStringService, ConnectionStringService>();

        // 注册基础服务（按依赖顺序）
        services.AddSingleton<DatabaseService>(provider => DatabaseService.Instance);
        services.AddSingleton<ThemeService>();
        services.AddSingleton<ExcelExportService>();
        services.AddSingleton<ConfigFileService>();
        services.AddSingleton<ConfigService>();

        // 注册 ViewModels（按依赖顺序）
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<BatchExportViewModel>();
        services.AddSingleton<TableMappingViewModel>(); // 改为 Singleton
        services.AddTransient<SchemaComparisonViewModel>();
        services.AddTransient<TableComparisonViewModel>();

        // 注册 Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            // Configure and build service provider
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Initialize database first
            DatabaseService.Instance.Initialize();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Loaded += (sender, args) => mainViewModel.CheckDatabaseConfiguration();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"应用程序启动失败: {ex.Message}\n\n详细信息:\n{ex}", "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
    }
}
