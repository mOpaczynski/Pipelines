#tool "nuget:?package=NUnit.ConsoleRunner"

using System.Diagnostics;

var target = Argument("target", "Default");
var projectConfiguration = Argument("projectConfiguration", "Release");
var migrationConfiguration = Argument("migrationConfiguration", "Configuration");

var solutionFilePath = GetFiles("./**/*.sln").First();

Task("Restore-Nuget-Packages")
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
        MigrateDatabases();
    });

Task("Rollback-Migrations")
    .Does(()=> {
        MigrateDatabases(0);
    });

Task("Restore-Databases-After-Rollback")
    .Does(() => {
        MigrateDatabases();
    });

Task("Test-Migration-Seeds")
    .Does(() => {
        MigrateDatabases();
    });

Task("Test-Migrate-Databases")
    .IsDependentOn("Migrate-Databases")
    .IsDependentOn("Rollback-Migrations")
    .IsDependentOn("Restore-Databases-After-Rollback")
    .IsDependentOn("Test-Migration-Seeds")
    .Does(()=> {
    });

Task("Run-Unit-Tests")
    .Does(() => {
        var testAssemblies = GetFiles($"./**/bin/{projectConfiguration}/*Tests.dll");
        NUnit3(testAssemblies);
    });

Task("Default")
    .Does(() => {
        Information("Target task was not selected, nothing will happen.");
    });

RunTarget(target);

private void MigrateDatabases(int? targetMigration = null){
    var migrateExecFile = GetFiles("./**/packages/EntityFramework*/tools/migrate.exe").First();
    var dataAccessDirectories = GetSubDirectories("./").Where(x => x.FullPath.Contains(".DataAccess"));

    foreach(var dataAccessDirectory in dataAccessDirectories){
        var configFile = GetFiles($"{dataAccessDirectory.FullPath}/*.config").First();
        var assemblyFile = GetFiles($"{dataAccessDirectory.FullPath}/**/{projectConfiguration}/{dataAccessDirectory.Segments.Last()}.dll").First();
        var migrateExecFileCopy = $"{assemblyFile.GetDirectory()}/{migrateExecFile.Segments.Last()}";
        var arguments = $"{assemblyFile.GetFilename()} {migrationConfiguration} /startupConfigurationFile:\"{configFile}\" /targetMigration:\"{targetMigration}\" /verbose";
            
        CopyFile(migrateExecFile, migrateExecFileCopy);

        Process runMigrator = new Process();
        runMigrator.StartInfo.FileName = migrateExecFileCopy;
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
}