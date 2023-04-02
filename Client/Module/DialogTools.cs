using ModernWpf.Controls;
using System.Windows.Threading;

namespace Client.Module
{
    public static class DialogTools
    {
        private static object queueObj = new object();

        /// <summary>
        /// 可确保在同一时刻只显示一个弹窗。
        /// </summary>
        /// <param name="dialog">ContentDialog 对象。</param>
        public static void ShowDialog(ContentDialog dialog)
        {
            lock (queueObj)
            {
                Values.UIDispatcher.Invoke(() =>
                {
                    dialog.ShowAsync();
                });
            }
        }

        /// <summary>
        /// 可确保在同一时刻只显示一个弹窗。
        /// </summary>
        /// <param name="title">弹窗标题。</param>
        /// <param name="content">弹窗内容。</param>
        public static void ShowDialogWithCloseButton(string title, object content)
        {
            Values.UIDispatcher.Invoke(() =>
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = "确定",
                    DefaultButton = ContentDialogButton.Close
                };
                ShowDialog(dialog);
            });
        }
    }
}
