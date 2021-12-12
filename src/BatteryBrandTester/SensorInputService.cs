using BatteryBrandTester.Events;
using PubSub;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace BatteryBrandTester;

public class SensorInputService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // logger.Info("Setting up GPIO for pin 4");
        var gpio = Pi.Gpio[BcmPin.Gpio04];
        //logger.Info("Setting pin to input mode");
        gpio.PinMode = GpioPinDriveMode.Input;
        // logger.Info("Registering falling callback");
        var count = 0;


        gpio.RegisterInterruptCallback(EdgeDetection.FallingEdge, () =>
        {
            if (gpio.Value)
            {
                // Fan blade has to blade and needs to be divided for proper RPM calculation
                var c = Interlocked.Increment(ref count);
                if (c % 2 == 0)
                    Hub.Default.Publish(new RotationEvent());
            }
        });
        return Task.CompletedTask;
    }
}