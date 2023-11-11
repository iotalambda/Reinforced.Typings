#addin nuget:https://api.nuget.org/v3/index.json?package=Cake.FileHelpers&version=6.1.3
#tool nuget:?package=NuGet.CommandLine&version=6.4.0

var target = Argument("target", "Build");
const string version = "1.6.2.100";

Task("Clean")
  .Does(() =>
{
  CleanDirectories("../Reinforced.Typings*/**/bin");
  CleanDirectories("../Reinforced.Typings*/**/obj");

  Information("Clean completed");
});

const string packageRoot = "../package";
const string licenseRoot = "../package/license";
const string toolsPath = "../package/tools";
const string contentPath = "../package/content";
const string buildPath = "../package/build";
const string multiTargetPath = "../package/buildMultiTargeting";
const string libPath = "../package/lib";
const string RELEASE = "Release";
const string NETCORE22 = "netcoreapp2.2";
const string NETCORE21 = "netcoreapp2.1";
const string NETSTANDARD16 = "netstandard1.6";
const string NETSTANDARD15 = "netstandard1.5";
const string NETSTANDARD20 = "netstandard2.0";
const string NETCORE31 = "netcoreapp3.1";
const string NETCORE30 = "netcoreapp3.0";
const string NETCORE20 = "netcoreapp2.0";
const string NETCORE10 = "netcoreapp1.0";
const string NETCORE11 = "netcoreapp1.1";
const string NET461 = "net461";
const string NET46 = "net46";
const string NET45 = "net45";
const string NET50 = "net5.0";
const string NET60 = "net6.0";
const string NET70 = "net7.0";
const string NET80 = "net8.0";

var cliFrameworks = new[] { NET70, NET80 };
var rtFrameworks = new[]  { NETSTANDARD20, NET70, NET80 };
var taskFrameworks = new[] { NET46, NETSTANDARD20 };

var netCore = new HashSet<string>(new[]{NETSTANDARD20, NET70, NET80});
var netCoreApp = new HashSet<string>(new[]{NET70, NET80});

const string CliNetCoreProject = "../Reinforced.Typings.Cli/Reinforced.Typings.Cli.NETCore.csproj";
const string RtNetCoreProject = "../Reinforced.Typings/Reinforced.Typings.NETCore.csproj";
const string IntegrateProject = "../Reinforced.Typings.Integrate/Reinforced.Typings.Integrate.NETCore.csproj";
const string tfParameter = "TargetFrameworks";
string tfRgx = $"<{tfParameter}>[a-zA-Z0-9;.]*</{tfParameter}>"; 
const string tfSingleParameter = "TargetFramework";
string tfsRgx = $"<{tfSingleParameter}>[a-zA-Z0-9;.]*</{tfSingleParameter}>"; 

Task("Reset")
  .IsDependentOn("UpdateVersions")
  .Description("Resets target frameworks")
  .Does(() =>
{
	var fw = NET461;
	ReplaceRegexInFiles(CliNetCoreProject,tfRgx,$"<{tfParameter}>{fw}</{tfParameter}>");       
    ReplaceRegexInFiles(RtNetCoreProject,tfRgx,$"<{tfParameter}>{fw}</{tfParameter}>"); 
    ReplaceRegexInFiles(CliNetCoreProject,tfsRgx,$"<{tfSingleParameter}>{fw}</{tfSingleParameter}>");       
    ReplaceRegexInFiles(RtNetCoreProject,tfsRgx,$"<{tfSingleParameter}>{fw}</{tfSingleParameter}>"); 
});

Task("PackageClean")
  .Description("Cleaning temporary package folder")
  .Does(() =>
{
  CleanDirectory(packageRoot); EnsureDirectoryExists(packageRoot);
  CleanDirectory(licenseRoot); EnsureDirectoryExists(licenseRoot);
  CleanDirectory(toolsPath); EnsureDirectoryExists(toolsPath);
  CleanDirectory(contentPath); EnsureDirectoryExists(contentPath);
  CleanDirectory(buildPath); EnsureDirectoryExists(buildPath);
  CleanDirectory(multiTargetPath); EnsureDirectoryExists(multiTargetPath);
  CleanDirectory(libPath); EnsureDirectoryExists(libPath);
});

Task("UpdateVersions")
.Description("Updating assembly/file versions")
.Does(()=>{
 // Update versions
  foreach(var p in new[]{CliNetCoreProject,RtNetCoreProject,IntegrateProject}){
    foreach(var par in new[]{"AssemblyVersion","FileVersion","InformationalVersion"}){
      var rgx = $"<{par}>[0-9.]*</{par}>";
      ReplaceRegexInFiles(p,rgx,$"<{par}>{version}</{par}>");
    }    
  }
});

Task("BuildIntegrate")
.Description("Building RT's integration MSBuild task")
.Does(()=>{
  foreach(var fw in taskFrameworks){
	  DotNetMSBuildSettings mbs = null;
          
      if (netCore.Contains(fw)){
		var ac = "NETCORE;" + fw.ToUpperInvariant().Replace(".","_");
		if (netCoreApp.Contains(fw)) ac += ";NETCORE_APP";
        mbs = new DotNetMSBuildSettings()
          .WithProperty("RtAdditionalConstants",ac)
          .WithProperty("RtNetCore","True");
      }
    DotNetPublish(IntegrateProject, new DotNetPublishSettings
    {
      Verbosity = DotNetVerbosity.Quiet,
      Configuration = RELEASE,	  
      MSBuildSettings = mbs,
      OutputDirectory = System.IO.Path.Combine(buildPath, fw),      
      Framework = fw
    });    
    
  }  
  
});

