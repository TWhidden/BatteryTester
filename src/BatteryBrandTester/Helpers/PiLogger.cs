using System.Runtime.CompilerServices;
using OledI2cCore;

namespace BatteryBrandTester.Helpers;

internal class PiLogger : IOledLogger
{
    public void Info(string logMessage, [CallerMemberName] string caller = "")
    {
        Console.WriteLine($"[{caller}] {logMessage}");
    }
}