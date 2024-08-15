
namespace Utilities
{
    public static class GlobalSettings
    {
        private static int _numberOfMotors = 4; //default

        public static event EventHandler NumberOfMotorsChanged;

        public static int NumberOfMotors
        {
            get { return _numberOfMotors; }
            set
            {
                _numberOfMotors = value;

                NumberOfMotorsChanged?.Invoke(null, EventArgs.Empty);
            }
        }

    }
}