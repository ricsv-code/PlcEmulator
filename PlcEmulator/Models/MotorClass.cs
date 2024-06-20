using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Utilities;
using System.Security.RightsManagement;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace PlcEmulator
{
    public class MotorClass 
    {
        //Machine Status booleans
        public bool Reserved { get; set; }
        public bool Error { get; set; }
        public bool InMinPosition { get; set; }
        public bool InMaxPosition { get; set; }
        public bool InCenteredPosition { get; set; }
        public bool InHomePosition { get; set; }
        public bool MotorIsHomed { get; set; }
        public bool MotorInProgress { get; set; }

        //
        public bool OverrideKey { get; set; }
        public bool OperationMode { get; set; }
        public bool MachineNeedsHoming { get; set; }
        public bool MachineStill { get; set; }
        public bool MachineInMotion { get; set; }
        //

        //Status byte 5 on code 255
        public bool ProhibitMovement { get; set; }
        public bool SickReset { get; set; }
        public bool SickActive { get; set; }
        public bool EStopReset { get; set; }
        public bool EStop { get; set; }
        //


        public byte? OperationalSpeed { get; set; }

        public byte? HiBytePos { get; set; }

        public byte? LoBytePos { get; set; }

    }
}


