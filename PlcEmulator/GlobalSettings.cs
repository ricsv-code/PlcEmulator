using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using PlcEmulator;

namespace Utilities
{
    public static class GlobalSettings
    {
        private static int numberOfMotors = 4; //default

        public static int NumberOfMotors
        {
            get { return numberOfMotors; }
            set { numberOfMotors = value; }
        }
    }

}