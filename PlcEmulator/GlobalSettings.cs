namespace GlobalSettings
{
    public static class GlobalSettings
    {
        private static int numberOfMotors = 4; //default

        public static int NumberOfMotors
        {
            get { return numberOfMotors; }
            set { numberOfMotors = value; }
        }
    }

}