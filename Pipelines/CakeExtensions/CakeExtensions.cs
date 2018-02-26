using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using CakeExtensions.Models;

using static System.FormattableString;

namespace CakeExtensions
{
    public static class CakeExtensions
    {
        private const string NugetPackFolderName = ".nuget";

        [CakeMethodAlias]
        public static void MigrateDatabases(this ICakeContext context, string projectConfiguration = "Release", string migrationConfiguration = "Configuration", int? targetMigration = null)
        {
            var solutionRootDirectory = GetRootPath();
            var migrateExecFile = Directory.GetFiles(Invariant($"{solutionRootDirectory}"), "migrate.exe", SearchOption.AllDirectories).First();
            var dataAccessProjects = Directory.GetDirectories(solutionRootDirectory, "*.DataAccess", SearchOption.AllDirectories);

            foreach (var dataAccessProject in dataAccessProjects)
            {
                var configFilePath = Directory.GetFiles(dataAccessProject, "App.config", SearchOption.TopDirectoryOnly).First();
                var assembyFilePath = Directory.GetFiles(dataAccessProject, Invariant($"{GetLastSegment(dataAccessProject)}.dll"), SearchOption.AllDirectories).First(x => x.Contains("bin") && x.Contains(projectConfiguration));
                var migrateExecFileCopyPath = Invariant($"{Path.GetDirectoryName(assembyFilePath)}/migrate.exe");
                var migrateExecArguments = Invariant($"{GetLastSegment(dataAccessProject)}.dll {migrationConfiguration} /startupConfigurationFile:\"{configFilePath}\" /targetMigration:\"{targetMigration}\" /verbose");

                if (!File.Exists(migrateExecFileCopyPath))
                {
                    File.Copy(migrateExecFile, migrateExecFileCopyPath);
                }

                var runMigrator = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = migrateExecFileCopyPath,
                        Arguments = migrateExecArguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

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

        [CakeMethodAlias]
        public static List<NuGetPackSettings> SetProjectsToPack(this ICakeContext context, string projectConfiguration = "Release")
        {
            var solutionRootDirectory = GetRootPath();
            var apiProjects = Directory.GetDirectories(solutionRootDirectory, "*.Api", SearchOption.AllDirectories);
            List<NuGetPackSettings> nugetPackSettings = new List<NuGetPackSettings>();

            foreach (var apiProject in apiProjects)
            {
                var projectSettings = new NuGetPackSettings
                {
                    Id = GetLastSegment(apiProject),
                    Version = GetProjectVersion(),
                    Title = "Test",
                    Authors = new[] {"One", "Two", "Three"},
                    Description = "Test",
                    Summary = "Summary",
                    ProjectUrl = new Uri("http://the-project-url.pl"),
                    Files = new[]
                    {
                        new NuSpecContent
                        {
                            Source = Invariant($"{apiProject}/bin/{projectConfiguration}/*"),
                            Target = "content"
                        }
                    },
                    OutputDirectory = Invariant($"{apiProject}/{NugetPackFolderName}")
                };

                nugetPackSettings.Add(projectSettings);
            }

            return nugetPackSettings;
        }

        private static string GetRootPath()
        {
            return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        }

        private static string GetLastSegment(string path)
        {
            return new DirectoryInfo(path).Name;
        }

        private static string GetProjectVersion()
        {
            return "1.0.0.0";
        }
    }
}