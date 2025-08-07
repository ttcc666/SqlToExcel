using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class ColumnSortViewModel : INotifyPropertyChanged
    {
        private SortableColumn? _selectedAvailableColumn;
        private SortableColumn? _selectedSortColumn;

        public ObservableCollection<SortableColumn> AvailableColumns { get; }
        public ObservableCollection<SortableColumn> SortColumns { get; }

        public ICommand AddColumnCommand { get; }
        public ICommand RemoveColumnCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public SortableColumn? SelectedAvailableColumn
        {
            get => _selectedAvailableColumn;
            set { _selectedAvailableColumn = value; OnPropertyChanged(); }
        }

        public SortableColumn? SelectedSortColumn
        {
            get => _selectedSortColumn;
            set
            {
                _selectedSortColumn = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<SortDirection> SortDirections => Enum.GetValues(typeof(SortDirection)).Cast<SortDirection>();

        public ColumnSortViewModel(IEnumerable<string> selectedColumns, IEnumerable<SortableColumn> existingSortColumns)
        {
            AvailableColumns = new ObservableCollection<SortableColumn>(selectedColumns.Select(c => new SortableColumn(c)));
            SortColumns = new ObservableCollection<SortableColumn>(existingSortColumns);

            AddColumnCommand = new RelayCommand(p => AddColumn(), p => CanAddColumn());
            RemoveColumnCommand = new RelayCommand(p => RemoveColumn(), p => CanRemoveColumn());
            MoveUpCommand = new RelayCommand(p => MoveUp(), p => CanMoveUp());
            MoveDownCommand = new RelayCommand(p => MoveDown(), p => CanMoveDown());
            OkCommand = new RelayCommand(p => CloseWindow(p, true));
            CancelCommand = new RelayCommand(p => CloseWindow(p, false));
        }

        private bool CanAddColumn() => SelectedAvailableColumn != null && !SortColumns.Any(sc => sc.ColumnName == SelectedAvailableColumn.ColumnName);
        private void AddColumn()
        {
            if (SelectedAvailableColumn != null)
            {
                SortColumns.Add(new SortableColumn(SelectedAvailableColumn.ColumnName));
            }
        }

        private bool CanRemoveColumn() => SelectedSortColumn != null;
        private void RemoveColumn()
        {
            if (SelectedSortColumn != null)
            {
                SortColumns.Remove(SelectedSortColumn);
            }
        }
        
        private bool CanMoveUp() => SelectedSortColumn != null && SortColumns.IndexOf(SelectedSortColumn) > 0;
        private void MoveUp()
        {
            if (CanMoveUp())
            {
                var index = SortColumns.IndexOf(SelectedSortColumn!);
                SortColumns.Move(index, index - 1);
            }
        }

        private bool CanMoveDown() => SelectedSortColumn != null && SortColumns.IndexOf(SelectedSortColumn) < SortColumns.Count - 1;
        private void MoveDown()
        {
            if (CanMoveDown())
            {
                var index = SortColumns.IndexOf(SelectedSortColumn!);
                SortColumns.Move(index, index + 1);
            }
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
