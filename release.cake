#tool GitVersion.CommandLine&version=4.0.0
#tool Squirrel.Windows&version=1.9.0
#addin Cake.Squirrel&version=0.15.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

// TODO download all previous releases to make a differential update

// How to package with the settings
Task("Releasify")
	.Does(() => {
		var settings = new SquirrelSettings {
            NoMsi = true
        };
        var currentFolderFiles = GetFiles(".");
        Information(string.Join(";", currentFolderFiles));
        var currentFolderFiles2 = GetFiles("nuget");
        Information(string.Join(";", currentFolderFiles2));
        var currentFolderFiles3 = GetFiles("nuget/nuget");
        Information(string.Join(";", currentFolderFiles3));
        var files = GetFiles("nuget/nuget/*.nupkg");
        Information(string.Join(";", files));
        var file = files.Last();
        Information(file);
		Squirrel(file, settings);
	});

// Task("FindAndUninstall")
//     .Does(() =>
// {
//     var lastVersion = GetDirectories("c:/Users/jakub/AppData/Local/Youtube-Sync/app-*").Last();
//     Information(lastVersion);
//     var uninstallPath = lastVersion + File("/Youtube-Sync.exe");
//     Information(uninstallPath);

//     if(FileExists(uninstallPath))
//         UninstallTopshelf(uninstallPath);
// });

// Task("UpdateAndInstall")
//     .IsDependentOn("FindAndUninstall")
//     .Does(() =>
// {
//     var setupPath = File("c:/Updates/Youtube-Sync/Setup.exe");
//     Information(setupPath);
//     StartProcess(setupPath);
//     System.Threading.Thread.Sleep(8000);
    
//     var lastVersion = GetDirectories("c:/Users/jakub/AppData/Local/Youtube-Sync/app-*").Last();
//     Information(lastVersion);
//     var installPath = lastVersion + File("/Youtube-Sync.exe");
//     Information(installPath);
//     if(FileExists(installPath))
//     {
//         InstallTopshelf(installPath);
//         StartTopshelf(installPath);
//     }
// });


// TASK TARGETS
Task("Default")
    .IsDependentOn("Releasify");

// EXECUTION
RunTarget(target);