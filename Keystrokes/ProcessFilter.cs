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

        public void Load()
        {
            IsEnable = Properties.Settings.Default.FilterEnable;

            var processItems = Properties.Settings.Default.FilterContent.Split(',');
            foreach (var processItem in processItems)
            {
                if (string.IsNullOrEmpty(processItem) == false)
                {
                    ProcessItems.Add(processItem);
                }
            }
        }

        public void Save()
        {
            Properties.Settings.Default.FilterEnable = IsEnable;
            Properties.Settings.Default.FilterContent = string.Join(",", ProcessItems.ToArray());

            Properties.Settings.Default.Save();
        }
    }
}
