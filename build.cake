var target = Argument("target", "Build Project");
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
    .IsDependentOn("Restore NuGet Packages")
    .Does(() => {
        MSBuild("./Pipelines/Pipelines.sln", settings => settings.SetConfiguration(configuration));
    });

Task("Default")
    .Does(() => {
        Information("Target was not selected")
    });

RunTarget(target);