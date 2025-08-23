using Microsoft.Win32;
using SqlSugar;
using SqlToExcel.Models;
using SqlToExcel.Services;
using SqlToExcel.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlToExcel.ViewModels
{
    public class SelectableDbColumn : INotifyPropertyChanged
    {
        private bool _isSelected;
        public DbColumnInfo Column { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public SelectableDbColumn(DbColumnInfo column)
        {
            Column = column;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class JsonMapping
    {
        public string old_table { get; set; }
        public List<string> old_fields { get; set; }
        public string new_table { get; set; }
        public List<string> new_fields { get; set; }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private string _sqlQuery1 = "";
        private string _sqlQuery2 = "";
        private string _sheetName1 = "SourceData";
        private string _sheetName2 = "TargetData";
        private string _statusMessage = "准备就绪";
        private bool _isCoreFunctionalityEnabled = false;
        private bool _isJsonImported = false;
        private bool _isEditMode = false;
        private string _editingConfigKey = "";
        private BatchExportConfig? _originalConfig = null;

        public ObservableCollection<DbTableInfo> Tables1 { get; } = new();
        public ObservableCollection<DbTableInfo> Tables2 { get; } = new();
        public ObservableCollection<SelectableDbColumn> Columns1 { get; } = new();
        public ObservableCollection<SelectableDbColumn> Columns2 { get; } = new();
        public ObservableCollection<SortableColumn> SortColumns1 { get; } = new();
        public ObservableCollection<SortableColumn> SortColumns2 { get; } = new();

        private List<string> _selectedColumnNames1 = new();
        private List<string> _selectedColumnNames2 = new();

        public ICollectionView Tables1View { get; private set; }
        public ICollectionView Tables2View { get; private set; }

        private string _searchText1 = "";
        public string SearchText1
        {
            get => _searchText1;
            set { _searchText1 = value; OnPropertyChanged(); Tables1View.Refresh(); }
        }

        private string _searchText2 = "";
        public string SearchText2
        {
            get => _searchText2;
            set { _searchText2 = value; OnPropertyChanged(); Tables2View.Refresh(); }
        }

        private DbTableInfo? _selectedTable1;
        public DbTableInfo? SelectedTable1
        {
            get => _selectedTable1;
            set
            {
                _selectedTable1 = value;
                OnPropertyChanged();
                LoadColumns(1);
                if (value != null)
                {
                    SheetName1 = $"{value.Name} (Source)";
                }
            }
        }

        private DbTableInfo? _selectedTable2;
        public DbTableInfo? SelectedTable2
        {
            get => _selectedTable2;
            set
            {
                _selectedTable2 = value;
                OnPropertyChanged();
                LoadColumns(2);
                if (value != null)
                {
                    SheetName2 = $"{value.Name} (Target)";
                }
            }
        }

        private Dictionary<string, string> _tableMappings = new();

        public string SqlQuery1 { get => _sqlQuery1; set { _sqlQuery1 = value; OnPropertyChanged(); CommandManager.RequerySuggested += (s, e) => { }; } }
        public string SqlQuery2 { get => _sqlQuery2; set { _sqlQuery2 = value; OnPropertyChanged(); CommandManager.RequerySuggested += (s, e) => { }; } }
        public string SheetName1 { get => _sheetName1; set { _sheetName1 = value; OnPropertyChanged(); } }
        public string SheetName2 { get => _sheetName2; set { _sheetName2 = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public bool IsCoreFunctionalityEnabled { get => _isCoreFunctionalityEnabled; set { _isCoreFunctionalityEnabled = value; OnPropertyChanged(); } }
        public bool IsJsonImported { get => _isJsonImported; set { _isJsonImported = value; OnPropertyChanged(); } }
        
        public bool IsEditMode
        {
            get => _isEditMode;
            private set
            {
                _isEditMode = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private DestinationType _selectedDestination = DestinationType.Target;
        public DestinationType SelectedDestination
        {
            get => _selectedDestination;
            set
            {
                if (_selectedDestination != value)
                {
                    _selectedDestination = value;
                    OnPropertyChanged();
                    ReloadDestinationData();
                }
            }
        }

        private int _maxRowCount = 5000;
        public int MaxRowCount
        {
            get => _maxRowCount;
            set { _maxRowCount = value; OnPropertyChanged(); }
        }

        public ICommand OpenConfigCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand Preview1Command { get; }
        public ICommand Preview2Command { get; }
        public ICommand PreviewBothCommand { get; }
        public ICommand OpenColumnSelectorCommand1 { get; }
        public ICommand OpenColumnSelectorCommand2 { get; }
        public ICommand OpenSortDialogCommand1 { get; }
        public ICommand OpenSortDialogCommand2 { get; }
        public ICommand SwitchThemeCommand { get; }
        public ICommand SaveConfigCommand { get; }
        public ICommand ImportJsonCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ShowTableComparisonCommand { get; }
        public ICommand SelectTargetDestinationCommand { get; }
        public ICommand SelectFrameworkDestinationCommand { get; }
 
        private readonly ExcelExportService _exportService;
        private readonly ThemeService _themeService;

        public MainViewModel(ExcelExportService exportService, ThemeService themeService)
        {
            _exportService = exportService;
            _themeService = themeService;
            OpenConfigCommand = new RelayCommand(p => OpenConfig());
            ExitCommand = new RelayCommand(p => Application.Current.Shutdown());
            ExportCommand = new RelayCommand(async p => await ExportAsync(), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery1) && !string.IsNullOrWhiteSpace(SqlQuery2));
            Preview1Command = new RelayCommand(async p => await PreviewAsync(SqlQuery1, SheetName1, "Source"), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery1));
            Preview2Command = new RelayCommand(async p => await PreviewAsync(SqlQuery2, SheetName2, SelectedDestination == DestinationType.Target ? "target" : "framework"), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery2));
            PreviewBothCommand = new RelayCommand(async p => await PreviewBothAsync(), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery1) && !string.IsNullOrWhiteSpace(SqlQuery2));
            OpenColumnSelectorCommand1 = new RelayCommand(p => OpenColumnSelector(1), p => SelectedTable1 != null && !IsJsonImported);
            OpenColumnSelectorCommand2 = new RelayCommand(p => OpenColumnSelector(2), p => SelectedTable2 != null && !IsJsonImported);
            OpenSortDialogCommand1 = new RelayCommand(p => OpenSortDialog(1), p => Columns1.Any(c => c.IsSelected));
            OpenSortDialogCommand2 = new RelayCommand(p => OpenSortDialog(2), p => Columns2.Any(c => c.IsSelected));
            SwitchThemeCommand = new RelayCommand(p => SwitchTheme());
            SaveConfigCommand = new RelayCommand(async p => await SaveConfigAsync(), p => CanSaveConfig());
            ImportJsonCommand = new RelayCommand(async p => await ImportJsonAsync(), p => !IsJsonImported);
            ResetCommand = new RelayCommand(p => ResetState(), p => IsJsonImported || IsEditMode);
            ShowTableComparisonCommand = new RelayCommand(p => OpenTableComparison());
            SelectTargetDestinationCommand = new RelayCommand(p => SelectedDestination = DestinationType.Target);
            SelectFrameworkDestinationCommand = new RelayCommand(p => SelectedDestination = DestinationType.Framework);
 
            LoadTableMappings();
 
            Tables1View = CollectionViewSource.GetDefaultView(Tables1);
            Tables1View.Filter = FilterTables1;
            Tables2View = CollectionViewSource.GetDefaultView(Tables2);
            Tables2View.Filter = FilterTables2;

            EventService.Subscribe<MappingsChangedEvent>(e => LoadTableMappings());
            
        }

        public void CheckDatabaseConfiguration()
        {
            if (DatabaseService.Instance.Initialize())
            {
                if (DatabaseService.Instance.IsConfigured())
                {
                    IsCoreFunctionalityEnabled = true;
                    StatusMessage = "数据库已连接，准备就绪。";
                    LoadTables();
                }
                else
                {
                    IsCoreFunctionalityEnabled = false;
                    StatusMessage = "数据库未配置，请从“文件”菜单中打开配置。";
                    if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
                    {
                        OpenConfig();
                    }
                }
            }
            else
            {
                IsCoreFunctionalityEnabled = false;
                StatusMessage = "数据库初始化失败，请检查应用权限或重启应用。";
                MessageBox.Show(StatusMessage, "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadDestinationData()
        {
            // Clear existing destination data
            SelectedTable2 = null;
            SqlQuery2 = "";
            SheetName2 = "TargetData"; // Reset sheet name
            Tables2.Clear();
            Columns2.Clear();
            SortColumns2.Clear();
            _selectedColumnNames2.Clear();

            // Load new tables based on selection
            string dbKey = SelectedDestination == DestinationType.Target ? "target" : "framework";
            DatabaseService.Instance.GetTables(dbKey).ForEach(t => Tables2.Add(t));

            // Update status or other dependent properties if necessary
            StatusMessage = $"目标已切换为: {SelectedDestination}";
        }

        private async void LoadTableMappings()
        {
            try
            {
                var mappings = await ConfigService.Instance.GetTableMappingsAsync();
                _tableMappings = mappings.ToDictionary(m => m.SourceTable, m => m.TargetTable, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"从数据库加载表映射时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                _tableMappings = new Dictionary<string, string>();
            }
        }

        private void LoadTables()
        {
            Tables1.Clear();
            Tables2.Clear();
            DatabaseService.Instance.GetTables("source").ForEach(t => Tables1.Add(t));

            string destinationDbKey = SelectedDestination == DestinationType.Target ? "target" : "framework";
            DatabaseService.Instance.GetTables(destinationDbKey).ForEach(t => Tables2.Add(t));
        }

        private void LoadColumns(int dbIndex)
        {
            Action<ObservableCollection<SelectableDbColumn>, ObservableCollection<SortableColumn>> setDefaultSort = 
                (columns, sortColumns) =>
            {
                var primaryKeyColumns = columns.Where(c => c.Column.IsPrimarykey).ToList();
                if (primaryKeyColumns.Any())
                {
                    sortColumns.Clear();
                    foreach (var pk in primaryKeyColumns)
                    {
                        sortColumns.Add(new SortableColumn(pk.Column.DbColumnName));
                    }
                }
            };

            if (dbIndex == 1 && SelectedTable1 != null)
            {
                Columns1.Clear();
                _selectedColumnNames1.Clear();
                SortColumns1.Clear();
                DatabaseService.Instance.GetColumns("source", SelectedTable1.Name).ForEach(c => Columns1.Add(new SelectableDbColumn(c)));
                setDefaultSort(Columns1, SortColumns1);

                var key = SelectedTable1.Name.Split('.').Last();
                if (_tableMappings.TryGetValue(key, out var targetTable))
                {
                    SelectedTable2 = Tables2.FirstOrDefault(t => t.Name.Split('.').Last().Equals(targetTable, StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (dbIndex == 2 && SelectedTable2 != null)
            {
                Columns2.Clear();
                _selectedColumnNames2.Clear();
                SortColumns2.Clear();
                string destinationDbKey = SelectedDestination == DestinationType.Target ? "target" : "framework";
                DatabaseService.Instance.GetColumns(destinationDbKey, SelectedTable2.Name).ForEach(c => Columns2.Add(new SelectableDbColumn(c)));
                setDefaultSort(Columns2, SortColumns2);
            }
        }

        private bool FilterTables1(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText1)) return true;
            if (obj is DbTableInfo table)
            {
                return table.Name.Contains(SearchText1, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private bool FilterTables2(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText2)) return true;
            if (obj is DbTableInfo table)
            {
                return table.Name.Contains(SearchText2, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private async void OpenColumnSelector(int dbIndex)
        {
            var columns = dbIndex == 1 ? Columns1 : Columns2;
            var selectedColumnNames = dbIndex == 1 ? _selectedColumnNames1 : _selectedColumnNames2;

            var viewModel = new ColumnSelectorViewModel(columns, selectedColumnNames);
            var view = new ColumnSelectorView
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            if (view.ShowDialog() == true)
            {
                if (dbIndex == 1)
                {
                    _selectedColumnNames1 = new List<string>(viewModel.SelectedColumnNamesInOrder);
                }
                else
                {
                    _selectedColumnNames2 = new List<string>(viewModel.SelectedColumnNamesInOrder);
                }
                
                await GenerateSqlAsync(dbIndex, viewModel.SelectedColumnNamesInOrder.Select(c => $"[{c}]").ToList());
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async void OpenSortDialog(int dbIndex)
        {
            var columns = dbIndex == 1 ? Columns1 : Columns2;
            var sortColumns = dbIndex == 1 ? SortColumns1 : SortColumns2;
            var selectedColumnNames = dbIndex == 1 ? _selectedColumnNames1 : _selectedColumnNames2;

            var viewModel = new ColumnSortViewModel(selectedColumnNames, sortColumns);
            var view = new ColumnSortView
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            if (view.ShowDialog() == true)
            {
                sortColumns.Clear();
                foreach (var sc in viewModel.SortColumns)
                {
                    sortColumns.Add(sc);
                }
                var selectedColumns = dbIndex == 1 ? _selectedColumnNames1 : _selectedColumnNames2;
                await GenerateSqlAsync(dbIndex, selectedColumns.Select(c => $"[{c}]").ToList());
            }
        }

        public async Task GenerateSqlAsync(int dbIndex, List<string> selectedColumnNames)
        {
            if (!selectedColumnNames.Any()) return;

            DbTableInfo? selectedTable = dbIndex == 1 ? SelectedTable1 : SelectedTable2;

            if (selectedTable == null)
            {
                MessageBox.Show("未选择表。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string topClause = "";
            string tableName = selectedTable.Name;
            string dbKey = dbIndex == 1 ? "source" : "target";

            long count = await DatabaseService.Instance.GetTableCountAsync(dbKey, tableName);
            if (count > MaxRowCount)
            {
                topClause = $"TOP {MaxRowCount} ";
            }

            var sqlBuilder = new System.Text.StringBuilder();
            sqlBuilder.Append("SELECT ");
            if (!string.IsNullOrEmpty(topClause))
            {
                sqlBuilder.Append(topClause);
            }
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("    " + string.Join("," + Environment.NewLine + "    ", selectedColumnNames));
            sqlBuilder.AppendLine($"FROM [{tableName}]");

            var sortColumns = dbIndex == 1 ? SortColumns1 : SortColumns2;
            if (sortColumns.Any())
            {
                var columns = dbIndex == 1 ? Columns1 : Columns2;
                var sortClauses = sortColumns.Select((sc, index) =>
                {
                    var colInfo = columns.FirstOrDefault(c => c.Column.DbColumnName.Equals(sc.ColumnName, StringComparison.OrdinalIgnoreCase))?.Column;

                    // 特殊处理：如果目标列是文本而源列是整数，则尝试将目标列转为整数排序
                    if (dbIndex == 2 && colInfo != null && colInfo.DataType.ToLower().Contains("char"))
                    {
                        if (index < SortColumns1.Count)
                        {
                            var sourceSortColumnName = SortColumns1[index].ColumnName;
                            var sourceColInfo = Columns1.FirstOrDefault(c => c.Column.DbColumnName.Equals(sourceSortColumnName, StringComparison.OrdinalIgnoreCase))?.Column;
                            if (sourceColInfo != null && sourceColInfo.DataType.ToLower() == "int")
                            {
                                return $"TRY_CAST([{sc.ColumnName}] AS INT) {(sc.Direction == SortDirection.Ascending ? "ASC" : "DESC")}";
                            }
                        }
                    }

                    // 默认行为
                    string collation = "";
                    if (colInfo != null)
                    {
                        string dataType = colInfo.DataType.ToLower();
                        if (dataType.Contains("char") || dataType.Contains("text"))
                        {
                            collation = " COLLATE Chinese_PRC_CI_AS";
                        }
                    }
                    return $"[{sc.ColumnName}]{collation} {(sc.Direction == SortDirection.Ascending ? "ASC" : "DESC")}";
                });

                sqlBuilder.AppendLine("ORDER BY");
                sqlBuilder.AppendLine("    " + string.Join("," + Environment.NewLine + "    ", sortClauses));
            }

            if (dbIndex == 1)
            {
                SqlQuery1 = sqlBuilder.ToString();
            }
            else if (dbIndex == 2)
            {
                SqlQuery2 = sqlBuilder.ToString();
            }
        }

        private void OpenConfig()
        {
            var configView = new DatabaseConfigView
            {
                Owner = Application.Current.MainWindow
            };
            configView.ShowDialog();
            CheckDatabaseConfiguration();
        }

        private async Task ExportAsync()
        {
            StatusMessage = "正在执行查询和导出...";
            try
            {
                string destinationDbKey = SelectedDestination == DestinationType.Target ? "target" : "framework";
                if (await _exportService.ExportToExcelAsync(SqlQuery1, SheetName1, SqlQuery2, SheetName2, destinationDbKey, string.Empty,string.Empty))
                {
                    StatusMessage = "文件已成功导出。";
                }
                else
                {
                    StatusMessage = "导出操作已取消。";
                }
            }
            catch
            {
                // The service itself will show a detailed error message box.
                StatusMessage = "导出失败，请检查SQL或文件权限。";
            }
        }

        private async Task PreviewAsync(string sql, string sheetName, string dbKey)
        {
            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(sheetName))
            {
                MessageBox.Show("SQL查询和Sheet名称不能为空。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StatusMessage = $"正在执行查询 ({dbKey})...";
                DataTable data = await _exportService.GetDataTableAsync(sql, dbKey);
                StatusMessage = $"查询 ({dbKey}) 完成，正在打开预览窗口...";

                var previewViewModel = new PreviewViewModel(data, sheetName, _exportService);
                var previewView = new PreviewView
                {
                    DataContext = previewViewModel,
                    Owner = Application.Current.MainWindow
                };
                previewView.Show();
                StatusMessage = "准备就绪";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行查询时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "查询出错，请检查SQL语句和数据库连接。";
            }
        }

        private async Task PreviewBothAsync()
        {
            try
            {
                StatusMessage = "正在执行两个查询...";
                var task1 = _exportService.GetDataTableAsync(SqlQuery1, "source");
                string destinationDbKey = SelectedDestination == DestinationType.Target ? "target" : "framework";
                var task2 = _exportService.GetDataTableAsync(SqlQuery2, destinationDbKey);

                await Task.WhenAll(task1, task2);

                DataTable dt1 = task1.Result;
                DataTable dt2 = task2.Result;

                StatusMessage = "查询完成，正在打开组合预览窗口...";

                var dualViewModel = new DualPreviewViewModel(dt1, dt2, _exportService);
                var dualView = new DualPreviewView
                {
                    DataContext = dualViewModel,
                    Owner = Application.Current.MainWindow
                };
                dualView.Show();
                StatusMessage = "准备就绪";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行查询时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "查询出错，请检查SQL语句和数据库连接。";
            }
        }

        private bool _isDarkTheme = false;

        private void SwitchTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            _themeService.ChangeTheme(_isDarkTheme ? "Dark" : "Default");
        }

        private bool CanSaveConfig()
        {
            return IsCoreFunctionalityEnabled &&
                   !string.IsNullOrWhiteSpace(SqlQuery1) &&
                   !string.IsNullOrWhiteSpace(SqlQuery2) &&
                   !string.IsNullOrWhiteSpace(SheetName1) &&
                   !string.IsNullOrWhiteSpace(SheetName2) &&
                   SortColumns1.Count > 0 &&
                   SortColumns2.Count > 0 &&
                   SortColumns1.Count == SortColumns2.Count;
        }

        private async Task SaveConfigAsync()
        {
            if (SortColumns1.Count == 0 || SortColumns2.Count == 0)
            {
                MessageBox.Show("源和目标都必须设置排序字段。", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SortColumns1.Count != SortColumns2.Count)
            {
                MessageBox.Show("源和目标的排序字段数量必须一致。", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveConfigViewModel saveConfigViewModel;
                
                if (_isEditMode)
                {
                    // 编辑模式：使用现有配置创建ViewModel
                    saveConfigViewModel = new SaveConfigViewModel(_originalConfig ?? new Models.BatchExportConfig());
                }
                else
                {
                    // 新增模式：生成排序键并创建ViewModel
                    string sourceSortKey = "Key: " + string.Join(",", SortColumns1.Select(c => c.ColumnName));
                    string targetSortKey = "Key: " + string.Join(",", SortColumns2.Select(c => c.ColumnName));
                    saveConfigViewModel = new SaveConfigViewModel(SqlQuery1, SheetName1, SqlQuery2, SheetName2, sourceSortKey, targetSortKey);
                }
                
                var saveConfigDialog = new SaveConfigDialog
                {
                    DataContext = saveConfigViewModel,
                    Owner = Application.Current.MainWindow
                };

                if (saveConfigDialog.ShowDialog() == true)
                {
                    var config = new BatchExportConfig
                    {
                        Key = saveConfigViewModel.ConfigKey,
                        Destination = this.SelectedDestination, // 保存目标选择
                        DataSource = new QueryConfig
                        {
                            SheetName = SheetName1,
                            TableName = SelectedTable1?.Name,
                            Sql = SqlQuery1,
                            Description = saveConfigViewModel.SourceDescription
                        },
                        DataTarget = new QueryConfig
                        {
                            SheetName = SheetName2,
                            TableName = SelectedTable2?.Name,
                            Sql = SqlQuery2,
                            Description = saveConfigViewModel.TargetDescription
                        }
                    };

                    // 如果是编辑模式且Key改变了，则先删除原配置
                    bool shouldDeleteOriginal = _isEditMode && !string.IsNullOrEmpty(_editingConfigKey) &&
                                              !_editingConfigKey.Equals(config.Key, StringComparison.OrdinalIgnoreCase);

                    if (shouldDeleteOriginal)
                    {
                        await ConfigService.Instance.DeleteConfigAsync(_editingConfigKey);
                    }

                    if (await ConfigService.Instance.SaveConfigAsync(config, true))
                    {
                        string message = _isEditMode ?
                            $"配置 '{config.Key}' 已成功更新。" :
                            $"配置 '{config.Key}' 已成功保存。";
                        
                        StatusMessage = message;
                        
                        var result = MessageBox.Show(
                            _isEditMode ?
                                "配置已成功更新。是否要切换到批量导出页面查看？" :
                                "配置已成功保存。是否要切换到批量导出页面查看？",
                            _isEditMode ? "更新成功" : "保存成功",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Placeholder for switching to batch export tab
                        }

                        // 清除编辑模式状态并重置界面
                        ResetState();
                    }
                    else
                    {
                        StatusMessage = _isEditMode ? "更新配置失败。" : "保存配置失败。";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "保存配置时出错。";
            }
        }

        private async Task ImportJsonAsync()
        {
            var importViewModel = new ImportJsonViewModel();

            while (true)
            {
                var importDialog = new ImportJsonDialog
                {
                    DataContext = importViewModel,
                    Owner = Application.Current.MainWindow
                };

                if (importDialog.ShowDialog() != true || string.IsNullOrEmpty(importViewModel.ResultJson))
                {
                    // 用户在第一个对话框（输入JSON）处取消，完全退出流程
                    break;
                }

                try
                {
                    StatusMessage = "正在解析JSON配置...";
                    var mapping = JsonSerializer.Deserialize<JsonMapping>(importViewModel.ResultJson);

                    if (mapping == null || mapping.old_fields == null || mapping.new_fields == null)
                    {
                        MessageBox.Show("JSON内容无效或缺少 'old_fields' / 'new_fields'。请修改后重试。", "解析错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue; // 返回JSON输入界面
                    }

                    var previewViewModel = new JsonImportPreviewViewModel(mapping.old_fields, mapping.new_fields);
                    var previewDialog = new JsonImportPreviewDialog
                    {
                        DataContext = previewViewModel,
                        Owner = Application.Current.MainWindow
                    };

                    if (previewDialog.ShowDialog() != true)
                    {
                        StatusMessage = "已取消预览，请重新输入或修改JSON。";
                        // 用户在第二个对话框（预览）处取消，返回JSON输入界面
                        continue;
                    }

                    // 用户确认导入，开始应用配置
                    StatusMessage = "正在应用JSON配置...";

                    if (string.IsNullOrEmpty(mapping.old_table) || string.IsNullOrEmpty(mapping.new_table))
                    {
                        MessageBox.Show("JSON内容缺少必要的表信息。请修改后重试。", "配置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue; // 返回JSON输入界面
                    }

                    // 自动检测并切换目标类型
                    string targetTableName = mapping.new_table;
                    if (DatabaseService.Instance.IsTableExists("target", targetTableName))
                    {
                        if (this.SelectedDestination != DestinationType.Target)
                        {
                            this.SelectedDestination = DestinationType.Target;
                        }
                    }
                    else if (DatabaseService.Instance.IsTableExists("framework", targetTableName))
                    {
                        if (this.SelectedDestination != DestinationType.Framework)
                        {
                            this.SelectedDestination = DestinationType.Framework;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"在目标数据库和框架库中都找不到目标表: {targetTableName}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue; // 返回JSON输入界面
                    }

                    var sourceTable = Tables1.FirstOrDefault(t => t.Name.Equals(mapping.old_table, StringComparison.OrdinalIgnoreCase));
                    var targetTable = Tables2.FirstOrDefault(t => t.Name.Equals(mapping.new_table, StringComparison.OrdinalIgnoreCase));

                    if (sourceTable == null) { MessageBox.Show($"在源数据库中找不到表: {mapping.old_table}", "错误", MessageBoxButton.OK, MessageBoxImage.Error); continue; }
                    if (targetTable == null) { MessageBox.Show($"在目标数据库中找不到表: {mapping.new_table}", "错误", MessageBoxButton.OK, MessageBoxImage.Error); continue; }

                    SelectedTable1 = sourceTable;
                    SelectedTable2 = targetTable;

                    var columnsForSql1 = new List<string>();
                    _selectedColumnNames1.Clear();
                    Columns1.ToList().ForEach(c => c.IsSelected = false);
                    foreach (var colName in mapping.old_fields)
                    {
                        var col = Columns1.FirstOrDefault(c => c.Column.DbColumnName.Equals(colName, StringComparison.OrdinalIgnoreCase));
                        if (col != null) { col.IsSelected = true; _selectedColumnNames1.Add(col.Column.DbColumnName); }
                        columnsForSql1.Add(col != null ? $"[{col.Column.DbColumnName}]" : $"NULL AS [{colName}]");
                    }

                    var columnsForSql2 = new List<string>();
                    _selectedColumnNames2.Clear();
                    Columns2.ToList().ForEach(c => c.IsSelected = false);
                    foreach (var colName in mapping.new_fields)
                    {
                        var col = Columns2.FirstOrDefault(c => c.Column.DbColumnName.Equals(colName, StringComparison.OrdinalIgnoreCase));
                        if (col != null) { col.IsSelected = true; _selectedColumnNames2.Add(col.Column.DbColumnName); }
                        columnsForSql2.Add(col != null ? $"[{col.Column.DbColumnName}]" : $"NULL AS [{colName}]");
                    }

                    await GenerateSqlAsync(1, columnsForSql1);
                    await GenerateSqlAsync(2, columnsForSql2);

                    IsJsonImported = true;
                    StatusMessage = "JSON配置已成功应用。";
                    MessageBox.Show("JSON配置已成功应用。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    break; // 成功应用后，退出循环
                }
                catch (Exception ex)
                {
                    StatusMessage = "应用配置失败。";
                    MessageBox.Show($"应用JSON配置时出错: {ex.Message}。请修改后重试。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue; // 发生任何错误都返回JSON输入界面
                }
            }
        }
 
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetState()
        {
            SelectedTable1 = null;
            SelectedTable2 = null;
            SqlQuery1 = "";
            SqlQuery2 = "";
            SheetName1 = "SourceData";
            SheetName2 = "TargetData";
            Columns1.Clear();
            Columns2.Clear();
            SortColumns1.Clear();
            SortColumns2.Clear();
            _selectedColumnNames1.Clear();
            _selectedColumnNames2.Clear();
            IsJsonImported = false;
            IsEditMode = false;
            _editingConfigKey = "";
            StatusMessage = "状态已重置。";
        }

        

        private string ExtractTableNameFromSql(string sql)
        {
            try
            {
                var fromIndex = sql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
                if (fromIndex == -1) return "";

                var fromSubstring = sql.Substring(fromIndex + 4).Trim();
                
                // 移除可能的ORDER BY子句
                var orderByIndex = fromSubstring.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
                if (orderByIndex != -1)
                {
                    fromSubstring = fromSubstring.Substring(0, orderByIndex).Trim();
                }

                // 移除方括号并获取表名
                var tableName = fromSubstring.Split(' ').FirstOrDefault()?.Trim('[', ']') ?? "";
                return tableName;
            }
            catch
            {
                return "";
            }
        }

        private void OpenTableComparison()
        {
            var viewModel = new TableComparisonViewModel();
            var window = new Window
            {
                Title = "Target表信息比对",
                Content = new TableComparisonView
                {
                    DataContext = viewModel
                },
                Width = 800,
                Height = 600,
                Owner = Application.Current.MainWindow
            };
            window.Show();
        }
    }
}