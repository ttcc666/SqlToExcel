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
        private readonly List<SelectableDbColumn> _selectedColumnsInOrder;

        public ObservableCollection<SelectableDbColumn> Columns { get; }
        public List<string> SelectedColumnNamesInOrder { get; private set; }
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

        public ColumnSelectorViewModel(IEnumerable<SelectableDbColumn> columns, IEnumerable<string> previouslySelectedNames)
        {
            Columns = new ObservableCollection<SelectableDbColumn>(columns);
            SelectedColumnNamesInOrder = new List<string>(previouslySelectedNames);

            _selectedColumnsInOrder = new List<SelectableDbColumn>();
            foreach (var name in SelectedColumnNamesInOrder)
            {
                var column = Columns.FirstOrDefault(c => c.Column.DbColumnName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (column != null)
                {
                    column.IsSelected = true;
                    if (!_selectedColumnsInOrder.Contains(column))
                    {
                        _selectedColumnsInOrder.Add(column);
                    }
                }
            }
            
            _columnText = string.Join(Environment.NewLine, SelectedColumnNamesInOrder);

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
                var column = sender as SelectableDbColumn;
                if (column != null)
                {
                    if (column.IsSelected)
                    {
                        if (!_selectedColumnsInOrder.Contains(column))
                        {
                            _selectedColumnsInOrder.Add(column);
                        }
                    }
                    else
                    {
                        _selectedColumnsInOrder.Remove(column);
                    }
                    UpdateTextFromSelection();
                }
            }
        }

        private void UpdateTextFromSelection()
        {
            _isUpdating = true;
            ColumnText = string.Join(Environment.NewLine, _selectedColumnsInOrder.Select(c => c.Column.DbColumnName));
            SelectedColumnNamesInOrder = _selectedColumnsInOrder.Select(c => c.Column.DbColumnName).ToList();
            _isUpdating = false;
        }

        private void UpdateSelectionFromText()
        {
            _isUpdating = true;
            var selectedNames = ColumnText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var selectedNamesSet = new HashSet<string>(selectedNames, StringComparer.OrdinalIgnoreCase);

            _selectedColumnsInOrder.Clear();

            foreach (var col in Columns)
            {
                col.IsSelected = selectedNamesSet.Contains(col.Column.DbColumnName);
            }

            // Re-populate _selectedColumnsInOrder in the new order from the text
            foreach (var name in selectedNames)
            {
                var column = Columns.FirstOrDefault(c => c.Column.DbColumnName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (column != null && !_selectedColumnsInOrder.Contains(column))
                {
                    _selectedColumnsInOrder.Add(column);
                }
            }
            SelectedColumnNamesInOrder = _selectedColumnsInOrder.Select(c => c.Column.DbColumnName).ToList();
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