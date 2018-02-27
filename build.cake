#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=OctopusTools"
#r "tools/CakeExtensions/CakeExtensions.dll"

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
        ////MigrateDatabases(projectConfiguration, migrationConfiguration);
    });

Task("Rollback-Migrations")
    .Does(()=> {
        ////MigrateDatabases(projectConfiguration, migrationConfiguration, 0);
    });

Task("Migrate-Databases-And-Test-Seeds")
    .IsDependentOn("Migrate-Databases")
    .IsDependentOn("Rollback-Migrations")
    .Does(() => {
        ////MigrateDatabases(projectConfiguration, migrationConfiguration);
        ////MigrateDatabases(projectConfiguration, migrationConfiguration);
    });

Task("Run-Unit-Tests")
    .Does(() => {
        var testAssemblies = GetFiles($"./**/bin/{projectConfiguration}/JenkinsTests.dll");
        var settings = new NUnit3Settings {
            Results = new[] { new NUnit3Result {FileName = "UnitTestsResult.xml"}},
            Labels = NUnit3Labels.Before
        };

        NUnit3(testAssemblies, settings);
    });

Task("Run-Api-Tests")
    .Does(() => {
        var testAssemblies = GetFiles($"./**/bin/{projectConfiguration}/ApiTests.dll");
        var settings = new NUnit3Settings {
            Results = new[] { new NUnit3Result {FileName = "ApiTestsResult.xml"}},
            Labels = NUnit3Labels.Before
        };

        NUnit3(testAssemblies, settings);
    });

Task("Run-Ui-Tests")
    .Does(() => {
        var testAssemblies = GetFiles($"./**/bin/{projectConfiguration}/UiTests.dll");
        var settings = new NUnit3Settings {
            Results = new[] { new NUnit3Result {FileName = "UiTestsResult.xml"}},
            Labels = NUnit3Labels.Before
        };

        NUnit3(testAssemblies, settings);
    });

Task("Octopus-Package")
    .Does(() => {
        var projects = SetProjectsToPack();

        foreach (var project in projects) {
            var nuGetPackSettings = new NuGetPackSettings {
                Id = project.Id,
                Version = project.Version,
                Description = project.Description,
                Authors = project.Authors,
                Files = new[] { new NuSpecContent { Source = project.FilesSource, Target = project.FilesTarget } },
                OutputDirectory = project.OutputDirectory
            };

            NuGetPack(nuGetPackSettings);
        }
    });

Task("Octopus-Push")
    .IsDependentOn("Octopus-Package")
    .Does(() => {
        var octoPush = SetOctoPush();
        
        OctoPush(
            octoPush.ServerUrl, 
            octoPush.ApiKey,
            octoPush.Packages,
            new OctopusPushSettings{ ReplaceExisting = octoPush.ReplaceExisting }
        );

        OctoCreateRelease(
            "Pipelines",
            new CreateReleaseSettings {
                Server = octoPush.ServerUrl,
                ApiKey = octoPush.ApiKey
            }
        );
    });

Task("Octopus-Release")
    .IsDependentOn("Octopus-Push")
    .Does(() => {
        
    });

Task("Default")
    .Does(() => {
        Information("Target task was not selected, nothing will happen.");
    });

RunTarget(target);