using System.Runtime.InteropServices.ObjectiveC;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Reflection;
using static Utilities.GuiCreators;
using System.Numerics;

namespace Utilities
{
    public static class GuiCreators
    {
        public static StackPanel CreateIndicator(string indicatorName, object source)
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

        public static StackPanel CreateInfoText(string speed, string position, object source)
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
                Margin = new System.Windows.Thickness(10, 0, 0, 0)
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

        public static Binding CreateBinding(string property, object source)
        {
            Binding binding = new Binding(property)
            {
                Source = source,
            };
            return binding;
        }

        #region Filters

        public class KalmanFilter
        {

            // TODO: Implement an Extended Kalman Filter for non-linear noise filtering.

            private double _estimate;
            private double _errorEstimate;
            private double _processNoise;
            private double _measurementNoise;

            /// <summary>
            /// Initializes a new simple Kalman filter used for filtering linear data
            /// </summary>
            /// <param name="initialEstimate">Estimated first value</param>
            /// <param name="initialErrorEstimate">How far off the measured value is expected to be</param>
            /// <param name="processNoise">How much noise is expected from the system (higher value = more noise)</param>
            /// <param name="measurementNoise">How much noise is expected from measurement sensors (higher value = more noise)</param>
            public KalmanFilter(double initialEstimate, double initialErrorEstimate, double processNoise, double measurementNoise)
            {
                _estimate = initialEstimate;
                _errorEstimate = initialErrorEstimate;
                _processNoise = processNoise;
                _measurementNoise = measurementNoise;

            }


            /// <summary>
            /// Updates the filter with a new value and returns a filter-adjusted value
            /// </summary>
            /// <param name="value">The new measured value</param>
            /// <param name="predictedValue">The value the filters algoritm predicts before taking the new value into account</param>
            public double Update(double value, out double predictedValue)
            {
                _errorEstimate += _processNoise;
                predictedValue = _errorEstimate;

                double kalmanGain = _errorEstimate / (_errorEstimate + _measurementNoise);

                _estimate += kalmanGain * (value - _estimate);
                _errorEstimate *= (1 - kalmanGain);

                return _estimate;
            }
        }

        public class ExtendedKalmanFilter
        {
            private double _estimate;
            private double _errorEstimate;
            private double _processNoise;
            private double _measurementNoise;


            /// <summary>
            /// Initializes a new Extended Kalman filter used for filtering non-linear data. 
            /// </summary>
            /// <param name="initialEstimate">Estimated first value</param>
            /// <param name="initialErrorEstimate">How far off the measured value is expected to be</param>
            /// <param name="processNoise">How much noise is expected from the system (higher value = more noise)</param>
            /// <param name="measurementNoise">How much noise is expected from measurement sensors (higher value = more noise)</param>
            public ExtendedKalmanFilter(double initialEstimate, double initialErrorEstimate, double processNoise, double measurementNoise)
            {
                _estimate = initialEstimate;
                _errorEstimate = initialErrorEstimate;
                _processNoise = processNoise;
                _measurementNoise = measurementNoise;
            }

            /// <summary>
            /// A function representing the model. Default: linear
            /// </summary>
            public Func<double, double> ProcessModel { get; set; } = (state) => (state);

            /// <summary>
            /// The derivative of ProcessModel. Default: 1
            /// </summary>
            public Func<double, double> ProcessModelDerivative { get; set; } = (state) => 1;

            /// <summary>
            /// A function representing the measurement model. Default: linear
            /// </summary>
            public Func<double, double> MeasurementModel { get; set; } = (state) => (state);

            /// <summary>
            /// The derivative of MeasurementModel. Default: 1
            /// </summary>
            public Func<double, double> MeasurementModelDerivative { get; set; } = (state) => 1;


            /// <summary>
            /// Updates the filter with a new value and returns a filter-adjusted value. ProcessModel and ProcessModelDerivative must be set before calling this method.
            /// </summary>
            /// <param name="value">The new measured value</param>
            /// <param name="predictedValue">The value the filters algoritm predicts before taking the new value into account</param>
            public double Update(double value, out double predictedValue)
            {

                predictedValue = ProcessModel(_estimate); //gissningen
                double derivativeProcessModel = ProcessModelDerivative(_estimate);
                double predictedErrorEstimate = derivativeProcessModel * _errorEstimate * derivativeProcessModel + _processNoise; //möjligt fel i gissningen


                double derivativeMeasurementModel = MeasurementModelDerivative(predictedValue);

                double kalmanGain = (predictedErrorEstimate * derivativeMeasurementModel) /
                                    (derivativeMeasurementModel * predictedErrorEstimate * derivativeMeasurementModel + _measurementNoise);
                //taylorutveckling för att linjärisera modellen i denna punkt, antagande om normalfördelning samma som i det enkla filtret


                _estimate = predictedValue + kalmanGain * (value - MeasurementModel(predictedValue));
                _errorEstimate = (1 - kalmanGain * derivativeMeasurementModel) * predictedErrorEstimate;

                return _estimate;
            }

