using System;

namespace SqlToExcel.Models
{
    public class MissingTable
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime ComparisonDate { get; set; }
    }
}
