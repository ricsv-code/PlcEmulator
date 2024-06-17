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
    public class MotorViewModel : INotifyPropertyChanged //binder ui properties till MotorClass instanser
    {
        private MotorClass Motor;

        public int MotorIndex { get; }
        public MotorViewModel(MotorClass motor, int index)
        {
            Motor = motor;
            MotorIndex = (index + 1);
        }
        public MotorClass motor
        {
            get { return Motor; }
        }
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
        public byte OperationalSpeed
        {
            get => Motor.OperationalSpeed ?? 0;
            set
            {
                if (Motor.OperationalSpeed != value)
                {
                    Motor.OperationalSpeed = value;
                    OnPropertyChanged(nameof(OperationalSpeed));
                }
            }
        }

        public byte HiBytePos
        {
            get => Motor.HiBytePos ?? 0;
            set
            {
                if (Motor.HiBytePos != value)
                {
                    Motor.HiBytePos = value;
                    OnPropertyChanged(nameof(HiBytePos));
                }
            }
        }

        public byte LoBytePos
        {
            get => Motor.LoBytePos ?? 0;
            set
            {
                if (Motor.LoBytePos != value)
                {
                    Motor.LoBytePos = value;
                    OnPropertyChanged(nameof(LoBytePos));
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


