using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace CakeExtensions
{
    public static class CakeExtensions
    {
        [CakeMethodAlias]
        public static void MigrateDatabases(this ICakeContext context, string projectConfiguration = "Debug", string migrationConfiguration = "Configuration", int? targetMigration = null)
        {
            var root = GetRootPath();

            var migrateExecFile = Directory.GetFiles(root, "migrate.exe", SearchOption.AllDirectories).First();
            var dataAccessDirectories = Directory.GetDirectories(root, "*.DataAccess", SearchOption.TopDirectoryOnly);

            foreach (var dataAccessDirectory in dataAccessDirectories)
            {
                var configFilePath = Directory.GetFiles(dataAccessDirectory, "App.config", SearchOption.TopDirectoryOnly).First();
                var assembyFilePath = Directory.GetFiles(dataAccessDirectory, $"{GetLastSegment(dataAccessDirectory)}.dll", SearchOption.AllDirectories).First(x => x.Contains("bin") && x.Contains($"{projectConfiguration}"));
                var migrateExecFileCopyPath = $"{Path.GetDirectoryName(assembyFilePath)}/migrate.exe";
                var arguments = $"{GetLastSegment(assembyFilePath)} {migrationConfiguration} /startupConfigurationFile:\"{configFilePath}\" /targetMigration:\"{targetMigration}\" /verbose";

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

            //foreach(var dataAccessDirectory in dataAccessDirectories){
            //    var configFile = GetFiles($"{dataAccessDirectory.FullPath}/*.config").First();
            //    var assemblyFile = GetFiles($"{dataAccessDirectory.FullPath}/**/{projectConfiguration}/{dataAccessDirectory.Segments.Last()}.dll").First();
            //    var migrateExecFileCopy = $"{assemblyFile.GetDirectory()}/{migrateExecFile.Segments.Last()}";
            //    var arguments = $"{assemblyFile.GetFilename()} {migrationConfiguration} /startupConfigurationFile:\"{configFile}\" /targetMigration:\"{targetMigration}\" /verbose";

            //    CopyFile(migrateExecFile, migrateExecFileCopy);

            //    Process runMigrator = new Process();
            //    runMigrator.StartInfo.FileName = migrateExecFileCopy;
            //    runMigrator.StartInfo.Arguments = arguments;
            //    runMigrator.StartInfo.UseShellExecute = false;
            //    runMigrator.StartInfo.RedirectStandardOutput = true;
            //    runMigrator.StartInfo.RedirectStandardError = true;
            //    runMigrator.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            //    runMigrator.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            //    runMigrator.Start();
            //    runMigrator.BeginOutputReadLine();
            //    runMigrator.BeginErrorReadLine();
            //    runMigrator.WaitForExit();

            //    if (runMigrator.ExitCode != 0)
            //    {
            //        throw new Exception($"Migrate.exe: Process returned an error (exit code {runMigrator.ExitCode}).");
            //    }
        }

        private static string GetRootPath()
        {
            var currentWorkingDir = AppDomain.CurrentDomain.BaseDirectory;
            var uri = new Uri(currentWorkingDir).AbsolutePath;

            for (var i = 0; i < 2; i++)
            {
                var index = uri.LastIndexOf("/");
                uri = uri.Substring(0, index);
            }

            return uri;
        }

        private static string GetLastSegment(string path)
        {
            var uri = new Uri(path).AbsolutePath;

            var index = uri.LastIndexOf("/");

            return uri.Substring(index + 1);
        }
    }
}
