using SqlToExcel.Models;
using SqlToExcel.Services;
using SqlToExcel.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace SqlToExcel.ViewModels
{
    public class BatchExportViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BatchExportConfigItemViewModel> Items { get; } = new();
        private readonly ExcelExportService _exportService;
        private readonly ConfigService _configService;

        public ICommand ExportCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExportConfigsCommand { get; }
        public ICommand BatchExportCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        public string SelectedCountText => $"已选择 {Items.Count(x => x.IsSelected)} 项";

        public BatchExportViewModel(ExcelExportService exportService)
        {
            _exportService = exportService;
            _configService = ConfigService.Instance;

            ExportCommand = new RelayCommand(async param => await ExportAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            PreviewCommand = new RelayCommand(async param => await PreviewAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            DeleteCommand = new RelayCommand(async param => await DeleteAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            EditCommand = new RelayCommand(async param => await EditAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            ExportConfigsCommand = new RelayCommand(async _ => await ExportConfigsAsync());
            BatchExportCommand = new RelayCommand(async _ => await BatchExportAsync(), _ => Items.Any(x => x.IsSelected));
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            ClearSelectionCommand = new RelayCommand(_ => ClearSelection());

            _configService.ConfigsChanged += OnConfigsChanged;
            _ = LoadConfigsAsync();
        }

        private bool CanExecute(BatchExportConfigItemViewModel? item)
        {
            return item != null && item.Status != "正在导出..." && item.Status != "正在预览...";
        }

        private void OnConfigsChanged(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => _ = ReloadConfigsAsync());
        }

        private async Task ReloadConfigsAsync()
        {
            Items.Clear();
            await LoadConfigsAsync();
        }

        private async Task LoadConfigsAsync()
        {
            try
            {
                var configs = await _configService.LoadConfigsAsync();
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        var item = new BatchExportConfigItemViewModel(config)
                        {
                            ExportCommand = this.ExportCommand,
                            PreviewCommand = this.PreviewCommand,
                            DeleteCommand = this.DeleteCommand,
                            EditCommand = this.EditCommand
                        };
                        item.PropertyChanged += OnItemPropertyChanged;
                        Items.Add(item);
                    }
                }
            }
            catch (Exception)
            {
                // Handle error loading configs
            }
        }

        private async Task ExportAsync(BatchExportConfigItemViewModel? item)
        {
            if (item == null) return;

            item.Status = "正在导出...";
            try
            {
                // Build the new filename
                int index = Items.IndexOf(item) + 1;
                string tableName = item.Config.DataSource.TableName ?? ExtractTableName(item.Config.DataSource.Sql);
                string fileName = $"{index}) {item.Key}-[{tableName}(Source)].xlsx";

                await _exportService.ExportToExcelAsync(
                    item.Config.DataSource.Sql,
                    item.Config.DataSource.SheetName,
                    item.Config.DataTarget.Sql,
                    item.Config.DataTarget.SheetName,
                    item.Config.Key,
                    fileName);
                item.Status = "成功";
            }
            catch (Exception ex)
            {
                item.Status = $"失败: {ex.Message}";
            }
        }

        private async Task PreviewAsync(BatchExportConfigItemViewModel? item)
        {
            if (item == null) return;

            item.Status = "正在预览...";
            try
            {
                var task1 = _exportService.GetDataTableAsync(item.Config.DataSource.Sql, "source");
                var task2 = _exportService.GetDataTableAsync(item.Config.DataTarget.Sql, "target");
                await Task.WhenAll(task1, task2);

                DataTable dt1 = task1.Result;
                DataTable dt2 = task2.Result;

                var dualViewModel = new DualPreviewViewModel(dt1, dt2);
                var dualView = new DualPreviewView
                {
                    DataContext = dualViewModel,
                    Owner = Application.Current.MainWindow
                };
                dualView.Show();
                item.Status = "准备就绪";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行查询时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                item.Status = "预览失败";
            }
        }

        private async Task DeleteAsync(BatchExportConfigItemViewModel? item)
        {
            if (item == null) return;

            var result = MessageBox.Show($"确定要删除配置 '{item.Key}' 吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _configService.DeleteConfigAsync(item.Key);
            }
        }

        private async Task EditAsync(BatchExportConfigItemViewModel? item)
        {
            if (item == null) return;

            // 发布事件，让主界面加载配置进行编辑
            EventService.Publish(new LoadConfigToMainViewEvent(item.Config, isEditMode: true, originalKey: item.Config.Key));
        }

        private async Task ExportConfigsAsync()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "batch_export_configs.json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await _configService.ExportConfigsToJsonAsync(saveFileDialog.FileName);
            }
        }

        private string ExtractTableName(string sql)
        {
            try
            {
                var fromIndex = sql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
                if (fromIndex == -1) return "UnknownTable";

                var fromSubstring = sql.Substring(fromIndex + 4).Trim();
                
                var orderByIndex = fromSubstring.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
                if (orderByIndex != -1)
                {
                    fromSubstring = fromSubstring.Substring(0, orderByIndex).Trim();
                }

                return fromSubstring.Split(' ').FirstOrDefault() ?? "UnknownTable";
            }
            catch
            {
                return "UnknownTable";
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BatchExportConfigItemViewModel.IsSelected))
            {
                OnPropertyChanged(nameof(SelectedCountText));
            }
        }

        private void SelectAll()
        {
            foreach (var item in Items)
            {
                item.IsSelected = true;
            }
        }

        private void ClearSelection()
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
        }

        private async Task BatchExportAsync()
        {
            var selectedItems = Items.Where(x => x.IsSelected).ToList();
            if (!selectedItems.Any())
            {
                MessageBox.Show("请先选择要导出的配置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Choose target folder using SaveFileDialog as workaround
            var folderDialog = new SaveFileDialog
            {
                Title = "选择导出文件夹 (请选择任意文件名，程序将使用该文件所在文件夹)",
                FileName = "选择此文件夹",
                Filter = "文件夹|*.folder"
            };

            if (folderDialog.ShowDialog() != true)
                return;

            var targetFolder = System.IO.Path.GetDirectoryName(folderDialog.FileName);
            if (string.IsNullOrEmpty(targetFolder))
            {
                MessageBox.Show("无法获取选择的文件夹路径。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show progress dialog using MessageBox for now to avoid XAML issues
            var result = MessageBox.Show($"确定要批量导出选中的 {selectedItems.Count} 个配置到文件夹 {targetFolder} 吗？",
                "确认批量导出", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes)
                return;

            // Start the export process directly
            try
            {
                var configs = selectedItems.Select(x => x.Config).ToList();
                var progressReported = 0;
                var total = configs.Count;
                
                await _exportService.BatchExportToFolderAsync(configs, targetFolder,
                    new Progress<(int current, int total, string currentItem)>(report =>
                    {
                        progressReported = report.current + 1;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Update status for currently processing item
                            if (report.current < selectedItems.Count)
                            {
                                selectedItems[report.current].Status = $"正在导出... ({progressReported}/{total})";
                            }
                        });
                    }));
                
                // Update status for all exported items
                foreach (var item in selectedItems)
                {
                    item.Status = "导出成功";
                }
                
                MessageBox.Show($"批量导出完成！已成功导出 {selectedItems.Count} 个配置到文件夹：{targetFolder}",
                    "批量导出完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                foreach (var item in selectedItems)
                {
                    item.Status = "导出失败";
                }
                MessageBox.Show($"批量导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
