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
            var solutionRootDirectory = GetRootPath();
            var migrateExecFile = Directory.GetFiles(Invariant($"{solutionRootDirectory.FullName}"), "migrate.exe", SearchOption.AllDirectories).First();
            var dataAccessDirectories = Directory.GetDirectories(solutionRootDirectory.FullName, "*.DataAccess", SearchOption.AllDirectories);

            foreach (var dataAccessDirectory in dataAccessDirectories)
            {
                var configFilePath = Directory.GetFiles(dataAccessDirectory, "App.config", SearchOption.TopDirectoryOnly).First();
                var assembyFilePath = Directory.GetFiles(dataAccessDirectory, Invariant($"{GetLastSegment(dataAccessDirectory)}.dll"), SearchOption.AllDirectories).First(x => x.Contains("bin") && x.Contains(projectConfiguration));
                var migrateExecFileCopyPath = Invariant($"{Path.GetDirectoryName(assembyFilePath)}/migrate.exe");
                var migrateExecArguments = Invariant($"{GetLastSegment(dataAccessDirectory)}.dll {migrationConfiguration} /startupConfigurationFile:\"{configFilePath}\" /targetMigration:\"{targetMigration}\" /verbose");

                if (!File.Exists(migrateExecFileCopyPath))
                {
                    File.Copy(migrateExecFile, migrateExecFileCopyPath);
                }

                Process runMigrator = new Process();
                runMigrator.StartInfo.FileName = migrateExecFileCopyPath;
                runMigrator.StartInfo.Arguments = migrateExecArguments;
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
                    throw new Exception(Invariant($"Migrate.exe: Process returned an error (exit code {runMigrator.ExitCode})."));
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
