using SqlToExcel.Models;
using System.Collections.ObjectModel;

namespace SqlToExcel.ViewModels
{
    public class ValidationResultViewModel
    {
        public ObservableCollection<ValidationResultItem> Results { get; }
        public string Summary { get; }

        public ValidationResultViewModel(ObservableCollection<ValidationResultItem> results, string summary)
        {
            Results = results;
            Summary = summary;
        }
    }
}
