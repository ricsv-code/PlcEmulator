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
    public partial class PlcEmulatorGui : Window
    {
        private PlcProcess _emulator;
        private Stopwatch _stopwatch;
        private bool _isRunning;
        private Dictionary<int, DispatcherTimer> _motorTimers = new Dictionary<int, DispatcherTimer>();
        private bool DarkModeOn;

        public PlcEmulatorGui()
        {
            InitializeComponent();

            _emulator = new PlcProcess(UpdateReceivedData, UpdateSentData, UpdateOperation, UpdateImage, ShowStopper);

            root.DataContext = this;

            _stopwatch = new Stopwatch();

            UpdateMenuItems();
            CreateMotorImages();
                        
            ButtonStop.IsEnabled = false;
        }


        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            ResourceDictionary theme = new ResourceDictionary();
            theme.Source = new Uri("Resources/DarkTheme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(theme);

            DarkModeOn = true;
            UpdateMenuItems();
            UpdateLayout();
        }

        private void StandardMode_Click(object sender, RoutedEventArgs e)
        {
            ResourceDictionary theme = new ResourceDictionary();
            theme.Source = new Uri("Resources/StandardTheme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(theme);

            DarkModeOn = false;
            UpdateMenuItems();
            UpdateLayout();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isRunning == false)
                {
                    string ipAddress = IpAddressTextBox.Text;
                    int port = int.Parse(PortTextBox.Text);
 
                    
                    _stopwatch.Restart();

                    _emulator.Start(ipAddress, port);
                    _isRunning = true;

                    ButtonStart.IsEnabled = false;
                    ButtonStop.IsEnabled = true;
                    ConnectionIndicator.Fill = Brushes.Green;

                    textBoxOperation.Text = $"PLC Emulator started..\r\n";
                }
            }
            catch (Exception ex)
            {
                textBoxReceivedData.AppendText($"Error: {ex.Message}$\r\n");
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_emulator != null)
                {
                    _stopwatch.Stop();
                    _emulator.Stop();
                    _isRunning = false;
                    ButtonStart.IsEnabled = true;
                    ButtonStop.IsEnabled = false;

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
            if (sender is MenuItem menuItem)
            {
                if (int.TryParse(menuItem.Header.ToString(), out int numberOfMotors))
                {
                    if (GlobalSettings.NumberOfMotors != numberOfMotors)
                    {
                        GlobalSettings.NumberOfMotors = numberOfMotors;

                        CreateMotorImages();
                    }
                }
                UpdateMenuItems();
            }
        }

        private void UpdateMenuItems()
        {
            Menu4.IsChecked = GlobalSettings.NumberOfMotors == 4;
            Menu9.IsChecked = GlobalSettings.NumberOfMotors == 9;
            DarkMode.IsChecked = DarkModeOn;
            StandardMode.IsChecked = !DarkModeOn;
        }

        private void ScriptsButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptDialog dialog = new ScriptDialog();

            if (dialog.ShowDialog() == true)
            {
                _emulator.RunScript(dialog.Scripts);
            }
        }

        private void SetMotorButton_Click(object sender, RoutedEventArgs e)
        {
            byte motorIndex = byte.Parse(SetMotorIndexTextBox.Text);
            int homePosition = int.Parse(HomePositionTextBox.Text);
            int centerPosition = int.Parse(CenterPositionTextBox.Text);
            int minPosition = int.Parse(MinPositionTextBox.Text);
            int maxPosition = int.Parse(MaxPositionTextBox.Text);

            var motor = _emulator.PlcMachine.Motors[motorIndex - 1];

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
                var motor = _emulator.PlcMachine.Motors[motorIndex];

                if (motor != null)
                {
                    motor.UpdateIndicators();

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

                        motor.MotorInProgress = true;
                        _motorTimers[motorIndex].Start();
                    }
                    else
                    {
                        if (_motorTimers.ContainsKey(motorIndex))
                        {
                            _motorTimers[motorIndex].Stop();
                            motor.MotorInProgress = false;
                        }
                    }
                }
            });
        }

        private void RotateMotor(int motorIndex)
        {
            var motor = _emulator.PlcMachine.Motors[motorIndex];
            if (motor == null) return;

            double targetAngle = Helpers.RadiansToDegrees(motor.TargetPosition);
            double currentAngle = Helpers.RadiansToDegrees(motor.AbsolutePosition);

            if (Math.Abs(currentAngle - targetAngle) > 0.1) 
            {
                int direction = currentAngle < targetAngle ? 1 : -1;

                int speed = (byte)motor.OperationalSpeed;

                double rotationStep = Math.Min((speed * 0.05), Math.Abs(targetAngle - currentAngle));

                currentAngle += direction * rotationStep;

                motor.HiBytePos = (byte)((int)Helpers.DegreesToRadians(currentAngle) >> 8);
                motor.LoBytePos = (byte)((int)Helpers.DegreesToRadians(currentAngle) & 0xFF);
                motor.UpdateIndicators();

                if ((direction > 0 && currentAngle >= targetAngle) || 
                    (direction < 0 && currentAngle <= targetAngle))
                {
                    currentAngle = targetAngle;
                }

                UpdateMotorImage(motorIndex, currentAngle);


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
                    motor.MotorInProgress = false;
                }
            }
        }

        private void UpdateMotorImage(int motorIndex, double currentAngle)
        {

            Dispatcher.Invoke(() =>
            {
                Border border = imageContainer.Children[motorIndex] as Border;
                StackPanel stackPanel = border.Child as StackPanel;
                StackPanel iStackPanel = stackPanel.Children[1] as StackPanel;
                Image image = iStackPanel.Children[1] as Image;

                if (image != null && image.RenderTransform is RotateTransform rotateTransform)
                {
                    var motor = _emulator.PlcMachine.Motors[motorIndex];
                    rotateTransform.Angle = currentAngle;
                    textBoxImageData.Text = ("Rotated motor " + (motorIndex + 1) + ": " + (int)currentAngle + "°");
                    //textBoxImageData.Text = ("LoByte: " + motor.LoBytePos + " | HiByte: " + motor.HiBytePos);
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

            foreach (var motor in _emulator.PlcMachine.Motors)
            {
                motor.MotorInProgress = false;
            }
        }

        private void CreateMotorImages()
        {
            Dispatcher.Invoke(() => 
            {
                while (imageContainer.Children.Count > 0)
                {
                    var child = imageContainer.Children[0];
                    imageContainer.Children.Remove(child);
                }

                imageContainer.Columns = imageContainer.Rows = (int)Math.Sqrt(GlobalSettings.NumberOfMotors);
            });

            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
            {
                var motorImage = new BitmapImage(new Uri("pack://application:,,,/arrow-right.png"));

                var image = new Image();

                var motorViewModel = _emulator.PlcMachine.Motors[i];

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
                    mTextBlock.FontWeight = FontWeights.Bold;
                    mTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    mTextBlock.VerticalAlignment = VerticalAlignment.Top;
                    mTextBlock.Height = 20;
                };

                var iStackPanel = GuiCreators.CreateInfoText("OperationalSpeed", "AbsolutePosition", motorViewModel);

                var motorInProgressStackPanel = GuiCreators.CreateIndicator("MotorInProgress", motorViewModel);
                var motorInMaxStackPanel = GuiCreators.CreateIndicator("InMaxPosition", motorViewModel);
                var motorIsHomedStackPanel = GuiCreators.CreateIndicator("MotorIsHomed", motorViewModel);
                var motorInCenterStackPanel = GuiCreators.CreateIndicator("InCenteredPosition", motorViewModel);
                var motorInHomeStackPanel = GuiCreators.CreateIndicator("InHomePosition", motorViewModel);
                var motorHasErrorStackPanel = GuiCreators.CreateIndicator("Error", motorViewModel);

                StackPanel statusStackPanel = new StackPanel();
                {
                    statusStackPanel.Orientation = Orientation.Vertical;
                    statusStackPanel.Margin = new Thickness(0, 0, 10, 0);
                    statusStackPanel.Children.Add(motorInProgressStackPanel);
                    statusStackPanel.Children.Add(motorInMaxStackPanel);
                    statusStackPanel.Children.Add(motorIsHomedStackPanel);
                    statusStackPanel.Children.Add(motorInCenterStackPanel);
                    statusStackPanel.Children.Add(motorInHomeStackPanel);
                    statusStackPanel.Children.Add(motorHasErrorStackPanel);
                };

                StackPanel horizontalStackPanel = new StackPanel();
                {
                    horizontalStackPanel.Orientation = Orientation.Horizontal;
                    horizontalStackPanel.Children.Add(statusStackPanel);
                    horizontalStackPanel.Children.Add(image);
                };

                StackPanel verticalStackPanel = new StackPanel();
                {
                    verticalStackPanel.Margin = new Thickness(2);
                    verticalStackPanel.Orientation = Orientation.Vertical;
                    verticalStackPanel.Children.Add(mTextBlock);
                    verticalStackPanel.Children.Add(horizontalStackPanel);
                    verticalStackPanel.Children.Add(iStackPanel);
                };

                Border border = new Border();
                {
                    border.BorderThickness = new Thickness(1);
                    border.BorderBrush = Brushes.Gray;
                    border.Margin = new Thickness(2);
                    border.Child = verticalStackPanel;
                };

                RotateTransform rotateTransform = new RotateTransform(0);
                image.RenderTransform = rotateTransform;

                Dispatcher.Invoke(() =>
                {
                    imageContainer.Children.Add(border);
                });
            }
        }
    }
}