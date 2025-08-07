using SqlSugar;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

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

        public ICommand TestSourceConnectionCommand { get; }
        public ICommand TestTargetConnectionCommand { get; }
        public ICommand SaveCommand { get; }

        public DatabaseConfigViewModel()
        {
            // Load existing settings if available
            SourceConnectionString = Properties.Settings.Default.SourceConnectionString ?? string.Empty;
            TargetConnectionString = Properties.Settings.Default.TargetConnectionString ?? string.Empty;

            TestSourceConnectionCommand = new RelayCommand(p => TestConnection(SourceConnectionString, "源"));
            TestTargetConnectionCommand = new RelayCommand(p => TestConnection(TargetConnectionString, "目标"));
            SaveCommand = new RelayCommand(p => SaveConfiguration(p as Window));
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

        private void SaveConfiguration(Window? window)
        {
            Properties.Settings.Default.SourceConnectionString = SourceConnectionString;
            Properties.Settings.Default.TargetConnectionString = TargetConnectionString;
            Properties.Settings.Default.Save();

            MessageBox.Show("配置已保存。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            window?.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}