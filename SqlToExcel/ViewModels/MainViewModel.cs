using SqlSugar;
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

    public class MainViewModel : INotifyPropertyChanged
    {
        private string _sqlQuery1 = "";
        private string _sqlQuery2 = "";
        private string _sheetName1 = "SourceData";
        private string _sheetName2 = "TargetData";
        private string _statusMessage = "准备就绪";
        private bool _isCoreFunctionalityEnabled = false;

        public ObservableCollection<DbTableInfo> Tables1 { get; } = new();
        public ObservableCollection<DbTableInfo> Tables2 { get; } = new();
        public ObservableCollection<SelectableDbColumn> Columns1 { get; } = new();
        public ObservableCollection<SelectableDbColumn> Columns2 { get; } = new();
        public ObservableCollection<SortableColumn> SortColumns1 { get; } = new();
        public ObservableCollection<SortableColumn> SortColumns2 { get; } = new();

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
            set { _selectedTable1 = value; OnPropertyChanged(); LoadColumns(1); }
        }

        private DbTableInfo? _selectedTable2;
        public DbTableInfo? SelectedTable2
        {
            get => _selectedTable2;
            set { _selectedTable2 = value; OnPropertyChanged(); LoadColumns(2); }
        }

        private Dictionary<string, string> _tableMappings = new();

        public string SqlQuery1 { get => _sqlQuery1; set { _sqlQuery1 = value; OnPropertyChanged(); CommandManager.RequerySuggested += (s, e) => { }; } }
        public string SqlQuery2 { get => _sqlQuery2; set { _sqlQuery2 = value; OnPropertyChanged(); CommandManager.RequerySuggested += (s, e) => { }; } }
        public string SheetName1 { get => _sheetName1; set { _sheetName1 = value; OnPropertyChanged(); } }
        public string SheetName2 { get => _sheetName2; set { _sheetName2 = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public bool IsCoreFunctionalityEnabled { get => _isCoreFunctionalityEnabled; set { _isCoreFunctionalityEnabled = value; OnPropertyChanged(); } }

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

        private readonly ExcelExportService _exportService;

        public MainViewModel()
        {
            _exportService = new ExcelExportService();
            OpenConfigCommand = new RelayCommand(p => OpenConfig());
            ExitCommand = new RelayCommand(p => Application.Current.Shutdown());
            ExportCommand = new RelayCommand(async p => await ExportAsync(), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery1) && !string.IsNullOrWhiteSpace(SqlQuery2));
            Preview1Command = new RelayCommand(async p => await PreviewAsync(SqlQuery1, SheetName1, "Source"), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery1));
            Preview2Command = new RelayCommand(async p => await PreviewAsync(SqlQuery2, SheetName2, "Target"), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery2));
            PreviewBothCommand = new RelayCommand(async p => await PreviewBothAsync(), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery1) && !string.IsNullOrWhiteSpace(SqlQuery2));
            OpenColumnSelectorCommand1 = new RelayCommand(p => OpenColumnSelector(1), p => SelectedTable1 != null);
            OpenColumnSelectorCommand2 = new RelayCommand(p => OpenColumnSelector(2), p => SelectedTable2 != null);
            OpenSortDialogCommand1 = new RelayCommand(p => OpenSortDialog(1), p => Columns1.Any(c => c.IsSelected));
            OpenSortDialogCommand2 = new RelayCommand(p => OpenSortDialog(2), p => Columns2.Any(c => c.IsSelected));
            SwitchThemeCommand = new RelayCommand(p => SwitchTheme());

            LoadTableMappings();

            Tables1View = CollectionViewSource.GetDefaultView(Tables1);
            Tables1View.Filter = FilterTables1;
            Tables2View = CollectionViewSource.GetDefaultView(Tables2);
            Tables2View.Filter = FilterTables2;
        }

        public void CheckDatabaseConfiguration()
        {
            if (DatabaseService.Instance.IsConfigured())
            {
                DatabaseService.Instance.Initialize();
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

        private void LoadTableMappings()
        {
            try
            {
                var json = File.ReadAllText("table_mappings.json");
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                _tableMappings = new Dictionary<string, string>(mappings, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载表映射时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                _tableMappings = new Dictionary<string, string>();
            }
        }

        private void LoadTables()
        {
            Tables1.Clear();
            Tables2.Clear();
            DatabaseService.Instance.GetTables("source").ForEach(t => Tables1.Add(t));
            DatabaseService.Instance.GetTables("target").ForEach(t => Tables2.Add(t));
        }

        private void LoadColumns(int dbIndex)
        {
            if (dbIndex == 1 && SelectedTable1 != null)
            {
                Columns1.Clear();
                DatabaseService.Instance.GetColumns("source", SelectedTable1.Name).ForEach(c => Columns1.Add(new SelectableDbColumn(c)));

                if (_tableMappings.TryGetValue(SelectedTable1.Name, out var targetTable))
                {
                    SelectedTable2 = Tables2.FirstOrDefault(t => t.Name.Equals(targetTable, StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (dbIndex == 2 && SelectedTable2 != null)
            {
                Columns2.Clear();
                DatabaseService.Instance.GetColumns("target", SelectedTable2.Name).ForEach(c => Columns2.Add(new SelectableDbColumn(c)));
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
            var viewModel = new ColumnSelectorViewModel(columns);
            var view = new ColumnSelectorView
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            if (view.ShowDialog() == true)
            {
                // The ViewModel now handles updating the IsSelected property directly.
                // We just need to regenerate the SQL based on the new selections.
                await GenerateSqlAsync(dbIndex, columns.Where(c => c.IsSelected).ToList());
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async void OpenSortDialog(int dbIndex)
        {
            var columns = dbIndex == 1 ? Columns1 : Columns2;
            var sortColumns = dbIndex == 1 ? SortColumns1 : SortColumns2;
            var selectedColumnNames = columns.Where(c => c.IsSelected).Select(c => c.Column.DbColumnName);

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
                // Regenerate SQL after sorting
                var selectedColumns = (dbIndex == 1 ? Columns1 : Columns2).Where(c => c.IsSelected).ToList();
                await GenerateSqlAsync(dbIndex, selectedColumns);
            }
        }

        public async Task GenerateSqlAsync(int dbIndex, List<SelectableDbColumn> selectedColumns)
        {
            if (!selectedColumns.Any()) return;

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

            var selectedColumnNames = string.Join(", ", selectedColumns.Select(c => c.Column.DbColumnName));

            var sortColumns = dbIndex == 1 ? SortColumns1 : SortColumns2;
            string orderByClause = "";
            if (sortColumns.Any())
            {
                orderByClause = " ORDER BY " + string.Join(", ", sortColumns.Select(sc => $"{sc.ColumnName} {(sc.Direction == SortDirection.Ascending ? "ASC" : "DESC")}"));
            }

            if (dbIndex == 1)
            {
                SqlQuery1 = $"SELECT {topClause}{selectedColumnNames} FROM {tableName}{orderByClause}";
            }
            else if (dbIndex == 2)
            {
                SqlQuery2 = $"SELECT {topClause}{selectedColumnNames} FROM {tableName}{orderByClause}";
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
            await _exportService.ExportToExcel(this);
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
                var task2 = _exportService.GetDataTableAsync(SqlQuery2, "target");

                await Task.WhenAll(task1, task2);

                DataTable dt1 = task1.Result;
                DataTable dt2 = task2.Result;

                StatusMessage = "查询完成，正在打开组合预览窗口...";

                var dualViewModel = new DualPreviewViewModel(dt1, dt2);
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
            var app = (App)Application.Current;
            _isDarkTheme = !_isDarkTheme;
            app.UpdateTheme(_isDarkTheme ? "Dark" : "Default");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}