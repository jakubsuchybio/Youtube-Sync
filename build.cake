#tool xunit.runner.console&version=2.4.1
#tool GitVersion.CommandLine&version=4.0.0
#tool Squirrel.Windows&version=1.9.1
#addin Cake.Squirrel&version=0.15.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var slnFile = "Youtube-Sync.sln";
string packageVersion;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./artifacts");
    CleanDirectories("./src/**/bin");
    CleanDirectories("./src/**/obj");
});

Task("Version")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var gitVersion = GitVersion(new GitVersionSettings{
        UpdateAssemblyInfo = true
    });

    packageVersion = gitVersion.NuGetVersion;

    Information($"NuGetVersion: {packageVersion}");
});

Task("Restore")
    .IsDependentOn("Version")
    .Does(() =>
{
    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild(slnFile, new DotNetCoreBuildSettings { NoRestore = true });
});

Task("Publish")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCorePublish(slnFile, new DotNetCorePublishSettings { NoRestore = true, NoBuild = true });
});

Task("Package")
    .IsDependentOn("Publish")
    .Does(() =>
{
    NuGetPack("./Youtube-Sync.nuspec", new NuGetPackSettings { OutputDirectory = "./artifacts" , Version = packageVersion });
});

// How to package with the settings
Task("Releasify")
    .IsDependentOn("Package")
	.Does(() => {
		var settings = new SquirrelSettings {
            NoMsi = true,
            Silent = true,
            ReleaseDirectory = "./artifacts/Releases/"
        };

        var file = GetFiles("./artifacts/*.nupkg").First();
		Squirrel(file, settings);
	});

Task("CopyToUpdates")
    .IsDependentOn("Releasify")
	.Does(() => {
        CopyDirectory("./artifacts/Releases/", "c:/Updates/Youtube-Sync/");
	});

// TASK TARGETS
Task("Default")
    .IsDependentOn("Releasify");

// EXECUTION
RunTarget(target);