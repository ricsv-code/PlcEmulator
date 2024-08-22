using System.Windows.Controls.Primitives;

namespace PlcEmulator
{
    public class PlcMachine
    {

        public PlcMachine()
        {
            MotorGrid = new UniformGrid();
        }

        public bool OverrideKey;
        public bool OperationMode;
        public bool MachineNeedsHoming;
        public bool MachineInCenter;
        public bool MachineStill;
        public bool MachineInMotion;
        public bool MachineInHome;

        //Status byte 5 on code 255
        public bool ProhibitMovement;
        public bool SickReset;
        public bool SickActive;
        public bool EStopReset;
        public bool EStop;

        public UniformGrid MotorGrid;

    }
}


