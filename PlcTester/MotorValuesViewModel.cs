using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;
using Utilities;

namespace PlcTester
{
    public class MotorValuesViewModel : INotifyPropertyChanged
    {
        private int _numberOfMotors;

        public MotorValuesViewModel()
        {
            _numberOfMotors = GlobalSettings.NumberOfMotors;

        }
        private MotorValues _motor;
        public int MotorIndex { get; }
        public MotorValuesViewModel(MotorValues motor, int index)
        {
            _motor = motor;
            MotorIndex = (index + 1);
        }
        public MotorValues Motor => _motor;

        public int Speed
        {
            get => _motor.Speed;
            set
            {
                if (_motor.Speed != value)
                {
                    _motor.Speed = value;
                    OnPropertyChanged(nameof(Speed));
                }
            }
        }

        public int Position
        {
            get => _motor.Position;
            set
            {
                if (_motor.Position != value)
                {
                    _motor.Position = value;
                    OnPropertyChanged(nameof(Position));
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

        public bool Reserved
        {
            get => _motor.Reserved;
            set
            {
                if (_motor.Reserved)
                {
                    _motor.Reserved = value;
                    OnPropertyChanged(nameof(Reserved));
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

        public int NumberOfMotors
        {
            get => _numberOfMotors;
            set
            {
                _numberOfMotors = value;
                OnPropertyChanged(nameof(NumberOfMotors));
                GlobalSettings.NumberOfMotors = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}