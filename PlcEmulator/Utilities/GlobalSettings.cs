using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using PlcEmulator;

namespace Utilities
{
    public static class GlobalSettings
    {
        private static int _numberOfMotors = 4; //default

        public static event EventHandler NumberOfMotorsChanged;

        public static int NumberOfMotors
        {
            get { return _numberOfMotors; }
            set
            {
                _numberOfMotors = value;

                NumberOfMotorsChanged?.Invoke(null, EventArgs.Empty);
            }
        }

    }
}