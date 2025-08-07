using SqlToExcel.Services;
using System.Data;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class PreviewViewModel
    {
        public DataTable Data { get; }
        public int RecordCount => Data.Rows.Count;
        public ICommand ExportCommand { get; }

        private readonly ExcelExportService _exportService;
        private readonly string _sheetName;

        public PreviewViewModel(DataTable data, string sheetName, ExcelExportService exportService)
        {
            Data = data;
            _sheetName = sheetName;
            _exportService = exportService;
            ExportCommand = new RelayCommand(p => Export());
        }

        private void Export()
        {
            _exportService.ExportSingleSheet(Data, _sheetName);
        }
    }
}