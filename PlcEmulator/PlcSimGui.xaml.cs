using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using PlcEmulatorCore;
using PlcEmulator;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;
using System.Reflection.Metadata;
using System.Windows.Data;
using System.Windows.Media;
using Utilities;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using System.Windows.Shapes;

namespace PlcEmulator
{
    public partial class PlcSimGui : Window
    {
        private EmulatorPlc _emulator;
        private Stopwatch _stopwatch;
        private bool _isRunning;

        private Dictionary<int, DispatcherTimer> _motorTimers = new Dictionary<int, DispatcherTimer>();
        private Dictionary<int, double> _currentAngles = new Dictionary<int, double>();
        private const double Tolerance = 0.1; //avrundningsfel

        private int _targetAngle;
        private int _currentAngle;
        private int _motorIndex;
        private int _rotationStep;



        public PlcSimGui()
        {
            InitializeComponent();
            CreateMotorImages();
            buttonStop.IsEnabled = false;
        }



        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning == false)
            {
                string ipAddress = IpAddressTextBox.Text;
                int port = int.Parse(PortTextBox.Text);

                _stopwatch = new Stopwatch();
                _emulator = new EmulatorPlc(ipAddress, port, UpdateReceivedData, UpdateSentData, UpdateOperation, UpdateImage);

                _stopwatch.Start();
                _emulator.Start();
                _isRunning = true;

                buttonStart.IsEnabled = false;
                buttonStop.IsEnabled = true;
                ConnectionIndicator.Fill = System.Windows.Media.Brushes.Green;


                textBoxOperation.Text = $"PLC Emulator started..\r\n";
            }
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (_emulator != null)
            {
                _stopwatch.Stop();
                _emulator.Stop();
                _isRunning = false;
                buttonStart.IsEnabled = true;
                buttonStop.IsEnabled = false;

                ConnectionIndicator.Fill = System.Windows.Media.Brushes.Red;

                textBoxOperation.Text = "PLC Emulator stopped..\r\n";
            }
        }

        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxSentData.Text = string.Empty;
            textBoxReceivedData.Text = string.Empty;
        }

        private void NumberOfMotorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.IsChecked)
            {
                if (int.TryParse(menuItem.Header.ToString(), out int numberOfMotors))
                {
                    var viewModel = DataContext as FrontViewModel;
                    viewModel.NumberOfMotors = numberOfMotors;

                    CreateMotorImages();
                }
            }
        }

        private void UpdateReceivedData(string data)
        {
            Dispatcher.Invoke(() =>
            {
                textBoxReceivedData.AppendText(string.Format("{0:00}:{1:00}:{2:000}", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds) + " | " + data + "\r\n");
                textBoxReceivedData.ScrollToEnd();
            });
        }

        private void UpdateSentData(string data)
        {
            Dispatcher.Invoke(() =>
            {
                textBoxSentData.AppendText(string.Format("{0:00}:{1:00}:{2:000}", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds) + " | " + data + "\r\n");
                textBoxSentData.ScrollToEnd();
            });
        }

        private void UpdateOperation(string data)
        {
            Dispatcher.Invoke(() =>
            {
                textBoxOperation.Text = data;
                textBoxOperation.ScrollToEnd();
            });
        }

        private void UpdateImage(byte[] request, int motorIndex)
        {
            int position = (request[2] << 8) | request[3];
            decimal angleRadians = position / 1000.0m;
            double angleDegrees = (double)(angleRadians * (180m / (decimal)Math.PI));
            _targetAngle = (int)angleDegrees;

            Dispatcher.Invoke(() =>
            {
                var viewModel = DataContext as FrontViewModel;
                var motorViewModel = viewModel?.Motors[motorIndex];

                if (motorViewModel != null)
                {
                    int speed = motorViewModel.OperationalSpeed;
                    _rotationStep = Math.Max(1, speed); //minimum speed om speed=0 ("hoppstorlek")

                    if (!_motorTimers.ContainsKey(motorIndex))
                    {
                        var timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromMilliseconds(10); //kanske ha speed-variabeln här ist?
                        timer.Tick += (sender, e) => RotateMotor(sender, e, motorIndex, speed);
                        _motorTimers[motorIndex] = timer;
                    }

                    if (!_motorTimers[motorIndex].IsEnabled)
                    {
                        if (!_currentAngles.ContainsKey(motorIndex))
                        {
                            _currentAngles[motorIndex] = 0;
                        }
                        _motorTimers[motorIndex].Start();
                    }
                }
            });
        }

        private void RotateMotor(object sender, EventArgs e, int motorIndex, int speed)
        {
            var viewModel = DataContext as FrontViewModel;
            var motorViewModel = viewModel?.Motors[motorIndex];
            if (motorViewModel == null) return;

            int position = (motorViewModel.HiBytePos << 8) | motorViewModel.LoBytePos;
            decimal angleRadians = position / 1000.0m;
            double targetAngle = (double)(angleRadians * (180m / (decimal)Math.PI));

            if (!_currentAngles.ContainsKey(motorIndex)) //plåster (ta bort sen??)
            {
                _currentAngles[motorIndex] = 0;
            }

            double currentAngle = _currentAngles[motorIndex];

            if (Math.Abs(currentAngle - targetAngle) > Tolerance) //AVRUNDNING (ta bort senare)
            {
                int direction = currentAngle < targetAngle ? 1 : -1; 
                currentAngle += direction * _rotationStep;

                if ((direction > 0 && currentAngle > targetAngle) || 
                    (direction < 0 && currentAngle < targetAngle))
                {
                    currentAngle = targetAngle;
                    motorViewModel.UpdateIndicators();
                }

                Dispatcher.Invoke(() =>
                {
                    StackPanel stackPanel = imageContainer.Children[motorIndex] as StackPanel;
                    StackPanel iStackPanel = stackPanel.Children[1] as StackPanel;
                    System.Windows.Controls.Image image = iStackPanel.Children[1] as System.Windows.Controls.Image;

                    if (image != null && image.RenderTransform is RotateTransform rotateTransform)
                    {
                        rotateTransform.Angle = currentAngle;
                        textBoxImageData.Text = ("Rotated motor " + (motorIndex + 1) + ": " + (double)currentAngle + "");
                        TextBlock motorInfoTextBlock = (TextBlock)this.FindName("motorInfoTextBlock");
                        if (motorInfoTextBlock != null)
                        {
                            motorInfoTextBlock.Text = ("Position:" + position + "Speed:" + speed);
                            motorViewModel.UpdateIndicators();
                        }
                    }
                });

                _currentAngles[motorIndex] = currentAngle;
            }
            else
            {
                _motorTimers[motorIndex].Stop();
                motorViewModel.UpdateIndicators();
            }
        }

        private void CreateMotorImages()
        {
            Dispatcher.Invoke(() => //testa ta bort?
            {
                while (imageContainer.Children.Count > 0) 
                {
                    var child = imageContainer.Children[0];
                    imageContainer.Children.Remove(child);
                }
            }); 

            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
            {
                var motorImage = new BitmapImage(new Uri("C:\\Users\\risve\\Source\\Repos\\plc_emulator\\PlcEmulator\\Data\\arrow-right.png"));

                var image = new System.Windows.Controls.Image();

                var viewModel = DataContext as FrontViewModel;
                var motorViewModel = viewModel?.Motors[i];
                if (motorViewModel == null) return;

                motorViewModel.UpdateIndicators();


                image.Source = motorImage;
                {
                    image.Width = 100;
                    image.Height = 100;
                    image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                };

                TextBlock mTextBlock = new TextBlock();
                {
                    mTextBlock.Text = $"Motor: {i + 1}";
                    mTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    mTextBlock.VerticalAlignment = VerticalAlignment.Top;
                    mTextBlock.Height = 20;
                };

                TextBlock iTextBlock = new TextBlock();
                {
                    iTextBlock.Name = "motorInfoTextBlock";
                    iTextBlock.Text = "TestText";
                    iTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    iTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    iTextBlock.Height = 20;
                };

                //Skapa indicators med bindings
                var machineInMotionStackPanel = viewModel.CreateIndicator("MachineInMotion", motorViewModel);
                var machineStillStackPanel = viewModel.CreateIndicator("MachineStill", motorViewModel);
                var machineNeedsHomingStackPanel = viewModel.CreateIndicator("MachineNeedsHoming", motorViewModel);
                var machineInCenterStackPanel = viewModel.CreateIndicator("InCentredPosition", motorViewModel);
                var machineInHomeStackPanel = viewModel.CreateIndicator("InHomePosition", motorViewModel);
                var eButtonPressedStackPanel = viewModel.CreateIndicator("EStop", motorViewModel);

                //Flytta ut de ovan??


                StackPanel statusStackPanel = new StackPanel();
                {
                    statusStackPanel.Orientation = Orientation.Vertical;
                    statusStackPanel.Margin = new Thickness(0, 0, 10, 0);
                    statusStackPanel.Children.Add(machineInMotionStackPanel);
                    statusStackPanel.Children.Add(machineStillStackPanel);
                    statusStackPanel.Children.Add(machineNeedsHomingStackPanel);
                    statusStackPanel.Children.Add(machineInCenterStackPanel);
                    statusStackPanel.Children.Add(machineInHomeStackPanel);
                    statusStackPanel.Children.Add(eButtonPressedStackPanel);
                };

                StackPanel horizontalStackPanel = new StackPanel();
                {
                    horizontalStackPanel.Orientation = Orientation.Horizontal;
                    horizontalStackPanel.Children.Add(statusStackPanel);
                    horizontalStackPanel.Children.Add(image);
                };

                StackPanel verticalStackPanel = new StackPanel();
                {

                    verticalStackPanel.Orientation = Orientation.Vertical;
                    verticalStackPanel.Children.Add(mTextBlock);
                    verticalStackPanel.Children.Add(horizontalStackPanel);
                    verticalStackPanel.Children.Add(iTextBlock);
                };



                RotateTransform rotateTransform = new RotateTransform(0);
                image.RenderTransform = rotateTransform;

                Dispatcher.Invoke(() =>
                {
                    imageContainer.Children.Add(verticalStackPanel);
                });

            }
        }
    }
}