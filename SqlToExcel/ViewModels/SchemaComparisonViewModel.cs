using SqlToExcel.Models;
using SqlToExcel.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class SchemaComparisonViewModel : INotifyPropertyChanged
    {
        private readonly ConfigService _configService;
        private readonly DatabaseService _databaseService;
        private readonly ExcelExportService _excelExportService;
        private bool _isLoading;

        public ObservableCollection<SchemaComparisonResult> ComparisonResults { get; } = new ObservableCollection<SchemaComparisonResult>();

        public ICommand ExportCommand { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public SchemaComparisonViewModel()
        {
            _configService = ConfigService.Instance;
            _databaseService = DatabaseService.Instance;
            _excelExportService = new ExcelExportService();

            ExportCommand = new RelayCommand(async _ => await ExportAsync(), _ => ComparisonResults.Any());

            EventService.Subscribe<MappingsChangedEvent>(async e => await LoadComparisonDataAsync());
            _ = LoadComparisonDataAsync();
        }

        private async Task ExportAsync()
        {
            if (!ComparisonResults.Any())
            {
                System.Windows.MessageBox.Show("没有可导出的数据。", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }
            await _excelExportService.ExportSchemaComparisonAsync(ComparisonResults);
        }

        private async Task LoadComparisonDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ComparisonResults.Clear();

            try
            {
                var mappings = await _configService.GetTableMappingsAsync();
                foreach (var mapping in mappings)
                {
                    var sourcePks = await _databaseService.GetPrimaryKeysAsync("source", mapping.SourceTable);
                    var sourceIndexDetails = await _databaseService.GetIndexDetailsAsync("source", mapping.SourceTable);
                    
                    var targetPks = await _databaseService.GetPrimaryKeysAsync("target", mapping.TargetTable);
                    var targetIndexDetails = await _databaseService.GetIndexDetailsAsync("target", mapping.TargetTable);

                    var result = new SchemaComparisonResult
                    {
                        SourceTableName = mapping.SourceTable,
                        SourcePrimaryKeys = string.Join("\n", sourcePks),
                        SourceIndexes = FormatIndexDetails(sourceIndexDetails, sourcePks),
                        TargetTableName = mapping.TargetTable,
                        TargetPrimaryKeys = string.Join("\n", targetPks),
                        TargetIndexes = FormatIndexDetails(targetIndexDetails, targetPks)
                    };
                    ComparisonResults.Add(result);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string FormatIndexDetails(List<IndexDetail> indexDetails, List<string> primaryKeys)
        {
            if (indexDetails == null || !indexDetails.Any())
            {
                return string.Empty;
            }

            var formattedIndexes = indexDetails
                .GroupBy(i => i.IndexName)
                .Where(g => !primaryKeys.Contains(g.Key)) // Exclude primary key indexes
                .Select(g =>
                {
                    var indexName = g.Key;
                    var indexType = g.First().IndexType.Replace("_", " ");
                    var columns = g.Where(i => !i.IsIncludedColumn).Select(i => i.ColumnName);
                    var includedColumns = g.Where(i => i.IsIncludedColumn).Select(i => i.ColumnName);

                    var columnString = string.Join(", ", columns);
                    var includedString = includedColumns.Any() ? $" INCLUDE ({string.Join(", ", includedColumns)})" : "";

                    return $"{indexName} ({indexType}): [{columnString}]{includedString}";
                });

            return string.Join("\n", formattedIndexes);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}