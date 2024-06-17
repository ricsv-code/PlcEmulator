using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using PlcEmulator;

namespace Utilities
{
    public class IntToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return false;
                int intValue = (int)value;
                int targetValue = int.Parse(parameter.ToString());

                return intValue == targetValue;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 4; //default
            bool boolValue = (bool)value;
            int targetValue = int.Parse(parameter.ToString());

            return boolValue ? targetValue : Binding.DoNothing;
        }
    }
}