using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using PlcEmulatorCore;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Utilities;
using System.Windows.Controls;


namespace PlcEmulator
{
    public partial class PlcSimGui : Window
    {
        private EmulatorPlc _emulator;
        private Stopwatch _stopwatch;
        private bool _isRunning;

        private Dictionary<int, DispatcherTimer> _motorTimers = new Dictionary<int, DispatcherTimer>();
        private Dictionary<int, double> _currentAngles = new Dictionary<int, double>();
        private Dictionary<int, double> _targetAngles = new Dictionary<int, double>();


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
            try
            {
                if (_isRunning == false)
                {
                    string ipAddress = IpAddressTextBox.Text;
                    int port = int.Parse(PortTextBox.Text);

                    _stopwatch = new Stopwatch();
                    _emulator = new EmulatorPlc(ipAddress, port, UpdateReceivedData, UpdateSentData, UpdateOperation, UpdateImage, ShowStopper);

                    _stopwatch.Start();
                    _emulator.Start();
                    _isRunning = true;

                    buttonStart.IsEnabled = false;
                    buttonStop.IsEnabled = true;
                    ConnectionIndicator.Fill = System.Windows.Media.Brushes.Green;

                    textBoxOperation.Text = $"PLC Emulator started..\r\n";
                }
            }
            catch (Exception ex)
            {
                textBoxReceivedData.AppendText($"Error: {ex.Message}$\r\n");
            }

        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                textBoxReceivedData.AppendText($"Error: {ex.Message}$\r\n");
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

        private static double RadiansToDegrees(int radians)
        {
            decimal angleRadians = radians / 1000.0m;
            double angleDegrees = (double)(angleRadians * (180m / (decimal)Math.PI));
            return angleDegrees;
        }

        private static double DegreesToRadians(double degrees)
        {
            double radians = degrees * (Math.PI / 180);
            double output = radians * 1000;
            return output;
        }

        private void UpdateImage(int motorIndex, int targetPos)
        {



            Dispatcher.Invoke(() =>
            {
                var viewModel = DataContext as FrontViewModel;
                var motorViewModel = viewModel?.Motors[motorIndex];

                _targetAngles[motorIndex] = RadiansToDegrees(targetPos);


                if (motorViewModel != null)
                {
                    int speed = (byte)motorViewModel.OperationalSpeed;
                    int intervalSpeed = 110 - speed; //justera efter hastighet
                    _rotationStep = 5; //5 grader i taget

                    var timer = new DispatcherTimer();

                    timer.Interval = TimeSpan.FromMilliseconds(intervalSpeed); //speed
                    timer.Tick += (sender, e) => RotateMotor(motorIndex, speed);
                    _motorTimers[motorIndex] = timer;


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

        private void RotateMotor(int motorIndex, int speed)
        {
            var viewModel = DataContext as FrontViewModel;
            var motorViewModel = viewModel?.Motors[motorIndex];
            if (motorViewModel == null) return;

            double targetAngle = _targetAngles[motorIndex];

            double currentAngle = _currentAngles[motorIndex];

            if (Math.Abs(currentAngle - targetAngle) > 0) //AVRUNDNING borttagen
            {
                int direction = currentAngle < targetAngle ? 1 : -1;
                currentAngle += direction * _rotationStep;

                motorViewModel.UpdateIndicators();

                //spara positionerna här eventuellt??

                if ((direction > 0 && currentAngle > targetAngle) || //overshoot protection
                    (direction < 0 && currentAngle < targetAngle))
                {
                    currentAngle = targetAngle;

                    
                }

                Dispatcher.Invoke(() =>
                {
                    StackPanel stackPanel = imageContainer.Children[motorIndex] as StackPanel;
                    StackPanel iStackPanel = stackPanel.Children[1] as StackPanel;
                    System.Windows.Controls.Image image = iStackPanel.Children[1] as System.Windows.Controls.Image;

                    if (image != null && image.RenderTransform is RotateTransform rotateTransform)
                    {
                        int hiByte = (int)motorViewModel.HiBytePos;
                        int loByte = (int)motorViewModel.LoBytePos;

                        rotateTransform.Angle = currentAngle;
                        //textBoxImageData.Text = ("Rotated motor " + (motorIndex + 1) + ": " + (int)currentAngle + "°");
                        textBoxImageData.Text = ("LoByte: " + loByte + " | HiByte: " + hiByte);
                    }
                });

                _currentAngles[motorIndex] = currentAngle;
            }
            else
            {
                if (_motorTimers.ContainsKey(motorIndex))
                {
                    _motorTimers[motorIndex].Stop();
                    _motorTimers.Remove(motorIndex);
                }

                motorViewModel.UpdateIndicators();

            }
        }

        private void ShowStopper(int motorIndex)
        {
            foreach (var timer in _motorTimers.Values)
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
            }

        }




        private void CreateMotorImages()
        {
            Dispatcher.Invoke(() => //måste vara kvar
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


                var iStackPanel = viewModel.CreateInfoText("OperationalSpeed", "AbsolutePosition", motorViewModel);

                //Skapa indicators med bindings
                var machineInMotionStackPanel = viewModel.CreateIndicator("MachineInMotion", motorViewModel);
                var machineStillStackPanel = viewModel.CreateIndicator("MachineStill", motorViewModel);
                var machineNeedsHomingStackPanel = viewModel.CreateIndicator("MachineNeedsHoming", motorViewModel);
                var machineInCenterStackPanel = viewModel.CreateIndicator("InCenteredPosition", motorViewModel);
                var machineInHomeStackPanel = viewModel.CreateIndicator("InHomePosition", motorViewModel);
                var eButtonPressedStackPanel = viewModel.CreateIndicator("EStop", motorViewModel);


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
                    verticalStackPanel.Children.Add(iStackPanel);
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