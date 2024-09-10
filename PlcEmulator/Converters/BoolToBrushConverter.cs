using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Utilities
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    if (parameter is string colorParam)
                    {
                        var colors = colorParam.Split(';');
                        Brush trueColor = typeof(Brushes).GetProperty(colors[0])?.GetValue(null) as Brush;
                        Brush falseColor = typeof(Brushes).GetProperty(colors[1])?.GetValue(null) as Brush;

                        if (trueColor == null)
                            trueColor = (Brush)System.Windows.Application.Current.FindResource(colors[0]);

                        if (falseColor == null)
                            falseColor = (Brush)System.Windows.Application.Current.FindResource(colors[1]);

                        return boolValue ? trueColor : falseColor;
                    }

                    return boolValue ? Brushes.Green : Brushes.Red;
                }

                return Brushes.Transparent;
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Brushes.Transparent;
            bool boolValue = (bool)value;
            int targetValue = int.Parse(parameter.ToString());

            return boolValue ? targetValue : Binding.DoNothing;
        }
    }
}