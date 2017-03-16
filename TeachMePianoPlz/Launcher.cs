using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Midi;

namespace TeachMePianoPlz
{
    class Launcher
    {
        [STAThread]
        static void Main(string[] args)
        {
            using(Teacher t = new Teacher())
                t.Run();
        }
    }
}
