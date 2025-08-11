using SqlToExcel.Models;
using SqlToExcel.Services;
using SqlSugar;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class TableMappingViewModel : INotifyPropertyChanged
    {
        private readonly ConfigService _configService;
        private readonly DatabaseService _databaseService;

        public ObservableCollection<DbTableInfo> AvailableSourceTables { get; } = new ObservableCollection<DbTableInfo>();
        public ObservableCollection<DbTableInfo> AvailableTargetTables { get; } = new ObservableCollection<DbTableInfo>();
        public ObservableCollection<TableMapping> MappedTables { get; } = new ObservableCollection<TableMapping>();

        private DbTableInfo _selectedSourceTable;
        public DbTableInfo SelectedSourceTable
        {
            get => _selectedSourceTable;
            set { _selectedSourceTable = value; OnPropertyChanged(); }
        }

        private DbTableInfo _selectedTargetTable;
        public DbTableInfo SelectedTargetTable
        {
            get => _selectedTargetTable;
            set { _selectedTargetTable = value; OnPropertyChanged(); }
        }

        public ICommand SaveMappingCommand { get; }
        public ICommand DeleteMappingCommand { get; }

        public TableMappingViewModel()
        {
            _configService = ConfigService.Instance;
            _databaseService = DatabaseService.Instance;

            SaveMappingCommand = new RelayCommand(async p => await SaveMappingAsync(), p => SelectedSourceTable != null && SelectedTargetTable != null);
            DeleteMappingCommand = new RelayCommand(async p => await DeleteMappingAsync(p));

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var allSourceTables = _databaseService.GetTables("source");
            var allTargetTables = _databaseService.GetTables("target");
            var mapped = await _configService.GetTableMappingsAsync();

            MappedTables.Clear();
            foreach (var m in mapped)
            {
                MappedTables.Add(m);
            }

            var mappedSourceNames = mapped.Select(m => m.SourceTable).ToHashSet();
            var mappedTargetNames = mapped.Select(m => m.TargetTable).ToHashSet();

            AvailableSourceTables.Clear();
            foreach (var table in allSourceTables.Where(t => !mappedSourceNames.Contains(t.Name)))
            {
                AvailableSourceTables.Add(table);
            }

            AvailableTargetTables.Clear();
            foreach (var table in allTargetTables.Where(t => !mappedTargetNames.Contains(t.Name)))
            {
                AvailableTargetTables.Add(table);
            }

            SelectedSourceTable = AvailableSourceTables.FirstOrDefault();
            SelectedTargetTable = AvailableTargetTables.FirstOrDefault();
        }

        private async Task SaveMappingAsync()
        {
            var newMapping = new TableMapping
            {
                SourceTable = SelectedSourceTable.Name,
                TargetTable = SelectedTargetTable.Name
            };

            await _configService.SaveTableMappingAsync(newMapping);
            await LoadDataAsync();
        }

        private async Task DeleteMappingAsync(object parameter)
        {
            if (parameter is int id)
            {
                await _configService.DeleteTableMappingAsync(id);
                await LoadDataAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}