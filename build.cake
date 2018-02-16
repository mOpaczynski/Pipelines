var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionFilePath = GetFiles("./Pipelines/Pipelines.sln").First;
var migrateExecFile = GetFiles("./Pipelines/packages/EntityFramework*/tools/migrate.exe").First;

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
        using(var migrator = CreateEfMigrator()){
            migrator.MigrateToLatest();
            migrator.Commit();
        }
    });

Task("Default")
    .Does(() => {
        Information("Target task was not selected, nothing will happen.");
    });

RunTarget(target)