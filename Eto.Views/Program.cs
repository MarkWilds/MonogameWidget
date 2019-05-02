using System;
using Eto;
using Eto.Forms;

namespace gtkView
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Platforms.Gtk)
                .Run(new MainWindow());
        }
    }
}
