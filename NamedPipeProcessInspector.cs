using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipeExplorer
{
    public static class NamedPipeProcessInspector
    {
        private const uint OpenExisting = 3;

        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeServerProcessId(
            SafeFileHandle pipe,
            out uint serverProcessId);

        public static PipeProcessInfo GetServerProcessInfo(string pipeName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
                throw new ArgumentException(
                    "A pipe name must be provided.",
                    nameof(pipeName));

            if (!pipeName.StartsWith(
                @"\\.\pipe\",
                StringComparison.OrdinalIgnoreCase))
            {
                pipeName = @"\\.\pipe\" + pipeName;
            }

            /*
             * desiredAccess is zero because the handle is only being opened
             * to query pipe information. No pipe data will be read or written.
             */
            using (SafeFileHandle pipeHandle = CreateFile(
                pipeName,
                0,
                0,
                IntPtr.Zero,
                OpenExisting,
                0,
                IntPtr.Zero))
            {
                if (pipeHandle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();

                    throw new Win32Exception(
                        error,
                        "Unable to open the named pipe \"" +
                        pipeName +
                        "\".");
                }

                uint processId;

                if (!GetNamedPipeServerProcessId(
                    pipeHandle,
                    out processId))
                {
                    int error = Marshal.GetLastWin32Error();

                    throw new Win32Exception(
                        error,
                        "Unable to determine the named pipe server process.");
                }

                return ReadProcessInformation(
                    pipeName,
                    checked((int)processId));
            }
        }

        private static PipeProcessInfo ReadProcessInformation(
            string pipeName,
            int processId)
        {
            PipeProcessInfo result = new PipeProcessInfo
            {
                PipeName = pipeName,
                ProcessId = processId,
                ProcessName = "Unavailable",
                ExecutablePath = "Unavailable",
                StartTime = "Unavailable",
                SessionId = "Unavailable"
            };

            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    result.ProcessName = TryGetProcessName(process);
                    result.ExecutablePath = TryGetExecutablePath(process);
                    result.StartTime = TryGetStartTime(process);
                    result.SessionId = TryGetSessionId(process);
                }
            }
            catch (ArgumentException)
            {
                result.ProcessName =
                    "Process exited before its information could be read.";
            }

            return result;
        }

        private static string TryGetProcessName(Process process)
        {
            try
            {
                return process.ProcessName;
            }
            catch (Exception ex)
            {
                return FormatUnavailable(ex);
            }
        }

        private static string TryGetExecutablePath(Process process)
        {
            try
            {
                if (process.MainModule == null)
                    return "Unavailable";

                return process.MainModule.FileName;
            }
            catch (Exception ex)
            {
                return FormatUnavailable(ex);
            }
        }

        private static string TryGetStartTime(Process process)
        {
            try
            {
                return process.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                return FormatUnavailable(ex);
            }
        }

        private static string TryGetSessionId(Process process)
        {
            try
            {
                return process.SessionId.ToString();
            }
            catch (Exception ex)
            {
                return FormatUnavailable(ex);
            }
        }

        private static string FormatUnavailable(Exception ex)
        {
            return "Unavailable (" + ex.Message + ")";
        }
    }
}