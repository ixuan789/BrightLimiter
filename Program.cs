using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BrightLimiter;

internal class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Any(a => a.Equals("--uninstall", StringComparison.OrdinalIgnoreCase)))
        {
            UninstallService();
            return;
        }

        if (args.Length is <2 or >3 ||
            !byte.TryParse(args[0], out var min) ||
            !byte.TryParse(args[1], out var max) ||
            min < 1 || max < 1 ||
            min > 100 || max > 100 ||
            min > max)
        {
            Console.WriteLine("Invalid parameter. Expected two values (1-100).");
            Console.WriteLine("Options:");
            Console.WriteLine("  --install      Install the program as a daemon service");
            Console.WriteLine("  --uninstall    Uninstall the daemon service");
            return;
        }

        if (args.Any(a => a.Equals("--install", StringComparison.OrdinalIgnoreCase)))
        {
            InstallService(args.Where(a => !a.Equals("--install", StringComparison.OrdinalIgnoreCase)).ToArray());
            return;
        }

        
        IHost host = Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices(service =>
            {
                service.AddHostedService(_ =>
                new MainWorker(min, max));
            }).Build();
        await host.RunAsync();
    }

    private static void InstallService(string[] args)
    {
        try
        {
            var module = Process.GetCurrentProcess().MainModule;
            if (module == null) throw new NullReferenceException("Can't read process info.");
            string path = module.FileName;
            string serviceArgs = args.Length >= 2 ? $"{args[0]} {args[1]}" : "58 100";
            using (var process = new Process())
            {
                process.StartInfo.FileName = "sc";
                process.StartInfo.Arguments = $"create \"{GetServiceName()}\" binPath= \"{path} {serviceArgs}\" start= auto";
                process.StartInfo.Verb = "runas"; 
                process.StartInfo.UseShellExecute = true;
                process.Start();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Failed to install service. Error code: {process.ExitCode}");
                    return;
                }
            }

            // 启动服务
            using (var process = new Process())
            {
                process.StartInfo.FileName = "sc";
                process.StartInfo.Arguments = $"start \"{GetServiceName()}\"";
                process.StartInfo.Verb = "runas";
                process.StartInfo.UseShellExecute = true;
                process.Start();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Failed to start service. Error code: {process.ExitCode}");
                }
                else
                {
                    Console.WriteLine("Service installed and started successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error installing service: {ex.Message}");
        }
    }

    private static void UninstallService()
    {
        try
        {
            // 停止服务
            using (var process = new Process())
            {
                process.StartInfo.FileName = "sc";
                process.StartInfo.Arguments = $"stop \"{GetServiceName()}\"";
                process.StartInfo.Verb = "runas";
                process.StartInfo.UseShellExecute = true;
                process.Start();
                process.WaitForExit();
            }

            // 删除服务
            using (var process = new Process())
            {
                process.StartInfo.FileName = "sc";
                process.StartInfo.Arguments = $"delete \"{GetServiceName()}\"";
                process.StartInfo.Verb = "runas";
                process.StartInfo.UseShellExecute = true;
                process.Start();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Failed to uninstall service. Error code: {process.ExitCode}");
                }
                else
                {
                    Console.WriteLine("Service uninstalled successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uninstalling service: {ex.Message}");
        }
    }

    private static string GetServiceName()
    {
        var process = Process.GetCurrentProcess().MainModule;
        return Path.GetFileNameWithoutExtension( process == null ? "BrightLimiter" : process.FileName);
    }
}
