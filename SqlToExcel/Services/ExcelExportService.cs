using Microsoft.Win32;
using OfficeOpenXml;
using SqlToExcel.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Windows;
using System.Threading;
using SqlToExcel.Models;

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

        public async Task<bool> ExportToExcelAsync(string sqlSource, string sheetNameSource, string sqlTarget, string sheetNameTarget, string destinationDbKey, string sourceDescription, string targetDescription, string? fileName = null)
        {
            try
            {
                var task1 = GetDataTableAsync(sqlSource, "source");
                var task2 = GetDataTableAsync(sqlTarget, destinationDbKey);

                await Task.WhenAll(task1, task2);

                DataTable dt1 = task1.Result;
                DataTable dt2 = task2.Result;

                var sqlLog = new List<object>
                {
                    new { SheetName = sheetNameSource, SQL_Query = sqlSource, Comments=sourceDescription },
                    new { SheetName = sheetNameTarget, SQL_Query = sqlTarget, Comments=targetDescription }
                };

                var sheets = new Dictionary<string, object>
                {
                    [sheetNameSource] = dt1,
                    [sheetNameTarget] = dt2,
                    ["Comments"] = sqlLog
                };

                var defaultFileName = fileName ?? $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                if (SaveSheetsToFile(sheets, defaultFileName))
                {
                    MessageBox.Show("Excel 文件已成功导出。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Re-throw to be caught by the caller if needed
            }
            return false;
        }

        

        public void ExportSingleSheet(DataTable data, string sheetName)
        {
            try
            {
                var sheets = new Dictionary<string, object>
                {
                    [sheetName] = data
                };

                if (SaveSheetsToFile(sheets, $"Export_{sheetName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"))
                {
                    MessageBox.Show("Excel 文件已成功导出。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 计算列宽，考虑中文字符的宽度
        /// </summary>
        /// <param name="text">要计算的文本</param>
        /// <returns>建议的列宽</returns>
        private double CalculateColumnWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 10; // 默认最小宽度

            double width = 0;
            foreach (char c in text)
            {
                // 判断是否为中文字符（CJK统一汉字区域）
                if ((c >= 0x4E00 && c <= 0x9FFF) ||  // CJK统一汉字
                    (c >= 0x3400 && c <= 0x4DBF) ||  // CJK扩展A
                    (c >= 0x20000 && c <= 0x2A6DF) || // CJK扩展B
                    (c >= 0x2A700 && c <= 0x2B73F) || // CJK扩展C
                    (c >= 0x2B740 && c <= 0x2B81F) || // CJK扩展D
                    (c >= 0x3000 && c <= 0x303F) ||  // CJK符号和标点
                    (c >= 0xFF00 && c <= 0xFFEF))    // 全角字符
                {
                    width += 2.2; // 中文字符宽度
                }
                else if (char.IsDigit(c))
                {
                    width += 1.0; // 数字宽度
                }
                else if (char.IsUpper(c))
                {
                    width += 1.3; // 大写字母宽度
                }
                else
                {
                    width += 1.0; // 其他字符宽度
                }
            }

            return width;
        }

        private bool SaveSheetsToFile(IDictionary<string, object> sheets, string defaultFileName)
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
                        else if (sheet.Value is System.Collections.IEnumerable collection && !(sheet.Value is string))
                        {
                            var list = collection.Cast<object>().ToList();
                            if (list.Any())
                            {
                                var itemType = list.First().GetType();
                                var properties = itemType.GetProperties();

                                // Header
                                for (int i = 0; i < properties.Length; i++)
                                {
                                    // 自定义表头名称
                                    if (itemType == typeof(ComparisonResultItem) && properties[i].Name == "IsInJson")
                                    {
                                        worksheet.Cells[1, i + 1].Value = "JSON 状态";
                                    }
                                    else if (itemType == typeof(ComparisonResultItem) && properties[i].Name == "FieldName")
                                    {
                                        worksheet.Cells[1, i + 1].Value = "数据库字段名";
                                    }
                                    else
                                    {
                                        worksheet.Cells[1, i + 1].Value = properties[i].Name;
                                    }
                                }

                                // Data
                                for (int i = 0; i < list.Count; i++)
                                {
                                    for (int j = 0; j < properties.Length; j++)
                                    {
                                        var cell = worksheet.Cells[i + 2, j + 1];
                                        object cellValue = properties[j].GetValue(list[i]);

                                        // 特殊处理 ComparisonResultItem 的 IsInJson 属性
                                        if (itemType == typeof(ComparisonResultItem) && properties[j].Name == "IsInJson")
                                        {
                                            cell.Value = (bool)cellValue ? "✓" : "✗";
                                        }
                                        else
                                        {
                                            cell.Value = cellValue?.ToString();
                                        }
                                        cell.Style.Numberformat.Format = "@";
                                    }

                                    // 条件格式化：如果为 ComparisonResultItem 且 IsInJson 为 false，则整行标红
                                    if (itemType == typeof(ComparisonResultItem))
                                    {
                                        var item = (ComparisonResultItem)list[i];
                                        if (!item.IsInJson)
                                        {
                                            var rowRange = worksheet.Cells[i + 2, 1, i + 2, properties.Length];
                                            rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            rowRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFCDD2"));
                                        }
                                    }
                                }
                            }
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

                        // 优化的列宽计算逻辑
                        for (int i = 1; i <= dataRange.End.Column; i++)
                        {
                            var headerText = worksheet.Cells[1, i].Text;
                            double calculatedWidth = CalculateColumnWidth(headerText);
                            int maxRows = Math.Min(100, dataRange.End.Row);
                            for (int row = 2; row <= maxRows; row++)
                            {
                                var cellText = worksheet.Cells[row, i].Text;
                                if (!string.IsNullOrEmpty(cellText))
                                {
                                    double cellWidth = CalculateColumnWidth(cellText);
                                    calculatedWidth = Math.Max(calculatedWidth, cellWidth);
                                }
                            }
                            worksheet.Column(i).Width = Math.Min(Math.Max(calculatedWidth * 1.2, 10), 50);
                        }

                        worksheet.Row(1).Height = 25;
                        for (int row = 2; row <= dataRange.End.Row; row++)
                        {
                            worksheet.Row(row).Height = 20;
                        }

                        worksheet.Cells[dataRange.Address].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        for (int row = 2; row <= dataRange.End.Row; row++)
                        {
                            for (int col = 1; col <= dataRange.End.Column; col++)
                            {
                                worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                        }
                    }
                    package.Save();
                }
                return true;
            }
            return false;
        }

        public void ExportComparisonResults(IEnumerable<TableComparisonResultViewModel> results)
        {
            try
            {
                var sheets = new Dictionary<string, object>();
                foreach (var result in results)
                {
                    // 注意：这里不再转换为匿名对象，而是直接传递原始集合
                    // SaveSheetsToFile 方法将需要被修改以处理这种特定类型
                    sheets[result.TableName] = result.ComparisonResults;
                }

                if (!sheets.Any())
                {
                    MessageBox.Show("没有可导出的数据。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SaveSheetsToFile(sheets, $"FieldComparison_{DateTime.Now:yyyyMMdd}.xlsx"))
                {
                    MessageBox.Show("Excel 文件已成功导出。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> BatchExportToFolderAsync(
            IEnumerable<BatchExportConfig> configs,
            string targetFolder,
            IProgress<(int current, int total, string currentItem)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var configList = configs.ToList();
            var total = configList.Count;
            bool allSucceeded = true;

            for (int i = 0; i < configList.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var config = configList[i];

                progress?.Report((i, total, config.Key));

                try
                {
                    // Build the filename
                    string tableName = config.DataSource.TableName ?? ExtractTableNameFromSql(config.DataSource.Sql);
                    string fileName = $"{config.Prefix}) {config.Key}-{tableName}(Source).xlsx";
                    string fullPath = Path.Combine(targetFolder, fileName);

                    // Execute queries
                    var task1 = GetDataTableAsync(config.DataSource.Sql, "source");
                    string destinationDbKey = config.Destination == DestinationType.Target ? "target" : "framework";
                    var task2 = GetDataTableAsync(config.DataTarget.Sql, destinationDbKey);

                    await Task.WhenAll(task1, task2);

                    DataTable dt1 = task1.Result;
                    DataTable dt2 = task2.Result;

                    var sqlLog = new List<object>
                    {
                        new { SheetName = config.DataSource.SheetName, SQL_Query = config.DataSource.Sql, Comments = config.DataSource.Description },
                        new { SheetName = config.DataTarget.SheetName, SQL_Query = config.DataTarget.Sql, Comments = config.DataTarget.Description }
                    };

                    var sheets = new Dictionary<string, object>
                    {
                        [config.DataSource.SheetName] = dt1,
                        [config.DataTarget.SheetName] = dt2,
                        ["Comments"] = sqlLog
                    };

                    // Save directly to the specified path
                    await SaveSheetsToPathAsync(sheets, fullPath);
                }
                catch (Exception ex)
                {
                    allSucceeded = false;
                    // Log the error to the debug console, but don't stop the batch.
                    System.Diagnostics.Debug.WriteLine($"Failed to export '{config.Key}': {ex.Message}");
                }
            }

            progress?.Report((total, total, "批量导出完成"));
            return allSucceeded;
        }

        private async Task SaveSheetsToPathAsync(IDictionary<string, object> sheets, string filePath)
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var package = new ExcelPackage(new FileInfo(filePath)))
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
                    else if (sheet.Value is IEnumerable<object> collection)
                    {
                        var list = collection.ToList();
                        if (list.Any())
                        {
                            var itemType = list.First().GetType();
                            var properties = itemType.GetProperties();

                            // Header
                            for (int i = 0; i < properties.Length; i++)
                            {
                                worksheet.Cells[1, i + 1].Value = properties[i].Name;
                            }

                            // Data
                            for (int i = 0; i < list.Count; i++)
                            {
                                for (int j = 0; j < properties.Length; j++)
                                {
                                    var cell = worksheet.Cells[i + 2, j + 1];
                                    cell.Value = properties[j].GetValue(list[i])?.ToString();
                                    cell.Style.Numberformat.Format = "@";
                                }
                            }
                        }
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

                    // 优化的列宽计算逻辑
                    for (int i = 1; i <= dataRange.End.Column; i++)
                    {
                        // 获取表头文字
                        var headerText = worksheet.Cells[1, i].Text;
                        
                        // 计算列宽（考虑中文字符）
                        double calculatedWidth = CalculateColumnWidth(headerText);
                        
                        // 检查数据内容，取最大宽度（可选，限制前100行以提高性能）
                        int maxRows = Math.Min(100, dataRange.End.Row);
                        for (int row = 2; row <= maxRows; row++)
                        {
                            var cellText = worksheet.Cells[row, i].Text;
                            if (!string.IsNullOrEmpty(cellText))
                            {
                                double cellWidth = CalculateColumnWidth(cellText);
                                calculatedWidth = Math.Max(calculatedWidth, cellWidth);
                            }
                        }
                        
                        // 设置列宽，最小10，最大50
                        worksheet.Column(i).Width = Math.Min(Math.Max(calculatedWidth * 1.2, 10), 50);
                    }

                    // 设置固定行高（约3行文字的高度）
                    // 表头行稍微高一点
                    worksheet.Row(1).Height = 25;
                    
                    // 数据行设置固定高度
                    for (int row = 2; row <= dataRange.End.Row; row++)
                    {
                        worksheet.Row(row).Height = 45; // 约3行文字的高度
                    }

                    // 禁用自动换行，改为使用垂直居中
                    worksheet.Cells[dataRange.Address].Style.WrapText = false;
                    worksheet.Cells[dataRange.Address].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    
                    // 设置水平对齐（可选：数字右对齐，文本左对齐）
                    for (int row = 2; row <= dataRange.End.Row; row++)
                    {
                        for (int col = 1; col <= dataRange.End.Column; col++)
                        {
                            var cell = worksheet.Cells[row, col];
                            // 保持文本格式，但设置对齐方式
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }
                }
                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }
    }
}
