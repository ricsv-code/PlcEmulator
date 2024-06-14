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
using System.Windows.Media;
using Utilities;

namespace PlcEmulator
{

    public partial class PlcSimGui : Window
    {
        private EmulatorPlc _emulator;
        private Stopwatch _stopwatch;
        private bool _isRunning;
        private Timer _positionUpdateTimer;

        public PlcSimGui()
        {
            InitializeComponent();
            CreateMotorImages();
            motorImage.Source = new BitmapImage(new Uri("C:\\Users\\risve\\source\\repos\\PlcEmulator\\PlcEmulator\\arrow-right.png"));
        }
        
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning == false)
            {
                _stopwatch = new Stopwatch();
                _emulator = new EmulatorPlc("127.0.0.1", 502, UpdateReceivedData, UpdateSentData, UpdateOperation, UpdateImage);

                _stopwatch.Start();
                _emulator.Start();
                _isRunning = true;
            }
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (_emulator != null)
            {
                _stopwatch.Stop();
                _emulator.Stop();
                textBoxOperation.Text = "PLC Emulator stopped..\r\n";
                _isRunning = false;
            }
        }

        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxSentData.Text = string.Empty;
            textBoxReceivedData.Text = string.Empty;
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

        private void UpdateImage(byte[] request)
        {
            int position = (request[2] << 8) | request[3];
            decimal angleRadians = position / 1000.0m;
            double angleDegrees = (double)(angleRadians * (180m / (decimal)Math.PI));
            int presentedDegrees = Convert.ToInt32(angleDegrees);

            Dispatcher.Invoke(() =>
            {
                rotateTransform.Angle = angleDegrees;
                textBoxImageData.Text = ("Rotated: " + presentedDegrees + "°");
            });
        }


        private void CreateMotorImages()
        {
            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
            {
                var motorImage = new BitmapImage(new Uri("C:\\Users\\risve\\source\\repos\\PlcEmulator\\PlcEmulator\\arrow-right.png"));

                var image = new System.Windows.Controls.Image();
                image.Source = motorImage;

                image.Width = 240;
                image.Height = 240;

                RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                RotateTransform rotateTransform = new RotateTransform(0);
                image.RenderTransform = rotateTransform;

                Dispatcher.Invoke(() =>
                {
                    imageContainer.Children.Add(image);
                });

            }
        }
    }
}