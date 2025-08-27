using System.Threading.Tasks;

namespace SqlToExcel.Services.Interfaces
{
    /// <summary>
    /// 文件对话框服务接口，用于处理文件选择和保存操作
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 显示保存文件对话框
        /// </summary>
        /// <param name="filter">文件过滤器，例如 "Excel Files|*.xlsx"</param>
        /// <param name="defaultFileName">默认文件名</param>
        /// <param name="title">对话框标题</param>
        /// <returns>如果用户选择了文件则返回文件路径，否则返回null</returns>
        Task<string?> ShowSaveFileDialogAsync(string filter, string? defaultFileName = null, string? title = null);

        /// <summary>
        /// 显示打开文件对话框
        /// </summary>
        /// <param name="filter">文件过滤器，例如 "JSON files (*.json)|*.json|All files (*.*)|*.*"</param>
        /// <param name="title">对话框标题</param>
        /// <returns>如果用户选择了文件则返回文件路径，否则返回null</returns>
        Task<string?> ShowOpenFileDialogAsync(string filter, string? title = null);

        /// <summary>
        /// 显示文件夹选择对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <returns>如果用户选择了文件夹则返回文件夹路径，否则返回null</returns>
        Task<string?> ShowFolderDialogAsync(string? title = null);
    }
}
