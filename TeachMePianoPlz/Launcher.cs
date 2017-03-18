using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Midi;
using System.Windows.Forms;

namespace TeachMePianoPlz
{
    class Launcher
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (InputDevice.InstalledDevices.Count == 0)
            {
                MessageBox.Show("No MIDI device was found." + Environment.NewLine + Environment.NewLine + "This won't do.", "Hm...");
            }
            else
            {
                using (Teacher t = new Teacher())
                    t.Run();
            }
        }
    }
}
