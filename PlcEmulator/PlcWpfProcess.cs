using PlcEmulatorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using PlcEmulator;
using Utilities;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Threading;
using System.Windows.Input;

namespace PlcEmulatorCore
{



    public class PlcWpfProcess : ViewModelBase
    {
        #region Fields
        private Stopwatch _stopwatch;
        private bool _isRunning;
        private Dictionary<int, System.Timers.Timer> _motorTimers = new Dictionary<int, System.Timers.Timer>();
        private bool DarkModeOn;

        private PlcEmulatorGui _gui;
        private PlcProcess _process;
        private Commands _commands;
        private InfoText _infoText;

        #endregion

        #region Constructors

        public PlcWpfProcess(string modelType)
        {



            _process = new PlcProcess();
            _gui = new PlcEmulatorGui(this);

            _stopwatch = new Stopwatch();
            _commands = new Commands();
            _infoText = new InfoText();
            _isRunning = false;

            StartCommand = new RelayCommand(Start);
            StopCommand = new RelayCommand(Stop);
            CleanCommand = new RelayCommand(Clean);
            RunScriptsCommand = new RelayCommand(RunScripts);
            NumberOfMotorsCommand = new RelayCommand(NumberOfMotorsChange);
            SetMotorPropertiesCommand = new RelayCommand(SetMotorProperties);
            DarkModeCommand = new RelayCommand(DarkMode);
            StandardCommand = new RelayCommand(StandardMode);

            SendSomeErrorsCommand = new RelayCommand(SendSomeErrors);

            UpdateMenuItems();

            _gui.ButtonStop.IsEnabled = false;

            CreateMotorImages();

            _process.ReceivedData -= (sender, e) => UpdateReceivedData(e);
            _process.ReceivedData += (sender, e) => UpdateReceivedData(e);

            _process.ReceivedOpData -= (sender, e) => UpdateOperation(e);
            _process.ReceivedOpData += (sender, e) => UpdateOperation(e);

            _process.ShowStopper -= (sender, e) => ShowStopper();
            _process.ShowStopper += (sender, e) => ShowStopper();

            _process.UpdateImage -= (sender, e) => UpdateImage(e);
            _process.UpdateImage += (sender, e) => UpdateImage(e);

            _process.SentData -= (sender, e) => UpdateSentData(e);
            _process.SentData += (sender, e) => UpdateSentData(e);


            _gui.Show();

        }



        #endregion

        #region Properties

        public ICommand StartCommand
        {
            get => _commands.StartCommand;
            set => SetProperty(ref _commands.StartCommand, value);
        }
        public ICommand StopCommand
        {
            get => _commands.StopCommand;
            set => SetProperty(ref _commands.StopCommand, value);
        }
        public ICommand DarkModeCommand
        {
            get => _commands.DarkModeCommand;
            set => SetProperty(ref _commands.DarkModeCommand, value);
        }
        public ICommand StandardCommand
        {
            get => _commands.StandardCommand;
            set => SetProperty(ref _commands.StandardCommand, value);
        }
        public ICommand SetMotorPropertiesCommand
        {
            get => _commands.SetMotorPropertiesCommand;
            set => SetProperty(ref _commands.SetMotorPropertiesCommand, value);
        }
        public ICommand RunScriptsCommand
        {
            get => _commands.RunScriptsCommand;
            set => SetProperty(ref _commands.RunScriptsCommand, value);
        }
        public ICommand CleanCommand
        {
            get => _commands.CleanCommand;
            set => SetProperty(ref _commands.CleanCommand, value);
        }
        public ICommand NumberOfMotorsCommand
        {
            get => _commands.NumberOfMotorsCommand;
            set => SetProperty(ref _commands.NumberOfMotorsCommand, value);
        }

        public ICommand SendSomeErrorsCommand
        {
            get => _commands.SendSomeErrorsCommand;
            set => SetProperty(ref _commands.SendSomeErrorsCommand, value);
        }

