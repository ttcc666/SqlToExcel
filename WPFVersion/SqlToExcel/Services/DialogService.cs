using Microsoft.Win32;
using SqlToExcel.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SqlToExcel.Services
{
    /// <summary>
    /// 文件对话框服务实现
    /// </summary>
    public class DialogService : IDialogService
    {
        public Task<string?> ShowSaveFileDialogAsync(string filter, string? defaultFileName = null, string? title = null)
        {
            return Task.FromResult(ShowSaveFileDialog(filter, defaultFileName, title));
        }

        public Task<string?> ShowOpenFileDialogAsync(string filter, string? title = null)
        {
            return Task.FromResult(ShowOpenFileDialog(filter, title));
        }

        public Task<string?> ShowFolderDialogAsync(string? title = null)
        {
            return Task.FromResult(ShowFolderDialog(title));
        }

        private string? ShowSaveFileDialog(string filter, string? defaultFileName = null, string? title = null)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title ?? "保存文件"
            };

            if (!string.IsNullOrEmpty(defaultFileName))
            {
                saveFileDialog.FileName = defaultFileName;
            }

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }

        private string? ShowOpenFileDialog(string filter, string? title = null)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title ?? "选择文件"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        private string? ShowFolderDialog(string? title = null)
        {
            // 使用 SaveFileDialog 来模拟文件夹选择（用于 Zip 文件保存）
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Zip Files|*.zip",
                Title = title ?? "选择保存位置",
                FileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                return Path.GetDirectoryName(saveFileDialog.FileName);
            }

            return null;
        }
    }
}
