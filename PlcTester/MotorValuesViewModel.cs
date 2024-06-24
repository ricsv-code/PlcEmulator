using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media.Animation;
using Utilities;

namespace PlcTester
{
    public class MotorValuesViewModel : INotifyPropertyChanged
    {
        private int _numberOfMotors;
        private ObservableCollection<MotorValues> _motors;
        public ObservableCollection<MotorValues> Motors
        {
            get { return _motors; }
            set
            {
                _motors = value;
                OnPropertyChanged(nameof(Motors));
            }
        }

        public MotorValuesViewModel()
        {
            _numberOfMotors = GlobalSettings.NumberOfMotors;
            Motors = new ObservableCollection<MotorValues>();
            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)//skapa 4 elr 9 motorer
            {
                Motors.Add(new MotorValues { MotorIndex = i + 1});
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
                Motors.Clear();
                for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
                {
                    Motors.Add(new MotorValues { MotorIndex = i + 1 });
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