using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PlcTester
{
    public class MotorValues : INotifyPropertyChanged
    {
        private int _motorIndex;
        private int _position;
        private int _speed;

        private bool _error { get; set; }
        private bool _inMinPosition { get; set; }
        private bool _inMaxPosition { get; set; }
        private bool _inCenteredPosition { get; set; }
        private bool _inHomePosition { get; set; }
        private bool _motorIsHomed { get; set; }
        private bool _motorInProgress {  get; set; }

        public int MotorIndex
        {
            get { return _motorIndex; }
            set
            {
                _motorIndex = value;
                OnPropertyChanged(nameof(MotorIndex));
            }
        }

        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }

        public bool Error
        {
            get => _error;
            set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged(nameof(Error));
                }
            }
        }
        public bool InMinPosition
        {
            get => _inMinPosition;
            set
            {
                if (_inMinPosition != value)
                {
                    _inMinPosition = value;
                    OnPropertyChanged(nameof(InMinPosition));
                }
            }
        }
        public bool InMaxPosition
        {
            get => _inMaxPosition;
            set
            {
                if (_inMaxPosition != value)
                {
                    _inMaxPosition = value;
                    OnPropertyChanged(nameof(InMaxPosition));
                }
            }
        }
        public bool InCenteredPosition
        {
            get => _inCenteredPosition;
            set
            {
                if (_inCenteredPosition != value)
                {
                    _inCenteredPosition = value;
                    OnPropertyChanged(nameof(InCenteredPosition));
                }
            }
        }
        public bool InHomePosition
        {
            get => _inHomePosition;
            set
            {
                if (_inHomePosition != value)
                {
                    _inHomePosition = value;
                    OnPropertyChanged(nameof(InHomePosition));
                }
            }
        }


        public bool MotorIsHomed
        {
            get => _motorIsHomed;
            set
            {
                if (_motorIsHomed != value)
                {
                    _motorIsHomed = value;
                    OnPropertyChanged(nameof(MotorIsHomed));
                }
            }
        }
        public bool MotorInProgress
        {
            get => _motorInProgress;
            set
            {
                if (_motorInProgress != value)
                {
                    _motorInProgress = value;
                    OnPropertyChanged(nameof(MotorInProgress));
                }
            }
        }

        //public bool Error { get; set; }
        //public bool InMinPosition { get; set; }
        //public bool InMaxPosition { get; set; }
        //public bool InCenteredPosition { get; set; }
        //public bool InHomePosition { get; set; }
        //public bool MotorIsHomed { get; set; }
        //public bool MotorInProgress { get; set; }



        public bool OperationMode { get; set; }
        public bool OverrideKey { get; set; }

        //255
        public bool ProhibitMovement { get; set; }
        public bool SickReset { get; set; }
        public bool SickActive { get; set; }
        public bool EStopReset { get; set; }
        public bool EStop { get; set; }
        //

        public bool MachineNeedsHoming { get; set; }
        public bool EButtonPressed { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}