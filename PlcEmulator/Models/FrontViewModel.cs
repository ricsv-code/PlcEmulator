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
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Data;

namespace PlcEmulator
{

    public class FrontViewModel : INotifyPropertyChanged //samlar MotorViewModels från MotorService
    {
        public ObservableCollection<MotorViewModel> Motors { get; set; }
        private double _windowHeight;
        private double _windowWidth;
        private double _motorColumnWidth;
        private double _motorRowHeight;
        private int _numberOfMotors;

        public FrontViewModel()
        {
            Motors = new ObservableCollection<MotorViewModel>();
            GlobalSettings.NumberOfMotorsChanged += OnNumberOfMotorsChanged;
            _numberOfMotors = GlobalSettings.NumberOfMotors;
            UpdateWindowDimensions();
            UpdateMotorsCollection();
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

        public int NumberOfMotors
        {
            get => _numberOfMotors;
            set
            {
                _numberOfMotors = value;
                GlobalSettings.NumberOfMotors = value;
                OnPropertyChanged();
                UpdateWindowDimensions();
                UpdateMotorsCollection();
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
                WindowHeights = 910;
                WindowWidths = 1860;
                MotorColumnWidth = 820;
                MotorRowHeight = 320;
            }
        }

        private void OnNumberOfMotorsChanged(object sender, EventArgs e)
        {
            NumberOfMotors = GlobalSettings.NumberOfMotors;
            UpdateWindowDimensions();
            UpdateMotorsCollection();
        }

        private void UpdateMotorsCollection()
        {
            Motors.Clear();
            var motorServices = MotorService.Instances;
            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
            {
                Motors.Add(new MotorViewModel(motorServices[i].Motor, i));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public StackPanel CreateIndicator(string indicatorName, object source)
        {
            TextBlock indicatorTextBlock = new TextBlock
            {
                Name = $"{indicatorName}TextBlock",
                Text = indicatorName
            };

            Binding binding = new Binding(indicatorName)
            {
                Source = source,
                Converter = new BooleanToBrushConverter()
            };

            Ellipse indicatorEllipse = new Ellipse
            {
                Name = $"{indicatorName}Ellipse",
                Width = 10,
                Height = 10,
            };
            indicatorEllipse.SetBinding(Ellipse.FillProperty, binding);

            StackPanel indicatorStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            indicatorStackPanel.Children.Add(indicatorEllipse);
            indicatorStackPanel.Children.Add(indicatorTextBlock);

            return indicatorStackPanel;
        }

        public StackPanel CreateInfoText(string speed, string position, object source)
        {
            Binding speedBinding = new Binding(speed)
            {
                Source = source,
            };
            Binding posBinding = new Binding(position)
            {
                Source = source,
            };


            TextBlock speedTextBlock = new TextBlock
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 20,
            };
            speedBinding.StringFormat = "Speed: {0}";
            speedTextBlock.SetBinding(TextBlock.TextProperty, speedBinding);
            

            TextBlock positionTextBlock = new TextBlock
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 20,
                Margin = new System.Windows.Thickness(10,0,0,0)
            };
            posBinding.StringFormat = "Position: {0}";
            positionTextBlock.SetBinding(TextBlock.TextProperty, posBinding);


            StackPanel infoStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };

            infoStackPanel.Children.Add(speedTextBlock);
            infoStackPanel.Children.Add(positionTextBlock);

            return infoStackPanel;
        }
    }
}


