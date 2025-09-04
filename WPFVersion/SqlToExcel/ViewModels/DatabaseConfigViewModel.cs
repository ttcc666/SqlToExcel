using SqlSugar;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System;

namespace SqlToExcel.ViewModels
{
    public class DatabaseConfigViewModel : INotifyPropertyChanged
    {
        private string _sourceConnectionString = string.Empty;
        private string _targetConnectionString = string.Empty;

        public string SourceConnectionString
        {
            get => _sourceConnectionString;
            set { _sourceConnectionString = value; OnPropertyChanged(); }
        }

        public string TargetConnectionString
        {
            get => _targetConnectionString;
            set { _targetConnectionString = value; OnPropertyChanged(); }
        }

        private string _frameworkConnectionString = string.Empty;
        public string FrameworkConnectionString
        {
            get => _frameworkConnectionString;
            set { _frameworkConnectionString = value; OnPropertyChanged(); }
        }


        public ICommand TestSourceConnectionCommand { get; }
        public ICommand TestTargetConnectionCommand { get; }
        public ICommand TestFrameworkConnectionCommand { get; }
        public ICommand SaveCommand { get; }

        private bool _isSaving = false;

        public DatabaseConfigViewModel()
        {
            // Load existing settings if available
            SourceConnectionString = Properties.Settings.Default.SourceConnectionString ?? string.Empty;
            TargetConnectionString = Properties.Settings.Default.TargetConnectionString ?? string.Empty;
            FrameworkConnectionString = Properties.Settings.Default.FrameworkConnectionString ?? string.Empty;

            TestSourceConnectionCommand = new RelayCommand(p => TestConnection(SourceConnectionString, "源"));
            TestTargetConnectionCommand = new RelayCommand(p => TestConnection(TargetConnectionString, "目标"));
            TestFrameworkConnectionCommand = new RelayCommand(p => TestConnection(FrameworkConnectionString, "框架库"));
            SaveCommand = new RelayCommand(p => SaveConfiguration());
        }

        private void TestConnection(string? connectionString, string dbName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show($"请输入{dbName}数据库连接字符串。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Use a temporary client for testing connection only
                var sugarClient = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = connectionString,
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true
                });

                // A more reliable way to test a connection is to simply open it.
                sugarClient.Ado.Open();
                if (sugarClient.Ado.Connection.State == System.Data.ConnectionState.Open)
                {
                    MessageBox.Show($"{dbName}数据库连接成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    sugarClient.Ado.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{dbName}数据库连接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveConfiguration()
        {
            if (_isSaving) return;

            if (string.IsNullOrWhiteSpace(SourceConnectionString) || string.IsNullOrWhiteSpace(TargetConnectionString))
            {
                MessageBox.Show("源数据库和目标数据库的连接字符串不能为空。", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isSaving = true;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                // Run blocking database calls on a background thread
                await Task.Run(() =>
                {
                    using (var testClient = new SqlSugarClient(new ConnectionConfig() { ConnectionString = SourceConnectionString, DbType = DbType.SqlServer, IsAutoCloseConnection = true }))
                    {
                        testClient.Ado.ExecuteCommand("SELECT 1");
                    }
                    using (var testClient = new SqlSugarClient(new ConnectionConfig() { ConnectionString = TargetConnectionString, DbType = DbType.SqlServer, IsAutoCloseConnection = true }))
                    {
                        testClient.Ado.ExecuteCommand("SELECT 1");
                    }
                    if (!string.IsNullOrWhiteSpace(FrameworkConnectionString))
                    {
                        using (var testClient = new SqlSugarClient(new ConnectionConfig() { ConnectionString = FrameworkConnectionString, DbType = DbType.SqlServer, IsAutoCloseConnection = true }))
                        {
                            testClient.Ado.ExecuteCommand("SELECT 1");
                        }
                    }
                });

                // If tests succeed, save the configuration
                Properties.Settings.Default.SourceConnectionString = SourceConnectionString;
                Properties.Settings.Default.TargetConnectionString = TargetConnectionString;
                Properties.Settings.Default.FrameworkConnectionString = FrameworkConnectionString;
                Properties.Settings.Default.Save();

                // Find and close the dialog on the UI thread
                var windowToClose = Application.Current.Windows.OfType<Views.DatabaseConfigView>().FirstOrDefault();
                if (windowToClose != null)
                {
                    windowToClose.DialogResult = true;
                    windowToClose.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据库连接测试失败，配置未保存。请检查连接字符串。\n\n错误: {ex.Message}", "测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                _isSaving = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}