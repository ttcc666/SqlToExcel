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
using System.Threading.Tasks;
using System;

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
            return await db.GetConnection(dbKey.ToLower()).Ado.GetDataTableAsync(sql);
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

        private double CalculateColumnWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 10;

            double width = 0;
            foreach (char c in text)
            {
                if ((c >= 0x4E00 && c <= 0x9FFF) || (c >= 0x3400 && c <= 0x4DBF) || (c >= 0x20000 && c <= 0x2A6DF) || (c >= 0x2A700 && c <= 0x2B73F) || (c >= 0x2B740 && c <= 0x2B81F) || (c >= 0x3000 && c <= 0x303F) || (c >= 0xFF00 && c <= 0xFFEF))
                {
                    width += 2.2;
                }
                else if (char.IsDigit(c))
                {
                    width += 1.0;
                }
                else if (char.IsUpper(c))
                {
                    width += 1.3;
                }
                else
                {
                    width += 1.0;
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
                var excelBytes = CreateExcelPackageBytesAsync(sheets).Result;
                File.WriteAllBytes(saveFileDialog.FileName, excelBytes);
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

        public async Task<bool> ExportValidationResultsToExcelAsync(IEnumerable<ValidationRowResultViewModel> validationResults)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "保存验证结果 Excel 文件",
                    FileName = $"ValidationResults_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Validation Report");
                        int rowIdx = 1;

                        foreach (var rowResult in validationResults)
                        {
                            if (rowIdx > 1) rowIdx++; 

                            worksheet.Cells[rowIdx, 1].Value = rowResult.GroupName;
                            worksheet.Cells[rowIdx, 1].Style.Font.Bold = true;
                            worksheet.Cells[rowIdx, 1].Style.Font.Size = 12;
                            worksheet.Cells[rowIdx, 2].Value = rowResult.MismatchedColumnsSummary;
                            worksheet.Cells[rowIdx, 2].Style.Font.Italic = true;
                            worksheet.Cells[rowIdx, 2].Style.Font.Size = 10;
                            rowIdx++;

                            worksheet.Cells[rowIdx, 1].Value = "字段名";
                            worksheet.Cells[rowIdx, 2].Value = "源数据";
                            worksheet.Cells[rowIdx, 3].Value = "目标数据";
                            var headerRange = worksheet.Cells[rowIdx, 1, rowIdx, 3];
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            rowIdx++;

                            foreach (var mismatch in rowResult.Mismatches)
                            {
                                worksheet.Cells[rowIdx, 1].Value = mismatch.DisplayColumnName;
                                worksheet.Cells[rowIdx, 2].Value = mismatch.SourceValue;
                                worksheet.Cells[rowIdx, 3].Value = mismatch.TargetValue;

                                if (!mismatch.IsMatch)
                                {
                                    var rowRange = worksheet.Cells[rowIdx, 1, rowIdx, 3];
                                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    rowRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFCDD2")); // Light Red
                                }
                                rowIdx++;
                            }
                        }

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                        worksheet.DefaultRowHeight = 20;

                        await File.WriteAllBytesAsync(saveFileDialog.FileName, package.GetAsByteArray());
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出验证结果时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
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
                    string tableName = config.DataSource.TableName ?? ExtractTableNameFromSql(config.DataSource.Sql);
                    string fileName = $"{config.Prefix}) {config.Key}-{tableName}(Source).xlsx";
                    string fullPath = Path.Combine(targetFolder, fileName);

                    var excelBytes = await GenerateSingleExcelExportBytesAsync(config);
                    await File.WriteAllBytesAsync(fullPath, excelBytes, cancellationToken);
                }
                catch (Exception ex)
                {
                    allSucceeded = false;
                    System.Diagnostics.Debug.WriteLine($"Failed to export '{config.Key}': {ex.Message}");
                }
            }

            progress?.Report((total, total, "批量导出完成"));
            return allSucceeded;
        }

        public async Task<byte[]> GenerateSingleExcelExportBytesAsync(BatchExportConfig config)
        {
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

            return await CreateExcelPackageBytesAsync(sheets);
        }

        public async Task<byte[]> CreateExcelPackageBytesAsync(IDictionary<string, object> sheets)
        {
            using (var package = new ExcelPackage())
            {
                foreach (var sheet in sheets)
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheet.Key);

                    if (sheet.Value is DataTable dt)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = dt.Columns[i].ColumnName;
                        }
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

                            for (int i = 0; i < properties.Length; i++)
                            {
                                if (itemType == typeof(ComparisonResultItem) && properties[i].Name == "IsInJson") worksheet.Cells[1, i + 1].Value = "JSON 状态";
                                else if (itemType == typeof(ComparisonResultItem) && properties[i].Name == "FieldName") worksheet.Cells[1, i + 1].Value = "数据库字段名";
                                else worksheet.Cells[1, i + 1].Value = properties[i].Name;
                            }

                            for (int i = 0; i < list.Count; i++)
                            {
                                for (int j = 0; j < properties.Length; j++)
                                {
                                    var cell = worksheet.Cells[i + 2, j + 1];
                                    object cellValue = properties[j].GetValue(list[i]);

                                    if (itemType == typeof(ComparisonResultItem) && properties[j].Name == "IsInJson") cell.Value = (bool)cellValue ? "✓" : "✗";
                                    else cell.Value = cellValue?.ToString();
                                    cell.Style.Numberformat.Format = "@";
                                }

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

                    var header = worksheet.Cells[1, 1, 1, dataRange.End.Column];
                    header.Style.Font.Bold = true;
                    header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    header.Style.Fill.BackgroundColor.SetColor(Color.DodgerBlue);
                    header.Style.Font.Color.SetColor(Color.White);
                    header.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    header.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells[dataRange.Address].AutoFilter = true;

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
                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task ExportSchemaComparisonAsync(IEnumerable<SchemaComparisonResult> results)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "保存主键/索引对比结果",
                    FileName = $"SchemaComparison_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var package = new ExcelPackage())
                    {
                        // Sheet 1: Table Comparison Summary
                        var summarySheet = package.Workbook.Worksheets.Add("表结构对比");
                        string[] summaryHeaders = { "源表名", "源表主键", "目标表名", "目标表主键" };
                        for (int i = 0; i < summaryHeaders.Length; i++)
                        {
                            summarySheet.Cells[1, i + 1].Value = summaryHeaders[i];
                        }
                        int summaryRow = 2;
                        foreach (var result in results)
                        {
                            summarySheet.Cells[summaryRow, 1].Value = result.SourceTableName;
                            summarySheet.Cells[summaryRow, 2].Value = result.SourcePrimaryKeys;
                            summarySheet.Cells[summaryRow, 3].Value = result.TargetTableName;
                            summarySheet.Cells[summaryRow, 4].Value = result.TargetPrimaryKeys;
                            summarySheet.Cells[summaryRow, 2].Style.WrapText = true;
                            summarySheet.Cells[summaryRow, 4].Style.WrapText = true;
                            summaryRow++;
                        }
                        var summaryHeaderRange = summarySheet.Cells[1, 1, 1, summarySheet.Dimension.End.Column];
                        summaryHeaderRange.Style.Font.Bold = true;
                        summaryHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        summaryHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DodgerBlue);
                        summaryHeaderRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        ApplySmartColumnWidth(summarySheet, summarySheet.Dimension);

                        // Sheet 2: Index Details (Side-by-Side)
                        var indexSheet = package.Workbook.Worksheets.Add("索引详细信息");
                        string[] indexHeaders = { "索引名称", "索引字段", "主键", "唯一", "聚集", "非聚集" };
                        int currentRow = 1;
                        const int rightBlockStartCol = 8; // Start target table info in column H, leaving a gap

                        foreach (var result in results)
                        {
                            if (currentRow > 1) { currentRow += 2; } // Add space before the new comparison block
                            int blockStartRow = currentRow;

                            var sourceHeaderCell = indexSheet.Cells[currentRow, 1];
                            sourceHeaderCell.Value = $"源表: {result.SourceTableName}";
                            sourceHeaderCell.Style.Font.Bold = true;
                            indexSheet.Cells[currentRow, 1, currentRow, indexHeaders.Length].Merge = true;

                            var targetHeaderCell = indexSheet.Cells[currentRow, rightBlockStartCol];
                            targetHeaderCell.Value = $"目标表: {result.TargetTableName}";
                            targetHeaderCell.Style.Font.Bold = true;
                            indexSheet.Cells[currentRow, rightBlockStartCol, currentRow, rightBlockStartCol + indexHeaders.Length - 1].Merge = true;
                            currentRow++;

                            for (int i = 0; i < indexHeaders.Length; i++)
                            {
                                indexSheet.Cells[currentRow, 1 + i].Value = indexHeaders[i];
                                indexSheet.Cells[currentRow, rightBlockStartCol + i].Value = indexHeaders[i];
                            }

                            var leftHeaderRange = indexSheet.Cells[currentRow, 1, currentRow, indexHeaders.Length];
                            leftHeaderRange.Style.Font.Bold = true;
                            leftHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            leftHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);

                            var rightHeaderRange = indexSheet.Cells[currentRow, rightBlockStartCol, currentRow, rightBlockStartCol + indexHeaders.Length - 1];
                            rightHeaderRange.Style.Font.Bold = true;
                            rightHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rightHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
                            currentRow++;

                            int sourceIndexCount = result.SourceIndexes?.Count ?? 0;
                            int targetIndexCount = result.TargetIndexes?.Count ?? 0;
                            int maxRows = Math.Max(sourceIndexCount, targetIndexCount);

                            for (int i = 0; i < maxRows; i++)
                            {
                                if (i < sourceIndexCount)
                                {
                                    var index = result.SourceIndexes[i];
                                    indexSheet.Cells[currentRow + i, 1].Value = index.IndexName;
                                    indexSheet.Cells[currentRow + i, 2].Value = index.ColumnsDisplay;
                                    indexSheet.Cells[currentRow + i, 3].Value = index.IsPrimaryKey ? "是" : "否";
                                    indexSheet.Cells[currentRow + i, 4].Value = index.IsUnique ? "是" : "否";
                                    indexSheet.Cells[currentRow + i, 5].Value = index.IsClustered ? "是" : "否";
                                    indexSheet.Cells[currentRow + i, 6].Value = index.IsNonClustered ? "是" : "否";
                                }
                                if (i < targetIndexCount)
                                {
                                    var index = result.TargetIndexes[i];
                                    indexSheet.Cells[currentRow + i, rightBlockStartCol].Value = index.IndexName;
                                    indexSheet.Cells[currentRow + i, rightBlockStartCol + 1].Value = index.ColumnsDisplay;
                                    indexSheet.Cells[currentRow + i, rightBlockStartCol + 2].Value = index.IsPrimaryKey ? "是" : "否";
                                    indexSheet.Cells[currentRow + i, rightBlockStartCol + 3].Value = index.IsUnique ? "是" : "否";
                                    indexSheet.Cells[currentRow + i, rightBlockStartCol + 4].Value = index.IsClustered ? "是" : "否";
                                    indexSheet.Cells[currentRow + i, rightBlockStartCol + 5].Value = index.IsNonClustered ? "是" : "否";
                                }
                            }

                            int blockEndRow = currentRow + maxRows - 1;
                            if (maxRows == 0) { blockEndRow = currentRow - 1; }
                            var blockRange = indexSheet.Cells[blockStartRow, 1, blockEndRow, rightBlockStartCol + indexHeaders.Length - 1];
                            blockRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            blockRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            blockRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            blockRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                            currentRow += maxRows;
                        }
                        
                        // Custom autofit for the complex index sheet
                        if (indexSheet.Dimension != null) 
                        {
                            ApplySmartColumnWidth(indexSheet, indexSheet.Dimension, false);
                        }

                        await File.WriteAllBytesAsync(saveFileDialog.FileName, await package.GetAsByteArrayAsync());
                        MessageBox.Show("Excel 文件已成功导出。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出过程中发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySmartColumnWidth(ExcelWorksheet worksheet, ExcelAddressBase dataRange, bool checkAllRows = true)
        {
            if (dataRange == null) return;

            var columnWidths = new Dictionary<int, double>();

            for (int i = 1; i <= dataRange.End.Column; i++)
            {
                columnWidths[i] = 0;
            }

            // Iterate through all rows to find the max width for each column
            int maxRows = checkAllRows ? dataRange.End.Row : Math.Min(100, dataRange.End.Row);
            for (int row = 1; row <= maxRows; row++)
            {
                for (int col = 1; col <= dataRange.End.Column; col++)
                {
                    var cellText = worksheet.Cells[row, col].Text;
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        double width = CalculateColumnWidth(cellText);
                        if (width > columnWidths[col])
                        {
                            columnWidths[col] = width;
                        }
                    }
                }
            }

            for (int i = 1; i <= dataRange.End.Column; i++)
            {
                worksheet.Column(i).Width = Math.Min(Math.Max(columnWidths[i] * 1.2, 10), 60);
            }
        }
    }
}
