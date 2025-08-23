using SqlToExcel.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlToExcel.ViewModels
{
    public class EditBatchSqlViewModel : INotifyPropertyChanged
    {
        private readonly BatchExportConfig _config;

        public string Key => _config.Key;

        private string _sourceSql;
        public string SourceSql
        {
            get => _sourceSql;
            set { _sourceSql = value; OnPropertyChanged(); }
        }

        private string _targetSql;
        public string TargetSql
        {
            get => _targetSql;
            set { _targetSql = value; OnPropertyChanged(); }
        }

        private string _sourceDescription;
        public string SourceDescription
        {
            get => _sourceDescription;
            set { _sourceDescription = value; OnPropertyChanged(); }
        }

        private string _targetDescription;
        public string TargetDescription
        {
            get => _targetDescription;
            set { _targetDescription = value; OnPropertyChanged(); }
        }

        private DestinationType _destination;
        public DestinationType Destination
        {
            get => _destination;
            set { _destination = value; OnPropertyChanged(); }
        }

        public EditBatchSqlViewModel(BatchExportConfig config)
        {
            _config = config;
            _sourceSql = config.DataSource.Sql;
            _targetSql = config.DataTarget.Sql;
            _sourceDescription = config.DataSource.Description;
            _targetDescription = config.DataTarget.Description;
            _destination = config.Destination;
        }

        public void SaveChanges()
        {
            _config.DataSource.Sql = SourceSql;
            _config.DataTarget.Sql = TargetSql;
            _config.DataSource.Description = SourceDescription;
            _config.DataTarget.Description = TargetDescription;
            _config.Destination = Destination;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
