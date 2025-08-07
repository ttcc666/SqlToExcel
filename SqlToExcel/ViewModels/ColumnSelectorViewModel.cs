using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class ColumnSelectorViewModel : INotifyPropertyChanged
    {
        private string _columnText;
        private bool _isUpdating;

        public ObservableCollection<SelectableDbColumn> Columns { get; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public string ColumnText
        {
            get => _columnText;
            set
            {
                _columnText = value;
                OnPropertyChanged();
                if (!_isUpdating)
                {
                    UpdateSelectionFromText();
                }
            }
        }

        public ColumnSelectorViewModel(IEnumerable<SelectableDbColumn> columns)
        {
            Columns = new ObservableCollection<SelectableDbColumn>(columns);
            _columnText = string.Join(Environment.NewLine, Columns.Where(c => c.IsSelected).Select(c => c.Column.DbColumnName));

            foreach (var col in Columns)
            {
                col.PropertyChanged += Column_PropertyChanged;
            }

            OkCommand = new RelayCommand(p => CloseWindow(p, true));
            CancelCommand = new RelayCommand(p => CloseWindow(p, false));
        }

        private void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableDbColumn.IsSelected) && !_isUpdating)
            {
                UpdateTextFromSelection();
            }
        }

        private void UpdateTextFromSelection()
        {
            _isUpdating = true;
            ColumnText = string.Join(Environment.NewLine, Columns.Where(c => c.IsSelected).Select(c => c.Column.DbColumnName));
            _isUpdating = false;
        }

        private void UpdateSelectionFromText()
        {
            _isUpdating = true;
            var selectedNames = new HashSet<string>(
                ColumnText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var col in Columns)
            {
                col.IsSelected = selectedNames.Contains(col.Column.DbColumnName);
            }
            _isUpdating = false;
        }

        private void CloseWindow(object? parameter, bool dialogResult)
        {
            if (parameter is System.Windows.Window window)
            {
                window.DialogResult = dialogResult;
                window.Close();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}