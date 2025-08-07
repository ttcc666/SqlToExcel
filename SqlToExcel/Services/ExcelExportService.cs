using Microsoft.Win32;
using MiniExcelLibs;
using SqlToExcel.ViewModels;
using System.Data;
using System.Windows;

namespace SqlToExcel.Services
{
    public class ExcelExportService
    {
        public async Task<DataTable> GetDataTableAsync(string sql, string dbKey)
        {
            var db = DatabaseService.Instance.Db;
            if (db == null)
            {
                throw new InvalidOperationException("数据库连接未初始化。");
            }
            return await db.GetConnection(dbKey.ToLower()).Ado.GetDataTableAsync(sql);
        }

        public async Task ExportToExcel(MainViewModel vm)
        {
            try
            {
                vm.StatusMessage = "正在执行查询...";
                var task1 = GetDataTableAsync(vm.SqlQuery1, "source");
                var task2 = GetDataTableAsync(vm.SqlQuery2, "target");

                await Task.WhenAll(task1, task2);

                DataTable dt1 = task1.Result;
                DataTable dt2 = task2.Result;

                vm.StatusMessage = "正在准备要导出的数据...";

                var sqlLog = new List<object>
                {
                    new { SheetName = vm.SheetName1, SQL_Query = vm.SqlQuery1 },
                    new { SheetName = vm.SheetName2, SQL_Query = vm.SqlQuery2 }
                };

                var sheets = new Dictionary<string, object>
                {
                    [vm.SheetName1] = dt1,
                    [vm.SheetName2] = dt2,
                    ["SQL查询记录"] = sqlLog
                };

                SaveSheetsToFile(sheets, $"Export_All_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                vm.StatusMessage = "文件已成功导出。";
            }
            catch (Exception ex)
            {
                vm.StatusMessage = $"导出失败: {ex.Message}";
                MessageBox.Show($"导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportSingleSheet(DataTable data, string sheetName)
        {
            try
            {
                var sheets = new Dictionary<string, object>
                {
                    [sheetName] = data
                };

                SaveSheetsToFile(sheets, $"Export_{sheetName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                MessageBox.Show("Excel 文件已成功导出。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSheetsToFile(IDictionary<string, object> sheets, string defaultFileName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "保存 Excel 文件",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                MiniExcel.SaveAs(saveFileDialog.FileName, sheets);
            }
        }
    }
}