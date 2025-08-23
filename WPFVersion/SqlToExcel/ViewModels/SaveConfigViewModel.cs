using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class SaveConfigViewModel : INotifyPropertyChanged
    {
        private string _configKey = "";
        private string _sourceDescription = "";
        private string _targetDescription = "";
        private string _sourceSql;
        private string _targetSql;

        // 从MainViewModel传入的数据
        public string SourceSql { get => _sourceSql; set { _sourceSql = value; OnPropertyChanged(); } }
        public string SourceSheetName { get; }
        public string TargetSql { get => _targetSql; set { _targetSql = value; OnPropertyChanged(); } }
        public string TargetSheetName { get; }

        // 属性
        public string ConfigKey
        {
            get => _configKey;
            set
            {
                _configKey = value;
                OnPropertyChanged();
            }
        }

        public string SourceDescription
        {
            get => _sourceDescription;
            set
            {
                _sourceDescription = value;
                OnPropertyChanged();
            }
        }

        public string TargetDescription
        {
            get => _targetDescription;
            set
            {
                _targetDescription = value;
                OnPropertyChanged();
            }
        }


        // 命令
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // 对话框结果
        public bool? DialogResult { get; set; }

        public SaveConfigViewModel(string sourceSql, string sourceSheetName, string targetSql, string targetSheetName, string? sourceDescription = null, string? targetDescription = null)
        {
            _sourceSql = sourceSql;
            SourceSheetName = sourceSheetName;
            _targetSql = targetSql;
            TargetSheetName = targetSheetName;
            _sourceDescription = sourceDescription ?? "";
            _targetDescription = targetDescription ?? "";

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public SaveConfigViewModel(Models.BatchExportConfig config)
            : this(config.DataSource.Sql, config.DataSource.SheetName, config.DataTarget.Sql, config.DataTarget.SheetName)
        {
            ConfigKey = config.Key;
            SourceDescription = config.DataSource.Description;
            TargetDescription = config.DataTarget.Description;
            IsEditMode = true;
        }


        private bool CanSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(ConfigKey) &&
                   !string.IsNullOrWhiteSpace(SourceSql) &&
                   !string.IsNullOrWhiteSpace(TargetSql) &&
                   !string.IsNullOrWhiteSpace(SourceSheetName) &&
                   !string.IsNullOrWhiteSpace(TargetSheetName);
        }

        private async void Save(object? parameter)
        {
            if (!await ValidateInputAsync())
            {
                return;
            }

            DialogResult = true;
            CloseDialog();
        }

        private void Cancel(object? parameter)
        {
            DialogResult = false;
            CloseDialog();
        }

        private async System.Threading.Tasks.Task<bool> ValidateInputAsync()
        {
            // 检查配置键是否为空
            if (string.IsNullOrWhiteSpace(ConfigKey))
            {
                MessageBox.Show("配置键不能为空！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 检查配置键是否包含非法字符
            if (ConfigKey.Contains("/") || ConfigKey.Contains("\\") || ConfigKey.Contains(":") ||
                ConfigKey.Contains("*") || ConfigKey.Contains("?") || ConfigKey.Contains("\"") ||
                ConfigKey.Contains("<") || ConfigKey.Contains(">") || ConfigKey.Contains("|"))
            {
                MessageBox.Show("配置键不能包含以下字符：/ \\ : * ? \" < > |", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 异步检查键是否存在
            if (!IsEditMode && await Services.ConfigService.Instance.IsKeyExistsAsync(ConfigKey))
            {
                 var result = MessageBox.Show(
                            $"配置键 '{ConfigKey}' 已存在。是否要覆盖现有配置？",
                            "配置已存在",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            }

            return true;
        }

        private void CloseDialog()
        {
            // 查找并关闭对话框窗口
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = DialogResult;
                    window.Close();
                    break;
                }
            }
        }
        public bool IsEditMode { get; private set; }
        public bool IsKeyAndDescriptionEditable => !IsEditMode;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}