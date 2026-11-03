using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipeExplorer
{
    public static class NamedPipeEnumerator
    {
        public static IReadOnlyList<string> GetNamedPipes()
        {
            const string pipeNamespace = @"\\.\pipe\";

            return Directory
                .EnumerateFileSystemEntries(pipeNamespace)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
