using OledI2cCore;
using Unosquare.RaspberryIO;

namespace BatteryBrandTester.Helpers;

internal class PiI2CCore : II2C
{
    public bool ReadyState => true;

    public event EventHandler<bool>? ReadyStateChanged;

    public bool SendBytes(byte[] dataBuffer, int len)
    {
        // first byte is the address
        var addr = dataBuffer[0];
        // get the device - this will add if needed
        var dev = Pi.I2C.AddDevice(addr);
        // get the register to write to
        var register = dataBuffer[1];

        // loop over the rest of the payload
        for (var i = 2; i < len; i++)
        {
            var data = dataBuffer[i];
            dev.WriteAddressByte(register, data);
        }

        return true;
    }
}