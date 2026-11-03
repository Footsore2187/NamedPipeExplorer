using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NamedPipeExplorer
{
    public static class ProcessDetailsWindow
    {
        public static void Show(
            IWin32Window owner,
            PipeProcessInfo processInfo)
        {
            if (processInfo == null)
                throw new ArgumentNullException(nameof(processInfo));

            using (Form window = new Form())
            {
                window.Text =
                    "Named Pipe Process - PID " + processInfo.ProcessId;
                window.StartPosition =
                    FormStartPosition.CenterParent;
                window.ClientSize = new Size(700, 400);
                window.MinimumSize = new Size(450, 300);

                TextBox details = new TextBox();
                details.Dock = DockStyle.Fill;
                details.Multiline = true;
                details.ReadOnly = true;
                details.ScrollBars = ScrollBars.Both;
                details.WordWrap = false;
                details.Font = new Font(
                    FontFamily.GenericMonospace,
                    10.0f);
                details.Text = CreateDetailsText(processInfo);

                Button closeButton = new Button();
                closeButton.Text = "Close";
                closeButton.AutoSize = true;
                closeButton.DialogResult = DialogResult.OK;

                FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 45;
                buttonPanel.FlowDirection = FlowDirection.RightToLeft;
                buttonPanel.Padding = new Padding(5);
                buttonPanel.Controls.Add(closeButton);

                window.Controls.Add(details);
                window.Controls.Add(buttonPanel);
                window.AcceptButton = closeButton;
                window.CancelButton = closeButton;

                window.ShowDialog(owner);
            }
        }

        private static string CreateDetailsText(
            PipeProcessInfo processInfo)
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine("Pipe name:");
            text.AppendLine(processInfo.PipeName);
            text.AppendLine();

            text.AppendLine("Server process ID:");
            text.AppendLine(processInfo.ProcessId.ToString());
            text.AppendLine();

            text.AppendLine("Process name:");
            text.AppendLine(processInfo.ProcessName);
            text.AppendLine();

            text.AppendLine("Executable path:");
            text.AppendLine(processInfo.ExecutablePath);
            text.AppendLine();

            text.AppendLine("Process start time:");
            text.AppendLine(processInfo.StartTime);
            text.AppendLine();

            text.AppendLine("Windows session ID:");

            return text.ToString();
        }
    }

}
