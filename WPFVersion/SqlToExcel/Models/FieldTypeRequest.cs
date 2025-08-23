using System.Collections.Generic;

namespace SqlToExcel.Models
{
    public class FieldTypeRequest
    {
        public string table { get; set; } = string.Empty;
        public List<string> fields { get; set; } = new List<string>();
    }
}