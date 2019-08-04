#tool xunit.runner.console&version=2.4.1
#tool GitVersion.CommandLine&version=4.0.0
#tool Squirrel.Windows&version=1.9.0
#addin Cake.Squirrel&version=0.15.1
#addin Cake.Services&version=0.3.5
#addin Cake.Topshelf&version=0.2.4

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

Task("FindAndUninstall")
    .Does(() =>
{
    var lastVersion = GetDirectories("c:/Users/jakub/AppData/Local/Youtube-Sync/app-*").Last();
    Information(lastVersion);
    var uninstallPath = lastVersion + File("/Youtube-Sync.exe");
    Information(uninstallPath);

    if(FileExists(uninstallPath))
        UninstallTopshelf(uninstallPath);
});

Task("UpdateAndInstall")
    .IsDependentOn("FindAndUninstall")
    .Does(() =>
{
    var setupPath = File("c:/Updates/Youtube-Sync/Setup.exe");
    Information(setupPath);
    StartProcess(setupPath);
    
    var lastVersion = GetDirectories("c:/Users/jakub/AppData/Local/Youtube-Sync/app-*").Last();
    Information(lastVersion);
    var installPath = lastVersion + File("/Youtube-Sync.exe");
    Information(installPath);
    if(FileExists(installPath))
        InstallTopshelf(installPath);
});


// TASK TARGETS
Task("Default")
    .IsDependentOn("UpdateAndInstall");

// EXECUTION
RunTarget(target);