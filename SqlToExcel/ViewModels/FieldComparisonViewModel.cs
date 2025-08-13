using SqlToExcel.Models;
using SqlToExcel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class FieldComparisonViewModel : INotifyPropertyChanged
    {
        private string _jsonInput = @"[
    {
        ""table"": ""YourTableName1"",
        ""fields"": [""field1"", ""field2""]
    },
    {
        ""table"": ""YourTableName2"",
        ""fields"": [""fieldA"", ""fieldB""]
    }
]";
        private string _statusMessage = "准备就绪. 请输入JSON数组并点击开始对比.";
        private bool _isProcessing = false;

        public ObservableCollection<TableComparisonResultViewModel> TabResults { get; set; }

        public string JsonInput
        {
            get => _jsonInput;
            set { _jsonInput = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set 
            { 
                _isProcessing = value; 
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand CompareFieldsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ExportCommand { get; }

        public FieldComparisonViewModel()
        {
            TabResults = new ObservableCollection<TableComparisonResultViewModel>();
            CompareFieldsCommand = new RelayCommand(async p => await CompareFieldsAsync(), p => !IsProcessing && !string.IsNullOrWhiteSpace(JsonInput));
            ClearCommand = new RelayCommand(p => Clear());
            ExportCommand = new RelayCommand(p => Export(), p => TabResults.Any());
        }

        private async Task CompareFieldsAsync()
        {
            IsProcessing = true;
            StatusMessage = "正在处理...";
            TabResults.Clear();

            try
            {
                var normalizedJson = JsonInput.Trim();
                if (!normalizedJson.StartsWith("[") || !normalizedJson.EndsWith("]"))
                {
                    throw new Exception("JSON格式无效，必须是一个数组 (以 [ 开头, 以 ] 结尾)。");
                }

                List<FieldTypeRequest> requests;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    requests = JsonSerializer.Deserialize<List<FieldTypeRequest>>(normalizedJson, options);
                    if (requests == null || requests.Count == 0)
                    {
                        throw new Exception("JSON数组为空或无法解析。");
                    }
                }
                catch (JsonException ex)
                {
                    throw new Exception($"JSON解析错误: {ex.Message}");
                }

                if (!DatabaseService.Instance.IsConfigured())
                {
                    throw new Exception("数据库未配置，请先在主窗口配置。");
                }

                int successCount = 0;
                bool hasDifference = false;

                for (int i = 0; i < requests.Count; i++)
                {
                    var request = requests[i];
                    StatusMessage = $"正在处理第 {i + 1}/{requests.Count} 个表: {request.table}";
                    var tabResult = new TableComparisonResultViewModel { TableName = request.table };

                    try
                    {
                        if (string.IsNullOrWhiteSpace(request.table) || request.fields == null)
                        {
                            throw new Exception("请求无效，缺少 'table' 或 'fields' 属性。");
                        }

                        var dbColumns = await Task.Run(() =>
                        {
                            var columnInfos = DatabaseService.Instance.GetColumns("source", request.table);
                            if (columnInfos.Count == 0 && !DatabaseService.Instance.IsTableExists("source", request.table))
                            {
                                throw new Exception($"表 '{request.table}' 不存在于源数据库中");
                            }
                            return columnInfos.Select(c => c.DbColumnName.Trim()).ToList();
                        });

                        var trimmedJsonFields = request.fields.Select(f => f.Trim()).ToList();
                        var jsonFields = new HashSet<string>(trimmedJsonFields, StringComparer.OrdinalIgnoreCase);
                        var dbFields = new HashSet<string>(dbColumns, StringComparer.OrdinalIgnoreCase);

                        // Populate the results for the UI
                        foreach (var dbField in dbColumns.OrderBy(f => f))
                        {
                            tabResult.ComparisonResults.Add(new ComparisonResultItem
                            {
                                FieldName = dbField,
                                IsInJson = jsonFields.Contains(dbField)
                            });
                        }

                        // Now, calculate statistics from the UI's data source for consistency
                        var missingCount = tabResult.ComparisonResults.Count(r => !r.IsInJson);
                        tabResult.StatusMessage = $"对比完成。共 {dbColumns.Count} 个字段，{missingCount} 个在JSON中缺失。";
                        successCount++;

                        // If there are any differences, save the report
                        bool setsAreEqual = dbFields.SetEquals(jsonFields);
                        if (!setsAreEqual)
                        {
                            hasDifference = true;
                            var report = new ComparisonReport
                            {
                                TableName = request.table,
                                JsonFields = jsonFields.ToArray(),
                                DbFields = dbFields.ToArray(),
                                ComparisonDate = DateTime.Now
                            };
                            await DatabaseService.Instance.SaveComparisonReportAsync(report);
                        }
                    }
                    catch (Exception ex)
                    {
                        tabResult.StatusMessage = $"处理失败: {ex.Message}";
                    }
                    TabResults.Add(tabResult);
                }

                // 如果有任何差异被保存，则发布事件
                if (hasDifference)
                {
                    EventService.Publish(new ComparisonReportUpdatedEvent());
                }

                StatusMessage = $"批量处理完成。成功: {successCount} / {requests.Count}。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"错误: {ex.Message}";
                MessageBox.Show(ex.Message, "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void Clear()
        {
            JsonInput = "";
            TabResults.Clear();
            StatusMessage = "已清空";
        }

        private void Export()
        {
            try
            {
                var excelService = new ExcelExportService();
                excelService.ExportComparisonResults(TabResults);
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出失败: {ex.Message}";
                MessageBox.Show(ex.Message, "导出错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}