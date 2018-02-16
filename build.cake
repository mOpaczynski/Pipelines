using System.Diagnostics;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

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
        MSBuild(solutionFilePath, settings => settings.SetConfiguration(configuration));
    });

Task("Migrate-Databases")
    .Does(() => {
        Information("Migrating Databases...");

        var migrateExecFile = GetFiles("./**/packages/EntityFramework*/tools/migrate.exe").First().FullPath;

        Process runMigrator = new Process();
        runMigrator.StartInfo.FileName = migrateExecFile;
        runMigrator.StartInfo.Arguments = "dasdasd asdasdsad";
        runMigrator.Start();
        runMigrator.WaitForExit();
    });

Task("Default")
    .Does(() => {
        Information("Target task was not selected, nothing will happen.");
    });

RunTarget(target)