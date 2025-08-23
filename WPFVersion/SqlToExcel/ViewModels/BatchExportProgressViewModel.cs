using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class BatchExportProgressViewModel : INotifyPropertyChanged
    {
        private int _currentProgress = 0;
        private int _totalCount = 0;
        private string _currentItemText = "";
        private StringBuilder _logBuilder = new StringBuilder();
        private bool _isCompleted = false;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public int CurrentProgress
        {
            get => _currentProgress;
            set { _currentProgress = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgressPercentage)); }
        }

        public int TotalCount
        {
            get => _totalCount;
            set { _totalCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgressPercentage)); }
        }

        public double ProgressPercentage
        {
            get => TotalCount > 0 ? (double)CurrentProgress / TotalCount * 100 : 0;
        }

        public string CurrentItemText
        {
            get => _currentItemText;
            set { _currentItemText = value; OnPropertyChanged(); }
        }

        public string LogText
        {
            get => _logBuilder.ToString();
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); }
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? RequestClose;

        public BatchExportProgressViewModel()
        {
            CancelCommand = new RelayCommand(_ => Cancel());
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        public void AddLog(string message)
        {
            _logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            OnPropertyChanged(nameof(LogText));
        }

        public void UpdateProgress(int current, string currentItem)
        {
            CurrentProgress = current;
            CurrentItemText = $"正在处理: {currentItem}";
        }

        public void Complete()
        {
            IsCompleted = true;
            CurrentItemText = "批量导出完成";
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
            AddLog("用户取消了批量导出操作");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}