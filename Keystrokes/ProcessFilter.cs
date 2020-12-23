using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes
{
    public class ProcessFilter
    {
        public bool IsEnable { get; set; }
        public List<string> ProcessItems { get; set; } = new List<string>();
    }
}
