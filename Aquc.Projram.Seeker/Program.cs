using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Aquc.Configuration;
using Aquc.Configuration.Abstractions;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Aquc.Netdisk.Smms;
using Aquc.AquaUpdater;
using System.Reflection;
using System.Diagnostics;

namespace Aquc.Projram.Seeker;

public class Program
{
    static ILogger<Program>? _logger;
    static async Task Main(string[] args)
    {
        using var host=new HostBuilder()
            .ConfigureLogging(service =>
            {
                service.ClearProviders();
                service.AddConsole();
                service.AddFile();
            })
            .ConfigureServices(container =>
            {
                container.AddSingleton<IConfigurationSource<SeekerConfig>, ConfigurationDefaultSource<SeekerConfig>>((services) =>
                    ConfigurationBuilder<SeekerConfig>.Create()
                        .SetDefault(new SeekerConfig())
                        .BindJsonAsync(Path.Combine(AppContext.BaseDirectory, "Aquc.Projram.Seeker.config.json")).Result
                        .BuildDefault());
            })
            .Build();
        var config=host.Services.GetService<IConfigurationSource<SeekerConfig>>()!;
        _logger = host.Services.GetService<ILogger<Program>>();
        var dirArgument = new Argument<DirectoryInfo> { Arity = ArgumentArity.ExactlyOne };
        var addCommand = new Command("add")
        {
            dirArgument
        };
        var seekCommand = new Command("seek");
        var registerCommand = new Command("register");
        var root = new RootCommand()
        {
            registerCommand,
            addCommand,
            seekCommand
        };
        registerCommand.SetHandler(() =>
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks",
                    Arguments = $"/create /f /sc daily /mo 1 /tr \"'{Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory),"Aquc.Projram.Seeker.exe")}' seek\" /st 07:00 /ri 60 /du 20:00 /TN \"Aquacore\\Aquc.Projram.Seeker.SeekAction\"",
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(5000);
            _logger?.LogInformation("Success schedule aliyunpan-token-update");
            process.Dispose();
            using var flow = config.GetFlow();
            flow.Data.DirectoryInfos.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            _logger?.LogInformation("Success add desktop directory.");
            // fix
            _ = new Launch();
            SubscriptionController.RegisterSubscription(new SubscribeOption
            {
                Args = "224377738",
                Provider = "bilibilimsgpvder",
                Directory = AppContext.BaseDirectory,
                Key = "Aquc.Projram.Seeker",
                Program = Environment.ProcessPath,
                Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()
            });
            Launch.UpdateLaunchConfig();
        });
        addCommand.SetHandler(handler =>
        {
            using var flow = config.GetFlow();
            flow.Data.DirectoryInfos.Add(handler.FullName);
            _logger?.LogInformation("Success add directory: {dir}",handler.FullName);
        }, dirArgument);
        seekCommand.SetHandler(async() =>
        {
            var images = new List<FileInfo>();
            var existed = config.Data.UploaderImageName;
            foreach(var dir in config.Data.DirectoryInfos)
            {
                if(dir==null)continue;
                SeekDirectory(0, new(dir), images);
            }
            _logger?.LogInformation("Seeked {n} images.", images.Count);
            var a = images.Where((fileinfo) => { return !existed.Contains(fileinfo.Name); });
            _logger?.LogInformation("Upload {n} images.", a.Count());
            using var i = config.GetFlow();
            await foreach (var result in UploadImage(a))
            {
                if (!string.IsNullOrEmpty(result))
                {
                    _logger?.LogInformation("Success upload file: {f}", result);
                    i.Data.UploaderImageName.Add(result);
                }
                else
                {
                    _logger?.LogWarning("Failed to upload file: {f}", result);
                }
            }
        });
        await root.InvokeAsync(args);
    }
    private static async IAsyncEnumerable<string> UploadImage(IEnumerable<FileInfo> f)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "backup");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var smms = new SmmsImageHost("dSgDcaR5oKWB4n1mPHoNVZRyRTzOacoK");
        foreach(var file in f)
        {
            file.CopyTo(Path.Combine(path,$"{Path.GetFileNameWithoutExtension(file.Name)}_{DateTime.Now:yyMMddHHmmss}{file.Extension}"), true);
            if (!await smms.UploadAsync(file.FullName))
            {
                await Task.Delay(1000);
                yield return await smms.UploadAsync(file.FullName) ? file.Name : "";
            }
            else
                yield return file.Name;
        }
    }
    private static void SeekDirectory(int index,DirectoryInfo d,List<FileInfo> f)
    {
        if (index > 6) return;
        f.AddRange(d.GetFiles("*.png"));
        f.AddRange(d.GetFiles("*.jpg"));
        foreach(var dir in d.GetDirectories())
        {
            SeekDirectory(index + 1, dir, f);
        }
    }
}
public class SeekerConfig : IConfigurationStruct
{
    //public IConfigurationManifest ConfigurationManifest { get; set; } 
    public List<string> DirectoryInfos { get; set; }
    public List<string> UploaderImageName { get; set; }
    public SeekerConfig()
    {
        DirectoryInfos = new List<string>();
        UploaderImageName = new();
        //ConfigurationManifest = new ConfigurationDefaultManifest(1);
    }
}