using SqlToExcel.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class BatchExportConfigItemViewModel : INotifyPropertyChanged
    {
        public BatchExportConfig Config { get; }
        private string _status = "准备就绪";
        private bool _isSelected = false;

        public string Key => Config.Key;
        public string SourceDescription => Config.DataSource.Description;
        public string TargetDescription => Config.DataTarget.Description;

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        // Commands are now handled by the parent BatchExportViewModel
        public ICommand? ExportCommand { get; set; }
        public ICommand? PreviewCommand { get; set; }
        public ICommand? DeleteCommand { get; set; }
        public ICommand? EditCommand { get; set; }


        public BatchExportConfigItemViewModel(BatchExportConfig config)
        {
            Config = config;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
