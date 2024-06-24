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
using System.Threading;

namespace PlcEmulator
{
    public class MotorViewModel : INotifyPropertyChanged //binder ui properties till MotorClass instanser
    {

        private double _windowHeight;
        private double _windowWidth;
        private double _motorColumnWidth;
        private double _motorRowHeight;
        private int _numberOfMotors;


        public MotorViewModel()
        {
            _numberOfMotors = GlobalSettings.NumberOfMotors;
            UpdateWindowDimensions();

        }

        private MotorClass _motor;
        public int MotorIndex { get; }
        public MotorViewModel(MotorClass motor, int index)
        {
            _motor = motor;
            MotorIndex = (index + 1);
        }
        public MotorClass Motor => _motor;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Reserved
        {
            get => _motor.Reserved;
            set
            {
                if (_motor.Reserved != value)
                {
                    _motor.Reserved = value;
                    OnPropertyChanged(nameof(Reserved));
                }
            }
        }
        public bool Error
        {
            get => _motor.Error;
            set
            {
                if (_motor.Error != value)
                {
                    _motor.Error = value;
                    OnPropertyChanged(nameof(Error));
                }
            }
        }
        public bool InMinPosition
        {
            get => _motor.InMinPosition;
            set
            {
                if (_motor.InMinPosition != value)
                {
                    _motor.InMinPosition = value;
                    OnPropertyChanged(nameof(InMinPosition));
                }
            }
        }
        public bool InMaxPosition
        {
            get => _motor.InMaxPosition;
            set
            {
                if (_motor.InMaxPosition != value)
                {
                    _motor.InMaxPosition = value;
                    OnPropertyChanged(nameof(InMaxPosition));
                }
            }
        }
        public bool InCenteredPosition
        {
            get => _motor.InCenteredPosition;
            set
            {
                if (_motor.InCenteredPosition != value)
                {
                    _motor.InCenteredPosition = value;
                    OnPropertyChanged(nameof(InCenteredPosition));
                }
            }
        }
        public bool InHomePosition
        {
            get => _motor.InHomePosition;
            set
            {
                if (_motor.InHomePosition != value)
                {
                    _motor.InHomePosition = value;
                    OnPropertyChanged(nameof(InHomePosition));
                }
            }
        }


        public bool MotorIsHomed
        {
            get => _motor.MotorIsHomed;
            set
            {
                if (_motor.MotorIsHomed != value)
                {
                    _motor.MotorIsHomed = value;
                    OnPropertyChanged(nameof(MotorIsHomed));
                }
            }
        }
        public bool MotorInProgress
        {
            get => _motor.MotorInProgress;
            set
            {
                if (_motor.MotorInProgress != value)
                {
                    _motor.MotorInProgress = value;
                    OnPropertyChanged(nameof(MotorInProgress));
                }
            }
        }

        public bool OverrideKey
        {
            get => _motor.OverrideKey;
            set
            {
                if (_motor.OverrideKey != value)
                {
                    _motor.OverrideKey = value;
                    OnPropertyChanged(nameof(OverrideKey));
                }
            }
        }

        public bool OperationMode
        {
            get => _motor.OperationMode;
            set
            {
                if (_motor.OperationMode != value)
                {
                    _motor.OperationMode = value;
                    OnPropertyChanged(nameof(OperationMode));
                }
            }
        }

        public bool MachineNeedsHoming
        {
            get => _motor.MachineNeedsHoming;
            set
            {
                if (_motor.MachineNeedsHoming != value)
                {
                    _motor.MachineNeedsHoming = value;
                    OnPropertyChanged(nameof(MachineNeedsHoming));
                }
            }
        }

        public bool MachineStill
        {
            get => _motor.MachineStill;
            set
            {
                if (_motor.MachineStill != value)
                {
                    _motor.MachineStill = value;
                    OnPropertyChanged(nameof(MachineStill));
                }
            }
        }

        public bool MachineInMotion
        {
            get => _motor.MachineInMotion;
            set
            {
                if (_motor.MachineInMotion != value)
                {
                    _motor.MachineInMotion = value;
                    OnPropertyChanged(nameof(MachineInMotion));
                }
            }
        }