Task("Build")
  .IsDependentOn("Clean")
  .IsDependentOn("PackageClean")
  .IsDependentOn("UpdateVersions")
  .IsDependentOn("BuildIntegrate")
  .Does(() =>
{
  // Build various versions of CLI tool
  foreach(var fw in cliFrameworks){
      Information("---------");
      Information("Building CLI for {0}",fw);
      Information("---------");

      ReplaceRegexInFiles(CliNetCoreProject,tfRgx,$"<{tfParameter}>{fw}</{tfParameter}>");       
      ReplaceRegexInFiles(RtNetCoreProject,tfRgx,$"<{tfParameter}>{fw}</{tfParameter}>"); 
      ReplaceRegexInFiles(CliNetCoreProject,tfsRgx,$"<{tfSingleParameter}>{fw}</{tfSingleParameter}>");       
      ReplaceRegexInFiles(RtNetCoreProject,tfsRgx,$"<{tfSingleParameter}>{fw}</{tfSingleParameter}>"); 

      DotNetMSBuildSettings mbs = null;
          
      if (netCore.Contains(fw)){
		var ac = "NETCORE;" + fw.ToUpperInvariant().Replace(".","_");
		if (netCoreApp.Contains(fw)) ac += ";NETCORE_APP";
        mbs = new DotNetMSBuildSettings()
          .WithProperty("RtAdditionalConstants",ac)
          .WithProperty("RtNetCore","True");
      }
      DotNetPublish(CliNetCoreProject, new DotNetPublishSettings {  
        Configuration = RELEASE, 
        MSBuildSettings = mbs,
        Framework = fw,
        Verbosity = DotNetVerbosity.Quiet,
        OutputDirectory = System.IO.Path.Combine(toolsPath, fw)        
      });
  }


  // Build various versions of lib
  foreach(var fw in rtFrameworks){
      Information("---------");
      Information("Building lib for {0}",fw);
      Information("---------");

      ReplaceRegexInFiles(RtNetCoreProject,tfRgx,$"<{tfParameter}>{fw}</{tfParameter}>");  
      ReplaceRegexInFiles(RtNetCoreProject,tfsRgx,$"<{tfSingleParameter}>{fw}</{tfSingleParameter}>"); 
      
      var mbs = new DotNetMSBuildSettings()
          .WithProperty("DocumentationFile",$@"bin\Release\{fw}\Reinforced.Typings.xml");

      if (netCore.Contains(fw)){
		var ac = "NETCORE;" + fw.ToUpperInvariant().Replace(".","_");
		if (netCoreApp.Contains(fw)) ac += ";NETCORE_APP";
        mbs = mbs
          .WithProperty("RtAdditionalConstants",ac)
          .WithProperty("RtNetCore","True");
      }
     DotNetPublish(RtNetCoreProject, new DotNetPublishSettings {  
        Configuration = RELEASE,
        MSBuildSettings = mbs,    
        Framework = fw,
        Verbosity = DotNetVerbosity.Quiet,
        OutputDirectory = System.IO.Path.Combine(libPath, fw)
      });
  }

  
  
  Information("---------");
  Information("Copying build stuff");
  Information("---------");

  // Copy build stuff
  CopyFileToDirectory("../stuff/Reinforced.Typings.settings.xml", contentPath);
  CopyFileToDirectory("../stuff/Reinforced.Typings.targets", buildPath);
  CopyFileToDirectory("../stuff/Reinforced.Typings.Multi.targets", multiTargetPath);

  Information("---------");
  Information("Writing readme");
  Information("---------");
  
  CopyFileToDirectory("../stuff/license.txt", licenseRoot);

  // Copy readme with actual version of Reinforced.Typings.settings.xml
  CopyFileToDirectory("../stuff/readme.txt", packageRoot);
  CopyFileToDirectory("../icon.png", packageRoot);
  using(var tr = System.IO.File.OpenRead("../stuff/Reinforced.Typings.settings.xml"))
  using(var tw = new System.IO.FileStream(System.IO.Path.Combine(packageRoot,"readme.txt"),FileMode.Append))
  {
    tr.CopyTo(tw);
  }

  Information("---------");
  Information("Updating nuspec");
  Information("---------");
  // Copy nuspec
  CopyFileToDirectory("../stuff/Reinforced.Typings.nuspec", packageRoot);
  
  var rn = string.Empty;
  if (System.IO.File.Exists(System.IO.Path.Combine("../stuff/relnotes", version) + ".md")){
        rn = System.IO.File.ReadAllText(System.IO.Path.Combine("../stuff/relnotes", version) + ".md");
  }

  Information("---------");
  Information("Packaging");
  Information("---------");  

  NuGetPack("../package/Reinforced.Typings.nuspec",new NuGetPackSettings(){
    ReleaseNotes = new List<string>() { rn },
    Version = version,
    OutputDirectory = "../"
  });  
  Information("Build complete");
});

RunTarget(target);