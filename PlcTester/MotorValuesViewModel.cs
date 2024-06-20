using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PlcTester
{
    public class MotorValuesViewModel : INotifyPropertyChanged
    {
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
            Motors = new ObservableCollection<MotorValues>();
            for (int i = 0; i < 9; i++)//skapa 9 motorer
            {
                Motors.Add(new MotorValues { MotorIndex = i + 1});
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}