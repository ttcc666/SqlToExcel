using SqlToExcel.Models;
using SqlToExcel.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace SqlToExcel.ViewModels
{
    public class BatchExportViewModel
    {
        public ObservableCollection<BatchExportConfigItemViewModel> Items { get; } = new();

        public BatchExportViewModel()
        {
            LoadConfigs();
        }

        private void LoadConfigs()
        {
            try
            {
                var json = File.ReadAllText("batch_export_configs.json");
                var configs = JsonSerializer.Deserialize<List<BatchExportConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var exportService = new ExcelExportService();

                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        Items.Add(new BatchExportConfigItemViewModel(config, exportService));
                    }
                }
            }
            catch (Exception)
            {
                // Handle error loading configs, maybe show a message to the user
            }
        }
    }
}
