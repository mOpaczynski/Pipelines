using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

using static System.FormattableString;

namespace CakeExtensions
{
    public static class CakeExtensions
    {
        [CakeMethodAlias]
        public static void MigrateDatabases(this ICakeContext context, string projectConfiguration = "Release", string migrationConfiguration = "Configuration", int? targetMigration = null)
        {
            context.Log.Information("hello");
            var root = GetRootPath();
            context.Log.Information(Invariant($"root: {root.FullName}"));

            context.Log.Information(Invariant($"searching for migrate.exe"));
            if (!Directory.Exists(root.FullName))
            {
                context.Log.Information(Invariant($"root directory not exists"));
            }
            var migrateExecFile = Directory.GetFiles(Invariant($"{root.FullName}"), "migrate.exe", SearchOption.AllDirectories).First();
            context.Log.Information($"migrateExecFile: {migrateExecFile}");
            var dataAccessDirectories = Directory.GetDirectories(root.FullName, "*.DataAccess", SearchOption.AllDirectories);
            context.Log.Information($"dataAccessDirectories: {dataAccessDirectories}");

            foreach (var dataAccessDirectory in dataAccessDirectories)
            {
                context.Log.Information($"Data acc dir: {dataAccessDirectory}");
                var configFilePath = Directory.GetFiles(dataAccessDirectory, "App.config", SearchOption.TopDirectoryOnly).First();
                context.Log.Information($"configFilePath: {configFilePath}");
                var assembyFilePath = Directory.GetFiles(dataAccessDirectory, $"{GetLastSegment(dataAccessDirectory)}.dll", SearchOption.AllDirectories).First(x => x.Contains("bin") && x.Contains($"{projectConfiguration}"));
                context.Log.Information($"assembyFilePath: {assembyFilePath}");
                var migrateExecFileCopyPath = $"{Path.GetDirectoryName(assembyFilePath)}/migrate.exe";
                context.Log.Information($"migrateExecFileCopyPath: {migrateExecFileCopyPath}");
                var arguments = $"{GetLastSegment(dataAccessDirectory)}.dll {migrationConfiguration} /startupConfigurationFile:\"{configFilePath}\" /targetMigration:\"{targetMigration}\" /verbose";
                context.Log.Information($"arguments: {arguments}");

                File.Copy(migrateExecFile, migrateExecFileCopyPath, true);

                Process runMigrator = new Process();
                runMigrator.StartInfo.FileName = migrateExecFileCopyPath;
                runMigrator.StartInfo.Arguments = arguments;
                runMigrator.StartInfo.UseShellExecute = false;
                runMigrator.StartInfo.RedirectStandardOutput = true;
                runMigrator.StartInfo.RedirectStandardError = true;
                runMigrator.OutputDataReceived += (s, e) => context.Log.Information(e.Data);
                runMigrator.ErrorDataReceived += (s, e) => context.Log.Information(e.Data);

                runMigrator.Start();
                runMigrator.BeginOutputReadLine();
                runMigrator.BeginErrorReadLine();
                runMigrator.WaitForExit();

                if (runMigrator.ExitCode != 0)
                {
                    throw new Exception($"Migrate.exe: Process returned an error (exit code {runMigrator.ExitCode}).");
                }
            }
        }

        private static DirectoryInfo GetRootPath()
        {
            DirectoryInfo currentWorkingDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            return currentWorkingDir.Parent.Parent;
        }

        private static string GetLastSegment(string path)
        {
            DirectoryInfo uri = new DirectoryInfo(path);

            return uri.Name;
        }
    }
}
