using System.Threading.Tasks;

namespace SqlToExcel.Services.Interfaces
{
    /// <summary>
    /// 消息框结果枚举
    /// </summary>
    public enum MessageResult
    {
        None,
        OK,
        Cancel,
        Yes,
        No
    }

    /// <summary>
    /// 消息框按钮类型枚举
    /// </summary>
    public enum MessageButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    /// <summary>
    /// 消息框图标类型枚举
    /// </summary>
    public enum MessageIcon
    {
        None,
        Information,
        Warning,
        Error,
        Question
    }

    /// <summary>
    /// 消息服务接口，用于处理用户消息显示
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <param name="button">按钮类型</param>
        /// <param name="icon">图标类型</param>
        /// <returns>用户选择的结果</returns>
        Task<MessageResult> ShowMessageAsync(string message, string title = "提示", MessageButton button = MessageButton.OK, MessageIcon icon = MessageIcon.Information);

        /// <summary>
        /// 显示信息消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <returns>Task</returns>
        Task ShowInformationAsync(string message, string title = "信息");

        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <returns>Task</returns>
        Task ShowWarningAsync(string message, string title = "警告");

        /// <summary>
        /// 显示错误消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <returns>Task</returns>
        Task ShowErrorAsync(string message, string title = "错误");

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <returns>如果用户选择是则返回true，否则返回false</returns>
        Task<bool> ShowConfirmationAsync(string message, string title = "确认");
    }
}
