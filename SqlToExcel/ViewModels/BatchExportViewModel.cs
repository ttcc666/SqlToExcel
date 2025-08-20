using System.Collections.Generic;
using System.Text.Json;
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
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using System.Collections;

namespace SqlToExcel.ViewModels
{
    public class BatchExportConfigItemViewModelComparer : IComparer
    {
        private readonly NaturalStringComparer _stringComparer = new NaturalStringComparer();

        public int Compare(object a, object b)
        {
            if (a is BatchExportConfigItemViewModel itemA && b is BatchExportConfigItemViewModel itemB)
            {
                return _stringComparer.Compare(itemA.Prefix, itemB.Prefix);
            }
            return 0;
        }
    }

    public class BatchExportViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BatchExportConfigItemViewModel> Items { get; }
        public ICollectionView FilteredItems { get; }
        private readonly ExcelExportService _exportService;
        private readonly ConfigService _configService;
        private Timer? _debounceTimer;

        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword == value) return;
                _searchKeyword = value;
                OnPropertyChanged();
                FilteredItems.Refresh();
            }
        }

        private bool _isBatchExporting;
        public bool IsBatchExporting
        {
            get => _isBatchExporting;
            set
            {
                _isBatchExporting = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand ExportCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExportConfigsCommand { get; }
        public ICommand ImportConfigCommand { get; }
        public ICommand BatchExportCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand RefreshCommand { get; }

        public string SelectedCountText => $"已选择 {FilteredItems.Cast<object>().Count(x => ((BatchExportConfigItemViewModel)x).IsSelected)} 项";

        public string TotalCountText => $"共 {Items.Count} 项";

        public BatchExportViewModel(ExcelExportService exportService)
        {
            _exportService = exportService;
            _configService = ConfigService.Instance;

            Items = new ObservableCollection<BatchExportConfigItemViewModel>();
            FilteredItems = CollectionViewSource.GetDefaultView(Items);
            FilteredItems.Filter = FilterPredicate;
            if (FilteredItems is ListCollectionView listCollectionView)
            {
                listCollectionView.CustomSort = new BatchExportConfigItemViewModelComparer();
            }

            Items.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(TotalCountText)); };

            ExportCommand = new RelayCommand(async param => await ExportAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            PreviewCommand = new RelayCommand(async param => await PreviewAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            DeleteCommand = new RelayCommand(async param => await DeleteAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            EditCommand = new RelayCommand(async param => await EditAsync(param as BatchExportConfigItemViewModel), param => CanExecute(param as BatchExportConfigItemViewModel));
            ExportConfigsCommand = new RelayCommand(async _ => await ExportConfigsAsync(), _ => !IsBatchExporting);
            ImportConfigCommand = new RelayCommand(async _ => await ImportConfigAsync(), _ => !IsBatchExporting);
            BatchExportCommand = new RelayCommand(async _ => await BatchExportAsync(), _ => Items.Any(x => x.IsSelected) && !IsBatchExporting);
            SelectAllCommand = new RelayCommand(_ => SelectAll(), _ => !IsBatchExporting);
            ClearSelectionCommand = new RelayCommand(_ => ClearSelection(), _ => !IsBatchExporting);
            RefreshCommand = new RelayCommand(async _ => await ReloadConfigsAsync(), _ => !IsBatchExporting);

            _configService.ConfigsChanged += OnConfigsChanged;
            _ = LoadConfigsAsync();
            InitializeDebounceTimer();
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not BatchExportConfigItemViewModel item)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(SearchKeyword))
            {
                return true;
            }

            return item.Key.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase);
        }

        private void InitializeDebounceTimer()
        {
            _debounceTimer = new Timer(async _ => await SaveAllConfigsCallback(), null, Timeout.Infinite, Timeout.Infinite);
        }

        private async Task SaveAllConfigsCallback()
        {
            var configsToSave = Items.Select(vm => vm.Config).ToList();
            await _configService.SaveAllConfigsAsync(configsToSave);
        }

        private bool CanExecute(BatchExportConfigItemViewModel? item)
        {
            return item != null && item.Status != "正在导出..." && item.Status != "正在预览..." && !IsBatchExporting;
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
                    int currentIndex = 0;
                    foreach (var config in configs)
                    {
                        // Backward compatibility for old configs
                        if (string.IsNullOrEmpty(config.Prefix))
                        {
                            config.Prefix = (currentIndex + 1).ToString();
                        }

                        var item = new BatchExportConfigItemViewModel(config)
                        {
                            ExportCommand = this.ExportCommand,
                            PreviewCommand = this.PreviewCommand,
                            DeleteCommand = this.DeleteCommand,
                            EditCommand = this.EditCommand
                        };
                        item.PropertyChanged += OnItemPropertyChanged;
                        Items.Add(item);
                        currentIndex++;
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
                string tableName = item.Config.DataSource.TableName ?? ExtractTableName(item.Config.DataSource.Sql);
                string fileName = $"{item.Prefix}) {item.Key}-{tableName}(Source).xlsx";
                string destinationDbKey = item.Config.Destination == DestinationType.Target ? "target" : "framework";

                bool success = await _exportService.ExportToExcelAsync(
                    item.Config.DataSource.Sql,
                    item.Config.DataSource.SheetName,
                    item.Config.DataTarget.Sql,
                    item.Config.DataTarget.SheetName,
                    destinationDbKey,
                    item.Config.DataSource.Description,
                    item.Config.DataTarget.Description,
                    fileName);

                if (success)
                {
                    item.Status = "成功";
                }
                else
                {
                    item.Status = "已取消";
                }
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
                string destinationDbKey = item.Config.Destination == DestinationType.Target ? "target" : "framework";
                var task1 = _exportService.GetDataTableAsync(item.Config.DataSource.Sql, "source");
                var task2 = _exportService.GetDataTableAsync(item.Config.DataTarget.Sql, destinationDbKey);
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

            var editViewModel = new EditBatchSqlViewModel(item.Config);
            var editDialog = new EditBatchSqlDialog
            {
                DataContext = editViewModel,
                Owner = Application.Current.MainWindow
            };

            if (editDialog.ShowDialog() == true)
            {
                // The changes are saved in the viewmodel's SaveChanges method.
                // Now, we need to trigger the auto-save for the whole list.
                _debounceTimer?.Change(200, Timeout.Infinite); // Trigger save after a short delay
            }
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

        private async Task ImportConfigAsync()
        {
            var importViewModel = new ImportJsonViewModel();
            var importDialog = new ImportJsonDialog
            {
                DataContext = importViewModel,
                Owner = Application.Current.MainWindow
            };

            if (importDialog.ShowDialog() == true)
            {
                var jsonContent = importViewModel.ResultJson;
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    MessageBox.Show("导入的内容为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                try
                {
                    var importedConfigs = JsonSerializer.Deserialize<List<BatchExportConfig>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (importedConfigs == null)
                    {
                        MessageBox.Show("无法解析JSON内容，请检查格式。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int updateCount = 0;
                    int addCount = 0;

                    foreach (var importedConfig in importedConfigs)
                    {
                        var existingItem = Items.FirstOrDefault(i => i.Key == importedConfig.Key);
                        if (existingItem != null)
                        {
                            // Update existing config
                            existingItem.Update(importedConfig);
                            updateCount++;
                        }
                        else
                        {
                            // Add new config
                            var newItem = new BatchExportConfigItemViewModel(importedConfig)
                            {
                                ExportCommand = this.ExportCommand,
                                PreviewCommand = this.PreviewCommand,
                                DeleteCommand = this.DeleteCommand,
                                EditCommand = this.EditCommand
                            };
                            newItem.PropertyChanged += OnItemPropertyChanged;
                            Items.Add(newItem);
                            addCount++;
                        }
                    }

                    // Trigger auto-save
                    _debounceTimer?.Change(200, Timeout.Infinite);

                    MessageBox.Show($"导入成功！\n更新了 {updateCount} 个配置。\n新增了 {addCount} 个配置。", "导入完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"JSON格式无效或不匹配: {ex.Message}", "导入失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"处理导入时发生未知错误: {ex.Message}", "导入失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
            else if (e.PropertyName == nameof(BatchExportConfigItemViewModel.Prefix))
            {
                // Debounce the save operation and refresh sort
                _debounceTimer?.Change(500, Timeout.Infinite);
                Application.Current.Dispatcher.Invoke(() => FilteredItems.Refresh());
            }
        }

        private void SelectAll()
        {
            foreach (var item in FilteredItems.Cast<BatchExportConfigItemViewModel>())
            {
                item.IsSelected = true;
            }
        }

        private void ClearSelection()
        {
            foreach (var item in FilteredItems.Cast<BatchExportConfigItemViewModel>())
            {
                item.IsSelected = false;
            }
        }

        private async Task BatchExportAsync()
        {
            var selectedItems = FilteredItems.Cast<BatchExportConfigItemViewModel>().Where(x => x.IsSelected).ToList();
            if (!selectedItems.Any())
            {
                MessageBox.Show("请先选择要导出的配置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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

            var result = MessageBox.Show($"确定要批量导出选中的 {selectedItems.Count} 个配置到文件夹 {targetFolder} 吗？",
                "确认批量导出", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes)
                return;

            IsBatchExporting = true;
            try
            {
                await Task.Run(async () =>
                {
                    var configs = selectedItems.Select(x => x.Config).ToList();
                    var progress = new Progress<(int current, int total, string currentItem)>(report =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (report.current < selectedItems.Count)
                            {
                                selectedItems[report.current].Status = $"正在导出... ({report.current + 1}/{report.total})";
                            }
                        });
                    });

                    if (await _exportService.BatchExportToFolderAsync(configs, targetFolder, progress))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var item in selectedItems)
                            {
                                item.Status = "导出成功";
                            }
                            MessageBox.Show($"批量导出完成！已成功导出 {selectedItems.Count} 个配置到文件夹：{targetFolder}", "批量导出完成", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("批量导出已完成，但部分项目导出失败。请检查输出文件夹和列表状态。", "部分失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                            foreach (var item in selectedItems.Where(i => i.Status.Contains("正在导出")))
                            {
                                item.Status = "导出失败";
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量导出过程中发生严重错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                foreach (var item in selectedItems)
                {
                    item.Status = "导出失败";
                }
            }
            finally
            {
                IsBatchExporting = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}