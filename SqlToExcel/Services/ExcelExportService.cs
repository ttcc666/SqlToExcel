using Microsoft.Win32;
using OfficeOpenXml;
using SqlToExcel.ViewModels;
using System.Data;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Windows;

namespace SqlToExcel.Services
{
    public class ExcelExportService
    {
        static ExcelExportService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<DataTable> GetDataTableAsync(string sql, string dbKey)
        {
            var db = DatabaseService.Instance.Db;
            if (db == null)
            {
                throw new InvalidOperationException("数据库连接未初始化。");
            }

            var dt = await db.GetConnection(dbKey.ToLower()).Ado.GetDataTableAsync(sql);

            // Correct column names
            var tableName = ExtractTableNameFromSql(sql);
            if (!string.IsNullOrEmpty(tableName))
            {
                var columns = DatabaseService.Instance.GetColumns(dbKey, tableName);
                if (columns.Count == dt.Columns.Count)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dt.Columns[i].ColumnName = columns[i].DbColumnName;
                    }
                }
            }

            return dt;
        }

        private string ExtractTableNameFromSql(string sql)
        {
            try
            {
                var fromIndex = sql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
                if (fromIndex == -1) return string.Empty;

                var fromSubstring = sql.Substring(fromIndex + 4).Trim();
                
                var orderByIndex = fromSubstring.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
                if (orderByIndex != -1)
                {
                    fromSubstring = fromSubstring.Substring(0, orderByIndex).Trim();
                }

                return fromSubstring.Split(' ').FirstOrDefault() ?? string.Empty;
            }
            catch
            {
                return string.Empty; // Fail silently if parsing fails
            }
        }

        public async Task ExportToExcelAsync(string sqlSource, string sheetNameSource, string sqlTarget, string sheetNameTarget, string exportKey)
        {
            try
            {
                var task1 = GetDataTableAsync(sqlSource, "source");
                var task2 = GetDataTableAsync(sqlTarget, "target");

                await Task.WhenAll(task1, task2);

                DataTable dt1 = task1.Result;
                DataTable dt2 = task2.Result;

                var sqlLog = new List<object>
                {
                    new { SheetName = sheetNameSource, SQL_Query = sqlSource },
                    new { SheetName = sheetNameTarget, SQL_Query = sqlTarget }
                };

                var sheets = new Dictionary<string, object>
                {
                    [sheetNameSource] = dt1,
                    [sheetNameTarget] = dt2,
                    ["SQL查询记录"] = sqlLog
                };

                SaveSheetsToFile(sheets, $"Export_{exportKey}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                MessageBox.Show("Excel 文件已成功导出。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Re-throw to be caught by the caller if needed
            }
        }

        public async Task ExportToExcel(MainViewModel vm)
        {
            try
            {
                vm.StatusMessage = "正在执行查询...";
                await ExportToExcelAsync(vm.SqlQuery1, vm.SheetName1, vm.SqlQuery2, vm.SheetName2, "All");
                vm.StatusMessage = "文件已成功导出。";
            }
            catch (Exception ex)
            {
                vm.StatusMessage = $"导出失败: {ex.Message}";
                // The message box is already shown in the new method, so we don't need it here.
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
                using (var package = new ExcelPackage(new FileInfo(saveFileDialog.FileName)))
                {
                    foreach (var sheet in sheets)
                    {
                        var worksheet = package.Workbook.Worksheets.Add(sheet.Key);

                        if (sheet.Value is DataTable dt)
                        {
                            // Manually load data to enforce text format
                            // Header
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                worksheet.Cells[1, i + 1].Value = dt.Columns[i].ColumnName;
                            }

                            // Data
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    var cell = worksheet.Cells[i + 2, j + 1];
                                    cell.Value = dt.Rows[i][j].ToString();
                                    cell.Style.Numberformat.Format = "@";
                                }
                            }
                        }
                        else
                        {
                            worksheet.Cells["A1"].LoadFromCollection(sheet.Value as IEnumerable<object>, true);
                        }

                        var dataRange = worksheet.Dimension;
                        if (dataRange == null) continue;

                        // Style the header
                        var header = worksheet.Cells[1, 1, 1, dataRange.End.Column];
                        header.Style.Font.Bold = true;
                        header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        header.Style.Fill.BackgroundColor.SetColor(Color.DodgerBlue);
                        header.Style.Font.Color.SetColor(Color.White);
                        header.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        header.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        // Add AutoFilter
                        worksheet.Cells[dataRange.Address].AutoFilter = true;
                        
                        // Custom column width adjustment first
                        for (int i = 1; i <= dataRange.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Column(i).Width = worksheet.Column(i).Width + 1;
                        }

                        // Then enable WrapText
                        worksheet.Cells[dataRange.Address].Style.WrapText = true;
                    }
                    package.Save();
                }
            }
        }
    }
}
