using SqlToExcel.Models;
using SqlToExcel.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class BatchExportConfigItemViewModel : INotifyPropertyChanged
    {
        private readonly BatchExportConfig _config;
        private readonly ExcelExportService _exportService;
        private string _status = "Ready";

        public string Key => _config.Key;
        public string SourceDescription => _config.DataSource.Description;
        public string TargetDescription => _config.DataTarget.Description;

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public ICommand ExportCommand { get; }

        public BatchExportConfigItemViewModel(BatchExportConfig config, ExcelExportService exportService)
        {
            _config = config;
            _exportService = exportService;
            ExportCommand = new RelayCommand(async p => await ExportAsync(), p => Status != "Exporting...");
        }

        private async Task ExportAsync()
        {
            Status = "Exporting...";
            try
            {
                await _exportService.ExportToExcelAsync(
                    _config.DataSource.Sql,
                    _config.DataSource.SheetName,
                    _config.DataTarget.Sql,
                    _config.DataTarget.SheetName,
                    _config.Key);
                Status = "Success";
            }
            catch (Exception ex)
            {
                Status = $"Failed: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
