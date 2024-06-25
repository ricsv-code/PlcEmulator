using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using PlcEmulatorCore;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Utilities;
using System.Windows.Controls;
using System.ComponentModel;


namespace PlcEmulator
{
    public partial class PlcEmulatorGui : Window
    {
        private EmulatorPlc _emulator;
        private Stopwatch _stopwatch;
        private bool _isRunning;
        private MotorViewModel _viewModel;

        private Dictionary<int, DispatcherTimer> _motorTimers = new Dictionary<int, DispatcherTimer>();

        public PlcEmulatorGui()
        {
            InitializeComponent();
            CreateMotorImages();
            _viewModel = (MotorViewModel)DataContext;
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
                    ConnectionIndicator.Fill = Brushes.Green;

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

                    ConnectionIndicator.Fill = Brushes.Red;

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

                    _viewModel.NumberOfMotors = numberOfMotors;

                    CreateMotorImages();
                }
            }
        }

        private void SetMotorButton_Click(object sender, RoutedEventArgs e) 
        {
            byte motorIndex = byte.Parse(SetMotorIndexTextBox.Text);
            int homePosition = int.Parse(HomePositionTextBox.Text);
            int centerPosition = int.Parse(CenterPositionTextBox.Text);
            int minPosition = int.Parse(MinPositionTextBox.Text);
            int maxPosition = int.Parse(MaxPositionTextBox.Text);

            var motor = MotorService.Instances[motorIndex - 1];

            motor.HomePosition = homePosition;
            motor.CenterPosition = centerPosition;
            motor.MinPosition = minPosition;
            motor.MaxPosition = maxPosition;

            motor.UpdateIndicators();
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



        private void UpdateImage(int motorIndex)
        {

            Dispatcher.Invoke(() =>
            {
                var motor = MotorService.Instances[motorIndex];


                if (motor != null)
                {
                    if (motor.AbsolutePosition != motor.TargetPosition)
                    {

                        int speed = (byte)motor.OperationalSpeed;

                        if (!_motorTimers.ContainsKey(motorIndex))
                        {
                            var timer = new DispatcherTimer();
                            timer.Interval = TimeSpan.FromMilliseconds(10);
                            timer.Tick += (sender, e) => RotateMotor(motorIndex);
                            _motorTimers[motorIndex] = timer;
                        }

                        motor.MachineInMotion = true;
                        _motorTimers[motorIndex].Start();
                    }
                    else
                    {
                        if (_motorTimers.ContainsKey(motorIndex))
                        {
                            _motorTimers[motorIndex].Stop();
                            motor.MachineInMotion = false;
                        }
                    }
                }
            });
        }


        private void RotateMotor(int motorIndex)
        {
            var motor = MotorService.Instances[motorIndex];
            if (motor == null) return;

            double targetAngle = Helpers.RadiansToDegrees(motor.TargetPosition);
            double currentAngle = Helpers.RadiansToDegrees(motor.AbsolutePosition);

            if (Math.Abs(currentAngle - targetAngle) > 0.1) //AVRUNDNING borttagen
            {
                int direction = currentAngle < targetAngle ? 1 : -1;
                
                int speed = (byte)motor.OperationalSpeed;

                double rotationStep = Math.Min((speed * 0.05), Math.Abs(targetAngle - currentAngle));//overshoot protection

                currentAngle += direction * rotationStep;

                motor.HiBytePos = (byte)((int)Helpers.DegreesToRadians(currentAngle) >> 8);
                motor.LoBytePos = (byte)((int)Helpers.DegreesToRadians(currentAngle) & 0xFF);
                motor.UpdateIndicators();

                if ((direction > 0 && currentAngle >= targetAngle) || //overshoot protection
                    (direction < 0 && currentAngle <= targetAngle))
                {
                    currentAngle = targetAngle;
                }

                _updateMotorImage(motorIndex, currentAngle);
                

            }
            else
            {
                currentAngle = targetAngle;
                motor.HiBytePos = (byte)((int)Helpers.DegreesToRadians(currentAngle) >> 8);
                motor.LoBytePos = (byte)((int)Helpers.DegreesToRadians(currentAngle) & 0xFF);
                motor.UpdateIndicators();
                
                if (_motorTimers.ContainsKey(motorIndex))
                {
                    _motorTimers[motorIndex].Stop();
                    motor.MachineInMotion = false;
                }
            }
        }

        private void _updateMotorImage(int motorIndex, double currentAngle)
        {

            Dispatcher.Invoke(() =>
            {
                StackPanel stackPanel = imageContainer.Children[motorIndex] as StackPanel;
                StackPanel iStackPanel = stackPanel.Children[1] as StackPanel;
                System.Windows.Controls.Image image = iStackPanel.Children[1] as System.Windows.Controls.Image;

                if (image != null && image.RenderTransform is RotateTransform rotateTransform)
                {
                    var motor = MotorService.Instances[motorIndex];
                    rotateTransform.Angle = currentAngle;
                    //textBoxImageData.Text = ("Rotated motor " + (motorIndex + 1) + ": " + (int)currentAngle + "°");
                    textBoxImageData.Text = ("LoByte: " + motor.LoBytePos + " | HiByte: " + motor.HiBytePos);
                }
            });
        }

        private void ShowStopper()
        {
            foreach (var timer in _motorTimers.Values)
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
            }

            foreach (var motor in MotorService.Instances)
            {
                motor.MachineInMotion = false;
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

                var motorViewModel = MotorService.Instances[i];

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


                var iStackPanel = GuiCreators.CreateInfoText("OperationalSpeed", "AbsolutePosition", motorViewModel);

                //Skapa indicators med bindings
                var machineInMotionStackPanel = GuiCreators.CreateIndicator("MachineInMotion", motorViewModel);
                var machineStillStackPanel = GuiCreators.CreateIndicator("MachineStill", motorViewModel);
                var machineNeedsHomingStackPanel = GuiCreators.CreateIndicator("MachineNeedsHoming", motorViewModel);
                var machineInCenterStackPanel = GuiCreators.CreateIndicator("InCenteredPosition", motorViewModel);
                var machineInHomeStackPanel = GuiCreators.CreateIndicator("InHomePosition", motorViewModel);
                var eButtonPressedStackPanel = GuiCreators.CreateIndicator("EStop", motorViewModel);


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