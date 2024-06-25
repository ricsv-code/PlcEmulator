using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PlcTester
{
    public class MotorValues
    {
        public int Position { get; set; }
        public int Speed { get; set; }

        public bool Error { get; set; }
        public bool InMinPosition { get; set; }
        public bool InMaxPosition { get; set; }
        public bool InCenteredPosition { get; set; }
        public bool InHomePosition { get; set; }
        public bool MotorIsHomed { get; set; }
        public bool MotorInProgress { get; set; }
        public bool Reserved { get; set; }
        public bool MachineNeedsHoming { get; set; }



        public bool OperationMode { get; set; }
        public bool OverrideKey { get; set; }

        //255
        public bool ProhibitMovement { get; set; }
        public bool SickReset { get; set; }
        public bool SickActive { get; set; }
        public bool EStopReset { get; set; }
        public bool EStop { get; set; }
        //

        public bool EButtonPressed { get; set; }

    }
}