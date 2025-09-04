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
        services.AddTransient<DatabaseConfigViewModel>();
        services.AddTransient<SchemaComparisonViewModel>();
        services.AddTransient<TableComparisonViewModel>();

        // 注册 Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // The ServiceProvider is already built in the App constructor.

        // 1. Initialize local DB and create the SqlSugarScope
        try
        {
            DatabaseService.Instance.InitializeLocalDbAndCreateScope();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用程序启动失败: 无法初始化本地数据库。\n\n详细信息:\n{ex}", "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
            return;
        }

        // 2. Loop to test remote connections until successful or user cancels
        while (true)
        {
            try
            {
                // Try to test remote connections
                DatabaseService.Instance.TestRemoteConnections();
                
                // If successful, break the loop
                break;
            }
            catch (Exception ex)
            {
                // Connection failed, show the configuration dialog
                var message = $"数据库连接失败: {ex.Message}\n\n请检查并保存您的数据库连接设置。";
                MessageBox.Show(message, "连接错误", MessageBoxButton.OK, MessageBoxImage.Warning);

                var configViewModel = ServiceProvider.GetRequiredService<DatabaseConfigViewModel>();
                var configView = new Views.DatabaseConfigView(configViewModel);
                
                bool? dialogResult = configView.ShowDialog();

                if (dialogResult == true)
                {
                    // User saved new settings. Dispose the old scope, then re-initialize and let the loop try again.
                    DatabaseService.Instance.DisposeScope();
                    DatabaseService.Instance.InitializeLocalDbAndCreateScope();
                }
                else
                {
                    // User cancelled or closed the dialog, so we shut down.
                    Current.Shutdown();
                    return;
                }
            }
        }

        // 3. If connection is successful, show the main window
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;
        mainWindow.Show();
    }
}
