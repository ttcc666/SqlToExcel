using SqlToExcel.Services.Interfaces;
using System.Threading.Tasks;
using System.Windows;

namespace SqlToExcel.Services
{
    /// <summary>
    /// 消息服务实现
    /// </summary>
    public class MessageService : IMessageService
    {
        public Task<MessageResult> ShowMessageAsync(string message, string title = "提示", MessageButton button = MessageButton.OK, MessageIcon icon = MessageIcon.Information)
        {
            var wpfButton = ConvertToWpfButton(button);
            var wpfIcon = ConvertToWpfIcon(icon);
            
            var result = MessageBox.Show(message, title, wpfButton, wpfIcon);
            return Task.FromResult(ConvertFromWpfResult(result));
        }

        public Task ShowInformationAsync(string message, string title = "信息")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string message, string title = "警告")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string message, string title = "错误")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return Task.CompletedTask;
        }

        public Task<bool> ShowConfirmationAsync(string message, string title = "确认")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        private MessageBoxButton ConvertToWpfButton(MessageButton button)
        {
            return button switch
            {
                MessageButton.OK => MessageBoxButton.OK,
                MessageButton.OKCancel => MessageBoxButton.OKCancel,
                MessageButton.YesNo => MessageBoxButton.YesNo,
                MessageButton.YesNoCancel => MessageBoxButton.YesNoCancel,
                _ => MessageBoxButton.OK
            };
        }

        private MessageBoxImage ConvertToWpfIcon(MessageIcon icon)
        {
            return icon switch
            {
                MessageIcon.None => MessageBoxImage.None,
                MessageIcon.Information => MessageBoxImage.Information,
                MessageIcon.Warning => MessageBoxImage.Warning,
                MessageIcon.Error => MessageBoxImage.Error,
                MessageIcon.Question => MessageBoxImage.Question,
                _ => MessageBoxImage.Information
            };
        }

        private MessageResult ConvertFromWpfResult(MessageBoxResult result)
        {
            return result switch
            {
                MessageBoxResult.OK => MessageResult.OK,
                MessageBoxResult.Cancel => MessageResult.Cancel,
                MessageBoxResult.Yes => MessageResult.Yes,
                MessageBoxResult.No => MessageResult.No,
                _ => MessageResult.None
            };
        }
    }
}
