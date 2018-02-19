using System.Diagnostics;

var target = Argument("target", "Default");
var projectConfiguration = Argument("projectConfiguration", "Release");
var migrationConfiguration = Argument("migrationConfiguration", "Configuration");
var targetMigration = Argument("targetMigration", "");

var solutionFilePath = GetFiles("./**/*.sln").First();

Task("Hello")
    .Does(() => {
        Information("Hello World!");
    });

Task("Restore-Nuget-Packages")
    .IsDependentOn("Hello")
    .Does(() => {
        NuGetRestore(solutionFilePath);
    });

Task("Build-Solution")
    .IsDependentOn("Restore-Nuget-Packages")
    .Does(() => {
        MSBuild(solutionFilePath, settings => settings.SetConfiguration(projectConfiguration));
    });

Task("Migrate-Databases")
    .Does(() => {
        var migrateExecFile = GetFiles("./**/packages/EntityFramework*/tools/migrate.exe").First();
        var dataAccessDirectories = GetSubDirectories("./").Where(x => x.FullPath.Contains(".DataAccess"));

        foreach(var dataAccessDirectory in dataAccessDirectories){
            var configFile = GetFiles($"{dataAccessDirectory.FullPath}/*.config").First();
            var assemblyFile = GetFiles($"{dataAccessDirectory.FullPath}/**/{projectConfiguration}/{dataAccessDirectory.Segments.Last()}.dll").First();
            var arguments = $"{assemblyFile.GetFilename()} {migrationConfiguration} /startupConfigurationFile:\"{configFile}\" /targetMigration:\"{targetMigration}\ /verbose";
            
            CopyFile(migrateExecFile, $"{assemblyFile.GetDirectory()}/{migrateExecFile.Segments.Last()}");

            Process runMigrator = new Process();
            runMigrator.StartInfo.FileName = $"{assemblyFile.GetDirectory()}/{migrateExecFile.Segments.Last()}";
            runMigrator.StartInfo.Arguments = arguments;
            runMigrator.StartInfo.UseShellExecute = false;
            runMigrator.StartInfo.RedirectStandardOutput = true;
            runMigrator.StartInfo.RedirectStandardError = true;
            runMigrator.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            runMigrator.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            runMigrator.Start();
            runMigrator.BeginOutputReadLine();
            runMigrator.BeginErrorReadLine();
            runMigrator.WaitForExit();

            if (runMigrator.ExitCode != 0)
            {
                throw new Exception($"Migrate.exe: Process returned an error (exit code {runMigrator.ExitCode}).");
            }
        }
    });

Task("Default")
    .Does(() => {
        Information("Target task was not selected, nothing will happen.");
    });

RunTarget(target)