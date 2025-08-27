namespace SqlToExcel.Models
{
    public class JsonTableMapping
    {
        public required string source_table { get; set; }
        public required string target_table { get; set; }
    }
}
