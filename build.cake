#tool nuget:?package=NUnit.ConsoleRunner&version=3.7.0
var TARGET = Argument ("target", Argument ("t", "Default"));
var VERSION = EnvironmentVariable ("APPVEYOR_BUILD_VERSION") ?? Argument("version", "0.0.9999");
var CONFIG = Argument("configuration", EnvironmentVariable ("CONFIGURATION") ?? "Release");
var SLN = "MonkeyMask.sln";

Task("Libraries").Does(()=>
{
	NuGetRestore(SLN);
	DotNetCoreBuild(SLN, new DotNetCoreBuildSettings
	{
		Configuration = CONFIG
	});
});

Task("Tests")
	.IsDependentOn("Libraries")
	.Does(() =>
{
	NUnit3($"./tests/**/bin/{ CONFIG }/**/*.Tests.dll", new NUnit3Settings());
});

Task ("NuGet")
	.IsDependentOn ("Tests")
	.Does (() =>
{
    if(!DirectoryExists("./Build/nuget/"))
        CreateDirectory("./Build/nuget");
        
	NuGetPack ("./.nuget/MonkeyMask.nuspec", new NuGetPackSettings { 
		Version = VERSION,
		OutputDirectory = "./Build/nuget/",
		BasePath = "./src"
	});	
});

//Build the component, which build samples, nugets, and libraries
Task ("Default")
	.IsDependentOn("NuGet");

Task ("Clean").Does (() => 
{
	CleanDirectory ("./component/tools/");
	CleanDirectories ("./Build/");
	CleanDirectories ("./**/bin");
	CleanDirectories ("./**/obj");
});

RunTarget (TARGET);