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
using System.Drawing;

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
            Motor.PropertyChanged += Motor_PropertyChanged;
        }

        private void Motor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MotorClass.OperationalSpeed))
            {
                OnPropertyChanged(nameof(OperationalSpeed));
            }
            if (e.PropertyName == nameof(MotorClass.HiBytePos) || e.PropertyName == nameof(MotorClass.LoBytePos))
            {
                OnPropertyChanged(nameof(HiBytePos));
                OnPropertyChanged(nameof(LoBytePos));
                OnPropertyChanged(nameof(AbsolutePosition));
            }
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
        public bool InCenteredPosition
        {
            get => Motor.InCenteredPosition;
            set
            {
                if (Motor.InCenteredPosition != value)
                {
                    Motor.InCenteredPosition = value;
                    OnPropertyChanged(nameof(InCenteredPosition));
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

        public void UpdateIndicators()
        {
            if ((Motor.HiBytePos ?? 0) == 0 && (Motor.LoBytePos ?? 0) == 0)
            {
                InHomePosition = true;
            }
            else
            {
                InHomePosition = false;
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
                    OnPropertyChanged("BoolFill");
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
        public byte OperationalSpeed => Motor.OperationalSpeed ?? 0;

        public byte HiBytePos => Motor.HiBytePos ?? 0;

        public byte LoBytePos => Motor.LoBytePos ?? 0;

        public int AbsolutePosition
        {
            get
            {
                if (Motor.HiBytePos.HasValue && Motor.LoBytePos.HasValue)
                {
                    return (int)((Motor.HiBytePos.Value << 8) | Motor.LoBytePos.Value);
                }
                else
                {
                    return 0; 
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