        public string TextBoxImageData
        {
            get => _infoText.TextBoxImageData;
            set => SetProperty(ref _infoText.TextBoxImageData, value);
        }
        public string TextBoxOperation
        {
            get => _infoText.TextBoxOperation;
            set => SetProperty(ref _infoText.TextBoxOperation, value);
        }
        public string TextBoxReceivedData
        {
            get => _infoText.TextBoxReceivedData.ToString();
            set
            {
                _infoText.TextBoxReceivedData.Append(value);

                if (_infoText.TextBoxReceivedData.Length > 5000)
                {
                    _infoText.TextBoxReceivedData.Remove(0, _infoText.TextBoxReceivedData.Length - 5000);
                }


                OnPropertyChanged(nameof(TextBoxReceivedData));
            }
        }

        public string TextBoxSentData
        {
            get => _infoText.TextBoxSentData.ToString();
            set
            {
                _infoText.TextBoxSentData.Append(value);

                if (_infoText.TextBoxSentData.Length > 5000)
                {
                    _infoText.TextBoxSentData.Remove(0, _infoText.TextBoxSentData.Length - 5000);
                }

                OnPropertyChanged(nameof(TextBoxSentData));
            }
        }

        #endregion

        #region Methods

        private void SendSomeErrors(object parameter)
        {
            bool? isChecked = parameter as bool?;
            if (isChecked.HasValue && isChecked.Value)
            {
                _process.SendSomeErrors = true;
            }
            else
            {
                _process.SendSomeErrors = false;
            }
        }

