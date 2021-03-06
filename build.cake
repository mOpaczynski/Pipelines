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
            var packageSettings = new OctopusPackSettings {
                Version = project.Version,
                Description = project.Description,
                BasePath = project.FilesSource,
                Author = project.Author,
                OutFolder = project.OutputDirectory,
                Overwrite = project.Overwrite
            };

            OctoPack(project.Id, packageSettings);
        }
    });

Task("Octopus-Push-Packages")
    .IsDependentOn("Octopus-Package")
    .Does(() => {
        var octoPush = GetOctoPushSettings();
        
        OctoPush(
            octoPush.ServerUrl,
            octoPush.ApiKey,
            octoPush.Packages,
            new OctopusPushSettings { 
                ReplaceExisting = octoPush.ReplaceExisting 
            }
        );
    });

Task("Octopus-Create-Release")
    .IsDependentOn("Octopus-Push-Packages")
    .Does(() => {
        var octoRelease = GetOctoReleaseSettings();

        OctoCreateRelease(
            octoRelease.OctopusProjectName,
            new CreateReleaseSettings {
                Server = octoRelease.ServerUrl,
                ApiKey = octoRelease.ApiKey
            }
        );
    });

Task("Octopus-Deploy-Release")
    .IsDependentOn("Octopus-Create-Release")
    .Does(() => {
        var octoDeploy = GetOctoDeployReleaseSettings();

        OctoDeployRelease(
            octoDeploy.ServerUrl,
            octoDeploy.ApiKey,
            octoDeploy.OctopusProjectName,
            octoDeploy.TargetEnvironment,
            octoDeploy.ReleaseNumber,
            new OctopusDeployReleaseDeploymentSettings()
        );
    });

Task("Default")
    .Does(() => {
        Information("Target task was not selected, nothing will happen.");
    });

RunTarget(target);