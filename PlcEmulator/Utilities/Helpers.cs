
using System.Runtime.CompilerServices;

namespace Utilities
{
    public static class Helpers
    {
        public static double RadiansToDegrees(int radians)
        {
            decimal angleRadians = radians / 1000.0m;
            double angleDegrees = (double)(angleRadians * (180m / (decimal)Math.PI));
            return angleDegrees;
        }

        public static double DegreesToRadians(double degrees)
        {
            double radians = degrees * (Math.PI / 180);
            double output = radians * 1000;
            return output;
        }

        public static byte CalculateChecksum(byte[] data)
        {
            return (byte)data.Take(9).Sum(b => b);
        }

        public static void LogSentData(EventHandler<string> updateSentData, byte[] response, string opCode)
        {
            response[9] = Helpers.CalculateChecksum(response);
            string sentData = BitConverter.ToString(response);
            updateSentData?.Invoke(null, $"Sent {opCode} response: {sentData}");
        }
    }
}