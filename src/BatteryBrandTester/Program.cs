using BatteryBrandTester;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                Pi.Init<BootstrapWiringPi>();

                services.AddHostedService<SensorInputService>();
                services.AddHostedService<ManagerService>();
                services.AddHostedService<RotationService>();
                services.AddHostedService<TimerService>();
                services.AddHostedService<ScreenService>();
                services.AddHostedService<DataRecorderService>();
            });
    }
}