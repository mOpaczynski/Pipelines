var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Hello")
    .Does(() => {
        Information("Hello World!");
    });

Task("Restore-Nuget-Packages")
    .IsDependentOn("Hello")
    .Does(() => {
        NuGetRestore("./Pipelines/Pipelines.sln");
    });

Task("Build-Solution")
    .IsDependentOn("Restore-Nuget-Packages")
    .Does(() => {
        MSBuild("./Pipelines/Pipelines.sln", settings => settings.SetConfiguration(configuration));
    });

Task("Migrate-Databases")
    .Does(() => {
        Information("Migrating Databases...")
    });

Task("Default")
    .Does(() => {
        Information("Target task was not selected, nothing will happen.");
    });

RunTarget(target)