using HandyControl.Controls;
using SqlToExcel.ViewModels;
using SqlSugar;
using System.Windows;

namespace SqlToExcel
{
    public partial class MainWindow : BlurWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            BatchExportView.DataContext = new BatchExportViewModel();

            this.Loaded += (s, e) =>
            {
                try
                {
                    viewModel.CheckDatabaseConfiguration();
                }
                catch (SqlSugarException)
                {
                    HandyControl.Controls.MessageBox.Show(
                        "数据库连接配置无效或已损坏，将重置为默认设置。请重新配置数据库连接。",
                        "配置错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    // 重置设置
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.Save();

                    // 再次尝试，这次会进入未配置状态
                    viewModel.CheckDatabaseConfiguration();
                }
            };
        }
    }
}