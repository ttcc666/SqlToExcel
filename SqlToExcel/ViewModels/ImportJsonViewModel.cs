using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class ImportJsonViewModel : INotifyPropertyChanged
    {
        private string _jsonText = "";
        public string JsonText
        {
            get => _jsonText;
            set { _jsonText = value; OnPropertyChanged(); }
        }

        public ICommand ImportCommand { get; }
        public ICommand LoadFromFileCommand { get; }
        public ICommand GenerateTemplateCommand { get; }
        public ICommand CloseCommand { get; }

        public string? ResultJson { get; private set; }

        public ImportJsonViewModel()
        {
            ImportCommand = new RelayCommand(p => Import(p as Window));
            LoadFromFileCommand = new RelayCommand(async p => await LoadFromFile());
            GenerateTemplateCommand = new RelayCommand(p => GenerateTemplate());
            CloseCommand = new RelayCommand(p => Close(p as Window));
        }

        private void Import(Window? window)
        {
            if (string.IsNullOrWhiteSpace(JsonText))
            {
                MessageBox.Show("JSON内容不能为空。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ResultJson = JsonText;
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private async Task LoadFromFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "选择一个JSON配置文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    JsonText = await File.ReadAllTextAsync(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GenerateTemplate()
        {
            var template = new
            {
                old_table = "SourceTableName",
                old_fields = new[] { "Column1", "Column2" },
                new_table = "TargetTableName",
                new_fields = new[] { "NewColumn1", "NewColumn2" }
            };
            JsonText = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        }

        private void Close(Window? window)
        {
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}