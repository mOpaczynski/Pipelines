using System.Diagnostics;

var target = Argument("target", "Default");
var projectConfiguration = Argument("projectConfiguration", "Release");
var migrationConfiguration = Argument("migrationConfiguration", "Configuration");

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
        var migrateExecFile = GetFiles("./**/packages/EntityFramework*/tools/migrate.exe").First().FullPath;
        var dataAccessDirectories = GetSubDirectories("./").Where(x => x.FullPath.Contains(".DataAccess"));

        foreach(var dataAccessDirectory in dataAccessDirectories){
            Information(dataAccessDirectory.ToString());
            var configFile = GetFiles($"{dataAccessDirectory.FullPath}/*.config").First();
            Information(configFile);
            var assemblyFile = GetFiles($"{dataAccessDirectory.FullPath}/**/{projectConfiguration}/{dataAccessDirectory.Segments.Last()}.dll").First();
            Information(assemblyFile);

            Information(migrateExecFile);

            Information($"{assemblyFile.GetFilename()} /startUpConfigurationFile='{configFile}' /startUpDirectory='{assemblyFile.GetDirectory()}'");

            Process runMigrator = new Process();
            runMigrator.StartInfo.FileName = migrateExecFile;
            runMigrator.StartInfo.Arguments = $"{assemblyFile.GetFilename()} {migrationConfiguration} /startUpDataDirectory='{assemblyFile.GetDirectory()} /startUpConfigurationFile='{configFile}' /startUpDirectory='{assemblyFile.GetDirectory()}' /verbose";
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