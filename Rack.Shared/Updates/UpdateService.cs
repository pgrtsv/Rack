using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Rack.Shared.Updates
{
    public sealed class UpdateService : IUpdateService
    {
        public UpdateService(IConfiguration configuration)
        {
            UpdatePath = configuration["UpdatePath"];
            var canCheckForUpdates = false;
            if (!string.IsNullOrWhiteSpace(UpdatePath))
                canCheckForUpdates = Directory.Exists(UpdatePath);
            CanCheckForUpdates = Observable.Return(canCheckForUpdates);
        }

        public string UpdatePath { get; }

        public string UpdateExePath { get; } = Path.Combine(
            Directory.GetParent(Assembly.GetEntryAssembly().Location).Parent.FullName,
            "Update.exe");

        public IObservable<bool> CanCheckForUpdates { get; }

        public IObservable<bool> CheckForUpdates() => Observable.Start(() =>
        {
            var checkForUpdateProcessInfo = new ProcessStartInfo(
                UpdateExePath,
                $"--checkForUpdate={UpdatePath}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var updateProcess = Process.Start(checkForUpdateProcessInfo);
            var output = string.Empty;
            while (!updateProcess.StandardOutput.EndOfStream)
            {
                output = updateProcess.StandardOutput.ReadLine();
                if (int.TryParse(output, out _))
                    continue;
                break;
            }

            dynamic updateInfo = JObject.Parse(output);

            return (bool) (updateInfo.releasesToApply.Count != 0);
        });

        public IObservable<Unit> Update(IProgress<int> progress) => Observable.Start(() =>
        {
            progress.Report(0);
            var updateProcessInfo = new ProcessStartInfo(
                UpdateExePath,
                $"--update={UpdatePath}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var updateProcess = Process.Start(updateProcessInfo);

            while (!updateProcess.StandardOutput.EndOfStream)
                if (int.TryParse(updateProcess.StandardOutput.ReadLine(), out var result))
                    progress.Report(result);

            updateProcess.WaitForExit();
        });
    }
}