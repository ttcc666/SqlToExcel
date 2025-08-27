namespace SqlToExcel.ViewModels
{
    public class IndexDetailViewModel
    {
        public required string IndexName { get; set; }
        public required string ColumnsDisplay { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        public bool IsNonClustered { get; set; }
    }
}
