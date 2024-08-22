namespace PlcEmulator
{
    public class PlcMotor 
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

        public double RotationAngle { get; set; }

        public byte? OperationalSpeed { get; set; }

        public byte? HiBytePos { get; set; }

        public byte? LoBytePos { get; set; }

        public int TargetPosition { get; set; }
        public int AbsolutePosition
        {
            get
            {
                if (HiBytePos.HasValue && LoBytePos.HasValue)
                {
                    return (int)((HiBytePos.Value << 8) | LoBytePos.Value);
                }
                return 0;
            }
        }

        public int HomePosition { get; set; } = 0;
        public int CenterPosition { get; set; } = 3142;
        public int MaxPosition { get; set; } = 6284;
        public int MinPosition { get; set; } = 0;


    }
}