        public bool ProhibitMovement
        {
            get => _motor.ProhibitMovement;
            set
            {
                if (_motor.ProhibitMovement != value)
                {
                    _motor.ProhibitMovement = value;
                    OnPropertyChanged(nameof(ProhibitMovement));
                }
            }
        }

        public bool SickReset
        {
            get => _motor.SickReset;
            set
            {
                if (_motor.SickReset != value)
                {
                    _motor.SickReset = value;
                    OnPropertyChanged(nameof(SickReset));
                }
            }
        }

        public bool SickActive
        {
            get => _motor.SickActive;
            set
            {
                if (_motor.SickActive != value)
                {
                    _motor.SickActive = value;
                    OnPropertyChanged(nameof(SickActive));
                }
            }
        }

        public bool EStopReset
        {
            get => _motor.EStopReset;
            set
            {
                if (_motor.EStopReset != value)
                {
                    _motor.EStopReset = value;
                    OnPropertyChanged(nameof(EStopReset));
                }
            }
        }

        public bool EStop
        {
            get => _motor.EStop;
            set
            {
                if (_motor.EStop != value)
                {
                    _motor.EStop = value;
                    OnPropertyChanged(nameof(EStop));
                }
            }
        }
        public byte? OperationalSpeed
        {
            get => _motor.OperationalSpeed ?? 0;
            set
            {
                if (_motor.OperationalSpeed != value)
                {
                    _motor.OperationalSpeed = value;
                    OnPropertyChanged(nameof(OperationalSpeed));
                }
            }
        }

        public byte? HiBytePos
        {
            get => _motor.HiBytePos ?? 0;
            set
            {
                if (_motor.HiBytePos != value)
                {
                    _motor.HiBytePos = value;
                    OnPropertyChanged(nameof(HiBytePos));
                    OnPropertyChanged(nameof(AbsolutePosition));

                }
            }
        }

        public byte? LoBytePos
        {
            get => _motor.LoBytePos ?? 0;
            set
            {
                if (_motor.LoBytePos != value)
                {
                    _motor.LoBytePos = value;
                    OnPropertyChanged(nameof(LoBytePos));
                    OnPropertyChanged(nameof(AbsolutePosition));
                }
            }
        }

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

        public int TargetPosition
        {
            get => _motor.TargetPosition;
            set
            {
                if (_motor.TargetPosition != value)
                {
                    _motor.TargetPosition = value;
                    OnPropertyChanged(nameof(TargetPosition));
                }
            }
        }

        //ui-grejer
        public void UpdateIndicators()
        {
            if ((_motor.HiBytePos ?? 0) == 0 && (_motor.LoBytePos ?? 0) == 0)
            {
                InHomePosition = true;
            }
            else
            {
                InHomePosition = false;
            }
            if (AbsolutePosition == 3142)
            {
                InCenteredPosition = true;
            }
            else
            {
                InCenteredPosition = false;
            }
        }

        public int NumberOfMotors
        {
            get => _numberOfMotors;
            set
            {
                _numberOfMotors = value;
                OnPropertyChanged(nameof(NumberOfMotors));
                GlobalSettings.NumberOfMotors = value;
                UpdateWindowDimensions();
            }
        }
        public double WindowHeights
        {
            get => _windowHeight;
            set
            {
                _windowHeight = value;
                OnPropertyChanged();
            }
        }

        public double WindowWidths
        {
            get => _windowWidth;
            set
            {
                _windowWidth = value;
                OnPropertyChanged();
            }
        }

        public double MotorColumnWidth
        {
            get => _motorColumnWidth;
            set
            {
                _motorColumnWidth = value;
                OnPropertyChanged();
            }
        }

        public double MotorRowHeight
        {
            get => _motorRowHeight;
            set
            {
                _motorRowHeight = value;
                OnPropertyChanged();
            }
        }

        private void UpdateWindowDimensions()
        {
            if (NumberOfMotors == 4)
            {
                WindowHeights = 650;
                WindowWidths = 1600;
                MotorColumnWidth = 580;
                MotorRowHeight = 200;

            }
            else if (NumberOfMotors == 9)
            {
                WindowHeights = 810;
                WindowWidths = 1860;
                MotorColumnWidth = 820;
                MotorRowHeight = 320;
            }
        }



    }
}