        private void DarkMode(object sender)
        {
            ResourceDictionary theme = new ResourceDictionary();
            theme.Source = new Uri("Resources/DarkTheme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(theme);

            DarkModeOn = true;
            UpdateMenuItems();
        }

        private void StandardMode(object sender)
        {
            ResourceDictionary theme = new ResourceDictionary();
            theme.Source = new Uri("Resources/StandardTheme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(theme);

            DarkModeOn = false;
            UpdateMenuItems();
        }

        private void Start(object sender)
        {
            try
            {
                if (_isRunning == false)
                {
                    string ipAddress = _gui.IpAddressTextBox.Text;
                    int port = int.Parse(_gui.PortTextBox.Text);


                    _stopwatch.Restart();

                    _process.Start(ipAddress, port);



                    _isRunning = true;

                    _gui.ButtonStart.IsEnabled = false;
                    _gui.ButtonStop.IsEnabled = true;
                    _gui.ConnectionIndicator.Fill = Brushes.Green;

                    TextBoxOperation = $"PLC Emulator started..\r\n";
                }
            }
            catch (Exception ex)
            {
                TextBoxReceivedData = ($"Error: {ex.Message}$\r\n");
            }
        }

        private void Stop(object sender)
        {
            try
            {
                if (_process != null)
                {
                    _stopwatch.Stop();
                    _process.Stop();
                    _isRunning = false;
                    _gui.ButtonStart.IsEnabled = true;
                    _gui.ButtonStop.IsEnabled = false;

                    _gui.ConnectionIndicator.Fill = Brushes.Red;

                    TextBoxOperation = "PLC Emulator stopped..\r\n";
                }
            }
            catch (Exception ex)
            {
                TextBoxReceivedData += ($"Error: {ex.Message}$\r\n");
            }
        }

        private void Clean(object sender)
        {
            
            _infoText.TextBoxSentData.Clear();
            TextBoxSentData = string.Empty;

            _infoText.TextBoxReceivedData.Clear();
            TextBoxReceivedData = string.Empty;
        }

        private void NumberOfMotorsChange(object param)
        {

            if (int.TryParse(param.ToString(), out int numberOfMotors))
            {
                if (GlobalSettings.NumberOfMotors != numberOfMotors)
                {
                    GlobalSettings.NumberOfMotors = numberOfMotors;

                    CreateMotorImages();
                }
            }

            UpdateMenuItems();
        }

        private void UpdateMenuItems()
        {
            _gui.Menu4.IsChecked = GlobalSettings.NumberOfMotors == 4;
            _gui.Menu9.IsChecked = GlobalSettings.NumberOfMotors == 9;
            _gui.DarkMode.IsChecked = DarkModeOn;
            _gui.StandardMode.IsChecked = !DarkModeOn;
        }

        private void RunScripts(object sender)
        {
            ScriptDialog dialog = new ScriptDialog();

            if (dialog.ShowDialog() == true)
            {
                _process.RunScript(dialog.Scripts);
            }
        }

        private void SetMotorProperties(object sender)
        {
            byte motorIndex = byte.Parse(_gui.SetMotorIndexTextBox.Text);
            int homePosition = int.Parse(_gui.HomePositionTextBox.Text);
            int centerPosition = int.Parse(_gui.CenterPositionTextBox.Text);
            int minPosition = int.Parse(_gui.MinPositionTextBox.Text);
            int maxPosition = int.Parse(_gui.MaxPositionTextBox.Text);

            var motor = _process.PlcMachine.Motors[motorIndex - 1];

            motor.HomePosition = homePosition;
            motor.CenterPosition = centerPosition;
            motor.MinPosition = minPosition;
            motor.MaxPosition = maxPosition;

            motor.UpdateIndicators();
        }

        private void UpdateReceivedData(string data)
        {

            TextBoxReceivedData = (string.Format("{0:00}:{1:00}:{2:000}", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds) + " | " + data + "\r\n");

        }

        private void UpdateSentData(string data)
        {

            TextBoxSentData = (string.Format("{0:00}:{1:00}:{2:000}", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds) + " | " + data + "\r\n");

        }

        private void UpdateOperation(string data)
        {
            TextBoxOperation = data;
        }

        private void UpdateImage(int motorIndex)
        {

            var motor = _process.PlcMachine.Motors[motorIndex];

            if (motor != null)
            {
                motor.UpdateIndicators();

                if (motor.AbsolutePosition != motor.TargetPosition)
                {

                    int speed = (byte)motor.OperationalSpeed;

                    if (!_motorTimers.ContainsKey(motorIndex))
                    {
                        var timer = new System.Timers.Timer();
                        timer.Interval = 10;
                        timer.Elapsed += (sender, e) => RotateMotor(motorIndex);
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
        }

        private void RotateMotor(int motorIndex)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var motor = _process.PlcMachine.Motors[motorIndex];
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

                    motor.RotationAngle = currentAngle;
                    TextBoxImageData = ("Rotated motor " + (motorIndex + 1) + ": " + (int)currentAngle + "°");
                    //textBoxImageData.Text = ("LoByte: " + motor.LoBytePos + " | HiByte: " + motor.HiBytePos);

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

            });
        }


        private void ShowStopper()
        {
            foreach (var timer in _motorTimers.Values)
            {
                if (timer.Enabled)
                {
                    timer.Stop();
                }
            }

            foreach (var motor in _process.PlcMachine.Motors)
            {
                motor.MotorInProgress = false;
                motor.UpdateIndicators();
            }
        }



        private void CreateMotorImages()
        {
            _gui.MotorGrid.Children.Clear();
            _gui.MotorGrid.Columns = _process.PlcMachine.MotorGrid.Rows = (int)Math.Sqrt(GlobalSettings.NumberOfMotors);

            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
            {
                var motorImage = new BitmapImage(new Uri("pack://application:,,,/Resources/arrow-right.png"));

                var image = new Image();

                var motorViewModel = _process.PlcMachine.Motors[i];

                RotateTransform rotateTransform = new RotateTransform(0);

                BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, GuiCreators.CreateBinding("RotationAngle", motorViewModel));

                image.Source = motorImage;
                {
                    image.Width = 100;
                    image.Height = 100;
                    image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    image.RenderTransform = rotateTransform;
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
                var motorInMinStackPanel = GuiCreators.CreateIndicator("InMinPosition", motorViewModel);
                var motorInHomeStackPanel = GuiCreators.CreateIndicator("InHomePosition", motorViewModel);
                var motorHasErrorStackPanel = GuiCreators.CreateIndicator("Error", motorViewModel);

                StackPanel statusStackPanel = new StackPanel();
                {
                    statusStackPanel.Orientation = Orientation.Vertical;
                    statusStackPanel.Margin = new Thickness(0, 0, 10, 0);
                    statusStackPanel.Children.Add(motorInProgressStackPanel);
                    statusStackPanel.Children.Add(motorIsHomedStackPanel);
                    statusStackPanel.Children.Add(motorInCenterStackPanel);
                    statusStackPanel.Children.Add(motorInMaxStackPanel);
                    statusStackPanel.Children.Add(motorInMinStackPanel);
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

                motorViewModel.UpdateIndicators();

                _gui.MotorGrid.Children.Add(border);

            }
        }

        #endregion
    }
}