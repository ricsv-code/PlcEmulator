namespace PlcEmulator
{
    public class PlcMachine
    {

        public PlcMachine(int numberOfMotors)
        {
            Motors = new List<MotorViewModel>();

            for (int _ = 0; _ < numberOfMotors; _++)
            {
                Motors.Add(new MotorViewModel());
            }
        }
        public List<MotorViewModel> Motors { get; set; }

        public bool OverrideKey { get; set; }
        public bool OperationMode { get; set; }
        public bool MachineNeedsHoming { get; set; }
        public bool MachineInCenter { get; set; }
        public bool MachineStill { get; set; }
        public bool MachineInMotion { get; set; }
        public bool MachineInHome { get; set; }

        //Status byte 5 on code 255
        public bool ProhibitMovement { get; set; }
        public bool SickReset { get; set; }
        public bool SickActive { get; set; }
        public bool EStopReset { get; set; }
        public bool EStop { get; set; }

    }
}


