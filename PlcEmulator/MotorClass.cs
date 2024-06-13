using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Utilities;

namespace PlcEmulator
{
    public class MotorClass
    {
        //Machine Status booleans
        public bool Reserved { get; set; }
        public bool Error { get; set; }
        public bool InMinPosition { get; set; }
        public bool InMaxPosition { get; set; }
        public bool InCentredPosition { get; set; }
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

        public void SetOperationalSpeed(byte value)
        {
            OperationalSpeed = value;
        }

        public byte GetOperationalSpeed()
        {
            return OperationalSpeed ?? 0;
        }
        public void SetHiBytePos(byte hiBytePos)
        {

            HiBytePos = hiBytePos;
        }

        public void SetLoBytePos(byte loBytePos)
        {

            LoBytePos = loBytePos;
        }

        public byte GetHiBytePos()
        {
            return HiBytePos ?? 0;
        }

        public byte GetLoBytePos()
        {
            return LoBytePos ?? 0;
        }

    }

    public class MotorService
    {
        private static readonly MotorService[] _instances = new MotorService[GlobalSettings.NumberOfMotors];
        public MotorClass Motor { get; private set; }

        private MotorService()
        {
            Motor = new MotorClass();
        }

        public static MotorService[] Instances
        {
            get
            {
                for (int i = 0; i < (GlobalSettings.NumberOfMotors); i++)
                {
                    if (_instances[i] == null)
                    {
                        _instances[i] = new MotorService();
                    }
                }
                return _instances;
            }
        }
    }


    public class MotorViewModel : INotifyPropertyChanged
    {
        public MotorViewModel()
        {
            for (int i = 0; i < (GlobalSettings.NumberOfMotors); i++)
            {
                Motor = MotorService.Instances[i].Motor;
            }
        }
        public MotorClass Motor
        { get; }
        public bool Reserved
        {
            get => Motor.Reserved;
            set
            {
                if (Motor.Reserved != value)
                {
                    Motor.Reserved = value;
                    OnPropertyChanged(nameof(Reserved));
                }
            }
        }
        public bool Error
        {
            get => Motor.Error;
            set
            {
                if (Motor.Error != value)
                {
                    Motor.Error = value;
                    OnPropertyChanged(nameof(Error));
                }
            }
        }
        public bool InMinPosition
        {
            get => Motor.InMinPosition;
            set
            {
                if (Motor.InMinPosition != value)
                {
                    Motor.InMinPosition = value;
                    OnPropertyChanged(nameof(InMinPosition));
                }
            }
        }
        public bool InMaxPosition
        {
            get => Motor.InMaxPosition;
            set
            {
                if (Motor.InMaxPosition != value)
                {
                    Motor.InMaxPosition = value;
                    OnPropertyChanged(nameof(InMaxPosition));
                }
            }
        }
        public bool InCentredPosition
        {
            get => Motor.InCentredPosition;
            set
            {
                if (Motor.InCentredPosition != value)
                {
                    Motor.InCentredPosition = value;
                    OnPropertyChanged(nameof(InCentredPosition));
                }
            }
        }
        public bool InHomePosition
        {
            get => Motor.InHomePosition;
            set
            {
                if (Motor.InHomePosition != value)
                {
                    Motor.InHomePosition = value;
                    OnPropertyChanged(nameof(InHomePosition));
                }
            }
        }
        public bool MotorIsHomed
        {
            get => Motor.MotorIsHomed;
            set
            {
                if (Motor.MotorIsHomed != value)
                {
                    Motor.MotorIsHomed = value;
                    OnPropertyChanged(nameof(MotorIsHomed));
                }
            }
        }
        public bool MotorInProgress
        {
            get => Motor.MotorInProgress;
            set
            {
                if (Motor.MotorInProgress != value)
                {
                    Motor.MotorInProgress = value;
                    OnPropertyChanged(nameof(MotorInProgress));
                }
            }
        }

        public bool OverrideKey
        {
            get => Motor.OverrideKey;
            set
            {
                if (Motor.OverrideKey != value)
                {
                    Motor.OverrideKey = value;
                    OnPropertyChanged(nameof(OverrideKey));
                }
            }
        }

        public bool OperationMode
        {
            get => Motor.OperationMode;
            set
            {
                if (Motor.OperationMode != value)
                {
                    Motor.OperationMode = value;
                    OnPropertyChanged(nameof(OperationMode));
                }
            }
        }

        public bool MachineNeedsHoming
        {
            get => Motor.MachineNeedsHoming;
            set
            {
                if (Motor.MachineNeedsHoming != value)
                {
                    Motor.MachineNeedsHoming = value;
                    OnPropertyChanged(nameof(MachineNeedsHoming));
                }
            }
        }

        public bool MachineStill
        {
            get => Motor.MachineStill;
            set
            {
                if (Motor.MachineStill != value)
                {
                    Motor.MachineStill = value;
                    OnPropertyChanged(nameof(MachineStill));
                }
            }
        }

        public bool MachineInMotion
        {
            get => Motor.MachineInMotion;
            set
            {
                if (Motor.MachineInMotion != value)
                {
                    Motor.MachineInMotion = value;
                    OnPropertyChanged(nameof(MachineInMotion));
                }
            }
        }

        public bool ProhibitMovement
        {
            get => Motor.ProhibitMovement;
            set
            {
                if (Motor.ProhibitMovement != value)
                {
                    Motor.ProhibitMovement = value;
                    OnPropertyChanged(nameof(ProhibitMovement));
                }
            }
        }

        public bool SickReset
        {
            get => Motor.SickReset;
            set
            {
                if (Motor.SickReset != value)
                {
                    Motor.SickReset = value;
                    OnPropertyChanged(nameof(SickReset));
                }
            }
        }

        public bool SickActive
        {
            get => Motor.SickActive;
            set
            {
                if (Motor.SickActive != value)
                {
                    Motor.SickActive = value;
                    OnPropertyChanged(nameof(SickActive));
                }
            }
        }

        public bool EStopReset
        {
            get => Motor.EStopReset;
            set
            {
                if (Motor.EStopReset != value)
                {
                    Motor.EStopReset = value;
                    OnPropertyChanged(nameof(EStopReset));
                }
            }
        }

        public bool EStop
        {
            get => Motor.EStop;
            set
            {
                if (Motor.EStop != value)
                {
                    Motor.EStop = value;
                    OnPropertyChanged(nameof(EStop));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}




