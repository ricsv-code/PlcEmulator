﻿using System.Diagnostics;
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
using System.Windows.Controls;

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

                textBoxOperation.Text = "PLC Emulator started..\r\n";
            }
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (_emulator != null)
            {
                _stopwatch.Stop();
                _emulator.Stop();
                _isRunning = false;

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
            int presentedDegrees = Convert.ToInt32(angleDegrees);

        Dispatcher.Invoke(() =>
            {
            StackPanel stackPanel = imageContainer.Children[motorIndex] as StackPanel;
            System.Windows.Controls.Image image = stackPanel.Children[1] as System.Windows.Controls.Image;

            if (image != null && image.RenderTransform is RotateTransform rotateTransform) 
            {
                rotateTransform.Angle = angleDegrees;
                textBoxImageData.Text = ("Rotated motor " + (motorIndex + 1) + ": " + presentedDegrees + "°");
            }
            });

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
            }); //rensar alla barn innan repopulering

            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
            {
                var motorImage = new BitmapImage(new Uri("C:\\Users\\risve\\source\\repos\\PlcEmulator\\PlcEmulator\\arrow-right.png"));

                var image = new System.Windows.Controls.Image();

                image.Source = motorImage;
                {
                    image.Width = 240;
                    image.Height = 240;
                    image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                };

                TextBlock textBlock = new TextBlock();
                {
                    textBlock.Text = $"Motor: {i + 1}";
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    textBlock.Height = 20;
                };

                StackPanel stackPanel = new StackPanel();
                {
                    stackPanel.Children.Add(textBlock);
                    stackPanel.Children.Add(image);
                }

                RotateTransform rotateTransform = new RotateTransform(0);
                image.RenderTransform = rotateTransform;

                Dispatcher.Invoke(() =>
                {
                    imageContainer.Children.Add(stackPanel);
                });

            }
        }
    }
}