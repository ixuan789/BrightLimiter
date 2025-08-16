using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WmiLight;

namespace BrightLimiter;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public class MainWorker(byte minBrightness, byte maxBrightness) : BackgroundService
{
    private static readonly ILoggerFactory LoggerFactory = 
        Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
    private readonly ILogger _logger = LoggerFactory.CreateLogger<MainWorker>();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Brightness Limiter Service is starting...");
        try
        {
            using var watcher = BrightnessWmiLightHelper.GetWatcher();
            watcher.EventArrived += WmiEventHandler;
            watcher.Start();
            _logger.LogInformation("Brightness Limiter Service is running.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogCritical($"An unhandled error occurred: {ex}");
        }
    }

    private async void WmiEventHandler(object sender, WmiEventArrivedEventArgs wmiEventArrivedEventArgs)
    {
        try
        {
            var currentBrightness = BrightnessWmiLightHelper.GetBrightnessLevel();
            // Same condition
            if (minBrightness == maxBrightness)
            {
                if (currentBrightness != minBrightness)
                    BrightnessWmiLightHelper.SetBrightnessLevel(minBrightness);
                return;
            }

            if (currentBrightness < minBrightness)
            {
                BrightnessWmiLightHelper.SetBrightnessLevel(minBrightness);
                // await Task.Delay(200);
                // var brightnessAfterDelay = BrightnessWmiLightHelper.GetBrightnessLevel();
                // if (brightnessAfterDelay < MinBrightness)
                //     BrightnessWmiLightHelper.SetBrightnessLevel(MinBrightness);

            }
            else if (currentBrightness > maxBrightness)
            {
                BrightnessWmiLightHelper.SetBrightnessLevel(maxBrightness);
                // await Task.Delay(200);
                // var brightnessAfterDelay = BrightnessWmiLightHelper.GetBrightnessLevel();
                // if (brightnessAfterDelay > MaxBrightness)
                //     BrightnessWmiLightHelper.SetBrightnessLevel(MaxBrightness);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Exception: {e}");
        }
    }
}