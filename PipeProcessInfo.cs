using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipeExplorer
{
    public sealed class PipeProcessInfo
    {
        public string PipeName { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string ExecutablePath { get; set; }
        public string StartTime { get; set; }
        public string SessionId { get; set; }
    }
}
