using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using CakeExtensions.Models;

using static System.FormattableString;
using Path = System.IO.Path;

namespace CakeExtensions
{
    public static class CakeExtensions
    {
        private const string NugetPackOutputFolder = "packed-for-deploy";
        private const string OctopusApiKey = "API-FP6SWTW1NQG6NCX62R4JGBMLPBW";
        private const string OctopusServerUrl = "http://localhost:80";
        private const string OctopusProjectName = "Pipelines";

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
        public static List<PackageSettings> SetProjectsToPack(this ICakeContext context)
        {
            var solutionRootDirectory = GetRootPath();
            var apiProjects = Directory.GetDirectories(solutionRootDirectory, "*.Api", SearchOption.AllDirectories);
            var webProjects = Directory.GetDirectories(solutionRootDirectory, "*.Web", SearchOption.AllDirectories).Where(x => !x.Contains(".dotnet"));
            var projectsToPack = new List<string>();
            var outPutFolder = new DirectoryInfo(Invariant($"{solutionRootDirectory}/{NugetPackOutputFolder}")).FullName;
            List<PackageSettings> nugetPackSettings = new List<PackageSettings>();

            Directory.CreateDirectory(outPutFolder);
            projectsToPack.AddRange(apiProjects);
            projectsToPack.AddRange(webProjects);

            foreach (var project in projectsToPack)
            {
                var projectSettings = new PackageSettings
                {
                    Id = GetLastSegment(project),
                    Version = GetProjectVersion(),
                    Description = Invariant($"The {GetLastSegment(project)} deployment package, built on {DateTime.Now}."),
                    Authors = new [] {"Strange Issues Department"},
                    FilesSource = new DirectoryInfo(Invariant($"{project}/bin")).FullName + "\\*",
                    FilesTarget = "bin",
                    OutputDirectory = new DirectoryInfo(Invariant($"{solutionRootDirectory}/{NugetPackOutputFolder}")).FullName
                };

                context.Log.Information(projectSettings.FilesSource);

                nugetPackSettings.Add(projectSettings);
            }

            return nugetPackSettings;
        }

        [CakeMethodAlias]
        public static OctoPushSettings GetOctoPushSettings(this ICakeContext context)
        {
            var octoPush = new OctoPushSettings
            {
                ApiKey = OctopusApiKey,
                ServerUrl = OctopusServerUrl,
                ReplaceExisting = true,
                Packages = new List<FilePath>()
            };

            var projectPacks = Directory.GetFiles(new DirectoryInfo(Invariant($"{GetRootPath()}/{NugetPackOutputFolder}")).FullName, "*.nupkg");

            foreach (var projectPack in projectPacks)
            {
                octoPush.Packages.Add(new FilePath(projectPack));
            }

            return octoPush;
        }

        [CakeMethodAlias]
        public static OctoCreateReleaseSettings GetOctoReleaseSettings(this ICakeContext context)
        {
            return new OctoCreateReleaseSettings
            {
                ApiKey = OctopusApiKey,
                ServerUrl = OctopusServerUrl,
                OctopusProjectName = OctopusProjectName
            };
        }

        [CakeMethodAlias]
        public static OctoDeployReleaseSettings GetOctoDeployReleaseSettings(this ICakeContext context)
        {
            return new OctoDeployReleaseSettings
            {
                ApiKey = OctopusApiKey,
                ServerUrl = OctopusServerUrl,
                OctopusProjectName = OctopusProjectName,
                TargetEnvironment = "Dev",
                ReleaseNumber = "latest"
            };
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
            return "1.0.2.5";
        }
    }
}