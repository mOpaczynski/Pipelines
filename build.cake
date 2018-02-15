var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Hello")
    .Does(() => {
        Information("Hello World!");
        });

Task("Restore NuGet Packages")
    .IsDependentOn("Hello")
    .Does(() => {
        NuGetRestore("./Pipelines/Pipelines.sln");
        });

Task("Build Project")
    .IsDependentOn("Restore Nuget Packages")
    .Does(() => {
        MSBuild("./Pipelines/Pipelines.sln", settings => settings.SetConfiguration(configuration));
        });

Task("Default")
    .IsDependentOn("Build Project");

RunTarget(target);