            public class UnscentedKalmanFilter
            {
                private double _estimate;
                private double _errorEstimate;
                private double _processNoise;
                private double _measurementNoise;


                /// <summary>
                /// Initializes a new Unscented Kalman filter used for filtering non-linear data. 
                /// </summary>
                /// <param name="initialEstimate">Estimated first value</param>
                /// <param name="initialErrorEstimate">How far off the measured value is expected to be</param>
                /// <param name="processNoise">How much noise is expected from the system (higher value = more noise)</param>
                /// <param name="measurementNoise">How much noise is expected from measurement sensors (higher value = more noise)</param>
                public UnscentedKalmanFilter(double initialEstimate, double initialErrorEstimate, double processNoise, double measurementNoise)
                {
                    _estimate = initialEstimate;
                    _errorEstimate = initialErrorEstimate;
                    _processNoise = processNoise;
                    _measurementNoise = measurementNoise;
                }

                /// <summary>
                /// A function representing the model. Default: linear
                /// </summary>
                public Func<double, double> ProcessModel { get; set; } = (state) => (state);


                /// <summary>
                /// A function representing the measurement model. Default: linear
                /// </summary>
                public Func<double, double> MeasurementModel { get; set; } = (state) => (state);




                /// <summary>
                /// Updates the filter with a new value and returns a filter-adjusted value. ProcessModel and ProcessModelDerivative must be set before calling this method.
                /// </summary>
                /// <param name="value">The new measured value</param>
                /// <param name="predictedValue">The value the filters algoritm predicts before taking the new value into account</param>
                public double Update(double value, out double predictedValue)
                {
                    Vector3D sigmaPoints = new Vector3D(_estimate, _estimate + System.Math.Sqrt(_errorEstimate), _estimate - System.Math.Sqrt(_errorEstimate));

                    Vector3D predictedSigmaPoints = new Vector3D(
                        ProcessModel(sigmaPoints.X),
                        ProcessModel(sigmaPoints.Y),
                        ProcessModel(sigmaPoints.Z));

                    double predictedMean = (predictedSigmaPoints.X + predictedSigmaPoints.Y + predictedSigmaPoints.Z) / 3;
                    double predictedCovariance = ((
                        System.Math.Pow(predictedSigmaPoints.X - predictedMean, 2) +
                        System.Math.Pow(predictedSigmaPoints.Y - predictedMean, 2) +
                        System.Math.Pow(predictedSigmaPoints.Z - predictedMean, 2)) / 3) + _processNoise;

                    Vector3D measurementSigmaPoints = new Vector3D(
                        MeasurementModel(predictedSigmaPoints.X),
                        MeasurementModel(predictedSigmaPoints.Y),
                        MeasurementModel(predictedSigmaPoints.Z));

                    double predictedMeasurementMean = (measurementSigmaPoints.X + measurementSigmaPoints.Y + measurementSigmaPoints.Z) / 3;
                    double predictedMeasurementCovariance = ((
                        System.Math.Pow(measurementSigmaPoints.X - predictedMeasurementMean, 2) +
                        System.Math.Pow(measurementSigmaPoints.Y - predictedMeasurementMean, 2) +
                        System.Math.Pow(measurementSigmaPoints.Z - predictedMeasurementMean, 2)) / 3) + _measurementNoise;

                    double crossCovariance = (
                        (predictedSigmaPoints.X - predictedMean) * (measurementSigmaPoints.X - predictedMeasurementMean) +
                        (predictedSigmaPoints.Y - predictedMean) * (measurementSigmaPoints.Y - predictedMeasurementMean) +
                        (predictedSigmaPoints.Z - predictedMean) * (measurementSigmaPoints.Z - predictedMeasurementMean)) / 3;


                    double kalmanGain = crossCovariance / predictedMeasurementCovariance;


                    _estimate = predictedMean + kalmanGain * (value - predictedMeasurementMean);
                    _errorEstimate = predictedCovariance - kalmanGain * crossCovariance * kalmanGain;

                    predictedValue = predictedMean;

                    return _estimate;
                }
            }

            #endregion

        }
    }
}
