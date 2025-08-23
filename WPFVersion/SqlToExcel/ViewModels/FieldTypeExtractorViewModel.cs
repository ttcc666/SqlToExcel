using SqlToExcel.Models;
using SqlToExcel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class FieldTypeExtractorViewModel : INotifyPropertyChanged
    {
        private string _jsonInput = @"{
    ""table"": ""t_License"",
    ""fields"": [
      ""id"",
      ""license_no"",
      ""status""
    ]
}";
        private ObservableCollection<FieldTypeInfo> _fieldTypes;
        private string _statusMessage = "准备就绪";
        private bool _isProcessing = false;

        public string JsonInput
        {
            get => _jsonInput;
            set { _jsonInput = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FieldTypeInfo> FieldTypes
        {
            get => _fieldTypes;
            set { _fieldTypes = value; OnPropertyChanged(); }
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

        public ICommand ExtractFieldTypesCommand { get; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand ClearCommand { get; }

        public FieldTypeExtractorViewModel()
        {
            FieldTypes = new ObservableCollection<FieldTypeInfo>();
            ExtractFieldTypesCommand = new RelayCommand(async p => await ExtractFieldTypesAsync(), p => !IsProcessing && !string.IsNullOrWhiteSpace(JsonInput));
            CopyToClipboardCommand = new RelayCommand(p => CopyToClipboard(), p => FieldTypes?.Count > 0);
            ClearCommand = new RelayCommand(p => Clear());
        }

        private async Task ExtractFieldTypesAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "正在解析JSON...";
                FieldTypes.Clear();

                // 自动替换全角字符为半角字符
                var normalizedJson = JsonInput
                    .Replace('｛', '{')
                    .Replace('｝', '}')
                    .Replace('［', '[')
                    .Replace('］', ']')
                    .Replace('：', ':')
                    .Replace('，', ',')
                    .Replace('"', '"')
                    .Replace('"', '"');

                // 解析JSON
                FieldTypeRequest request;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    request = JsonSerializer.Deserialize<FieldTypeRequest>(normalizedJson, options);
                    
                    if (request == null || string.IsNullOrWhiteSpace(request.table))
                    {
                        throw new Exception("JSON格式无效或缺少表名");
                    }

                    if (request.fields == null || request.fields.Count == 0)
                    {
                        throw new Exception("字段列表不能为空");
                    }
                }
                catch (JsonException ex)
                {
                    StatusMessage = $"JSON解析错误: {ex.Message}";
                    
                    // 提供更友好的错误提示
                    var errorMessage = $"JSON格式错误:\n{ex.Message}";
                    
                    // 检查是否还有全角字符
                    if (JsonInput.Contains('｛') || JsonInput.Contains('｝'))
                    {
                        errorMessage += "\n\n提示：检测到全角大括号 ｛｝，请使用半角字符 {} ";
                    }
                    
                    if (ex.LineNumber.HasValue)
                    {
                        errorMessage += $"\n\n错误位置：第 {ex.LineNumber} 行";
                    }
                    
                    MessageBox.Show(errorMessage, "JSON解析错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StatusMessage = $"正在验证表 '{request.table}'...";

                // 检查数据库是否已配置
                if (!DatabaseService.Instance.IsConfigured())
                {
                    StatusMessage = "数据库未配置";
                    MessageBox.Show("请先配置数据库连接", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 验证表是否存在于target数据库
                await Task.Run(() =>
                {
                    if (!DatabaseService.Instance.IsTableExistsInTarget(request.table))
                    {
                        throw new Exception($"表 '{request.table}' 不存在于目标数据库中");
                    }
                });

                StatusMessage = $"正在获取字段类型信息...";

                // 获取字段类型信息
                Dictionary<string, string> fieldTypesDict = null;
                await Task.Run(() =>
                {
                    fieldTypesDict = DatabaseService.Instance.GetFieldTypesInfo("target", request.table, request.fields);
                });

                // 填充结果
                if (fieldTypesDict != null)
                {
                    foreach (var field in request.fields)
                    {
                        var fieldType = fieldTypesDict.ContainsKey(field) ? fieldTypesDict[field] : "字段不存在";
                        FieldTypes.Add(new FieldTypeInfo
                        {
                            FieldName = field,
                            FieldType = fieldType
                        });
                    }

                    StatusMessage = $"成功获取 {FieldTypes.Count} 个字段的类型信息";
                }
                else
                {
                    StatusMessage = "获取字段类型信息失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"错误: {ex.Message}";
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void CopyToClipboard()
        {
            try
            {
                if (FieldTypes == null || FieldTypes.Count == 0)
                {
                    StatusMessage = "没有数据可复制";
                    return;
                }

                var sb = new StringBuilder();
                
                // 只添加字段类型列，不包含表头
                foreach (var field in FieldTypes)
                {
                    sb.AppendLine(field.FieldType);
                }

                // 移除最后一个换行符
                if (sb.Length > 0 && sb[sb.Length - 1] == '\n')
                {
                    sb.Length -= Environment.NewLine.Length;
                }

                // 复制到剪贴板
                Clipboard.SetText(sb.ToString());
                StatusMessage = $"已复制 {FieldTypes.Count} 个字段类型到剪贴板（可直接粘贴到Excel）";
            }
            catch (Exception ex)
            {
                StatusMessage = $"复制失败: {ex.Message}";
                MessageBox.Show($"复制到剪贴板失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear()
        {
            JsonInput = "";
            FieldTypes.Clear();
            StatusMessage = "已清空";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}