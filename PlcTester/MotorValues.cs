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

        public bool Error { get; set; }
        public bool InMinPosition { get; set; }
        public bool InMaxPosition { get; set; }
        public bool InCenteredPosition { get; set; }
        public bool InHomePosition { get; set; }
        public bool MotorIsHomed { get; set; }
        public bool MotorInProgress { get; set; }

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