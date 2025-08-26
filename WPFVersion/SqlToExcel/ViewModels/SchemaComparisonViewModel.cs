using SqlToExcel.Models;
using SqlToExcel.Services;
using SqlToExcel.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System;

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
        public ICommand RefreshCommand { get; }

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

            ExportCommand = new RelayCommand(async _ => await ExportAsync(), _ => ComparisonResults != null && ComparisonResults.Any());
            RefreshCommand = new RelayCommand(async _ => await LoadComparisonDataAsync());

            EventService.Subscribe<MappingsChangedEvent>(async e => await LoadComparisonDataAsync());
            _ = LoadComparisonDataAsync();
        }

        private async Task ExportAsync()
        {
            if (ComparisonResults == null || !ComparisonResults.Any())
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
                if (mappings == null) { return; }
                var validMappings = mappings.Where(m => m != null).ToList();

                // Use OrdinalIgnoreCase for case-insensitive comparison of table names
                var mappedTargetTables = new HashSet<string>(
                    validMappings.Select(m => m.TargetTable),
                    StringComparer.OrdinalIgnoreCase
                );

                // 1. Process mapped tables
                foreach (var mapping in validMappings)
                {
                    var sourcePks = await _databaseService.GetPrimaryKeysAsync("source", mapping.SourceTable);
                    var sourceIndexDetailsRaw = await _databaseService.GetIndexDetailsAsync("source", mapping.SourceTable);

                    var targetPks = await _databaseService.GetPrimaryKeysAsync("target", mapping.TargetTable);
                    var targetIndexDetailsRaw = await _databaseService.GetIndexDetailsAsync("target", mapping.TargetTable);

                    var result = new SchemaComparisonResult
                    {
                        SourceTableName = mapping.SourceTable,
                        SourcePrimaryKeys = string.Join("\n", sourcePks),
                        SourceIndexes = ProcessIndexDetails(sourceIndexDetailsRaw),
                        TargetTableName = mapping.TargetTable,
                        TargetPrimaryKeys = string.Join("\n", targetPks),
                        TargetIndexes = ProcessIndexDetails(targetIndexDetailsRaw)
                    };
                    ComparisonResults.Add(result);
                }

                // 2. Process unmapped tables
                var allTargetTables = await _databaseService.GetTablesAsync("target");
                if (allTargetTables == null) { return; } // Nothing to compare against

                var unmappedTables = allTargetTables.Where(t => !mappedTargetTables.Contains(t));

                foreach (var tableName in unmappedTables)
                {
                    var targetPks = await _databaseService.GetPrimaryKeysAsync("target", tableName);
                    var targetIndexDetailsRaw = await _databaseService.GetIndexDetailsAsync("target", tableName);

                    var result = new SchemaComparisonResult
                    {
                        SourceTableName = "(无对应源表)",
                        SourcePrimaryKeys = string.Empty,
                        SourceIndexes = new List<IndexDetailViewModel>(),
                        TargetTableName = tableName,
                        TargetPrimaryKeys = string.Join("\n", targetPks),
                        TargetIndexes = ProcessIndexDetails(targetIndexDetailsRaw)
                    };
                    ComparisonResults.Add(result);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private List<IndexDetailViewModel> ProcessIndexDetails(List<IndexDetail> rawDetails)
        {
            if (rawDetails == null || !rawDetails.Any())
            {
                return new List<IndexDetailViewModel>();
            }

            var groupedIndexes = rawDetails.GroupBy(i => i.IndexName);
            var processedList = new List<IndexDetailViewModel>();

            foreach (var group in groupedIndexes)
            {
                var first = group.First();
                var columns = group.Select(i => i.ColumnName).Where(c => !string.IsNullOrEmpty(c)).Distinct();

                var newDetail = new IndexDetailViewModel
                {
                    IndexName = group.Key,
                    IsPrimaryKey = first.IsPrimaryKey,
                    IsUnique = first.IsUnique,
                    IsClustered = first.IsClustered,
                    IsNonClustered = first.IsNonClustered,
                    ColumnsDisplay = string.Join(", ", columns)
                };
                processedList.Add(newDetail);
            }

            return processedList;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}