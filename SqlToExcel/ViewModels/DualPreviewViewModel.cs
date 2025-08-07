using System.Data;

namespace SqlToExcel.ViewModels
{
    public class DualPreviewViewModel
    {
        public DataTable Data1 { get; }
        public DataTable Data2 { get; }
        public int RecordCount1 => Data1.Rows.Count;
        public int RecordCount2 => Data2.Rows.Count;

        public DualPreviewViewModel(DataTable data1, DataTable data2)
        {
            Data1 = data1;
            Data2 = data2;
        }
    }
}