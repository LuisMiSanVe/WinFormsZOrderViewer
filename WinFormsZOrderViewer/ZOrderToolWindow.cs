using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace WinFormsZOrderViewer
{
    [Guid("d1234567-89ab-cdef-0123-456789abcdef")]
    public class ZOrderToolWindow : ToolWindowPane
    {
        public ZOrderToolWindow() : base(null)
        {
            this.Caption = "Z-Order Viewer";
            this.Content = new ZOrderToolWindowControl();
        }

        public static async Task ShowWindowAsync(AsyncPackage package)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            ToolWindowPane window = await package.FindToolWindowAsync(typeof(ZOrderToolWindow), 0, true, CancellationToken.None).ConfigureAwait(true);
            if (window?.Frame != null)
            {
                var frame = (Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.Show());
            }
        }
    }
}
