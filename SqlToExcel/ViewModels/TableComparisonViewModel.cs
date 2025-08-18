using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using SqlToExcel.Services;
using SqlToExcel.ViewModels;

namespace SqlToExcel.ViewModels
{
    public class TableComparisonViewModel : INotifyPropertyChanged
    {
        private string _jsonSourceTables;
        private readonly DatabaseService _databaseService;
        private readonly ConfigService _configService;

        public event PropertyChangedEventHandler PropertyChanged;

        public string JsonSourceTables
        {
            get => _jsonSourceTables;
            set
            {
                _jsonSourceTables = value;
                OnPropertyChanged(nameof(JsonSourceTables));
            }
        }

        public ObservableCollection<string> MissingTablesResult { get; } = new ObservableCollection<string>();

        public RelayCommand CompareCommand { get; }

        public TableComparisonViewModel()
        {
            _databaseService = DatabaseService.Instance;
            _configService = ConfigService.Instance;
            CompareCommand = new RelayCommand(async _ => await ExecuteCompare(), _ => !string.IsNullOrWhiteSpace(JsonSourceTables));
        }

        private async Task ExecuteCompare()
        {
            try
            {
                var sourceTables = JsonConvert.DeserializeObject<string[]>(JsonSourceTables);
                if (sourceTables == null)
                {
                    MessageBox.Show("JSON格式无效。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var targetTables = await _databaseService.GetTableNamesAsync();

                var missingTables = targetTables.Except(sourceTables, System.StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();

                MissingTablesResult.Clear();
                foreach (var table in missingTables)
                {
                    MissingTablesResult.Add(table);
                }

                //await _configService.SaveMissingTablesAsync(missingTables);

                MessageBox.Show($"比对完成！发现 {missingTables.Count} 个差异表。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (JsonException)
            {
                MessageBox.Show("无法解析JSON，请检查格式是否为 [\"table1\", \"table2\"]。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"发生未知错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}