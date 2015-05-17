#=-Global variables-=#
$project="Uaaa"
$branchDir=Split-Path ((Get-Variable MyInvocation -Scope 0).Value).MyCommand.Path | Split-Path -Parent
$buildDir="_Release"
$nugetFilenameWithPath = Resolve-Path "..\packages\NuGet.CommandLine.2.8.5\tools\NuGet.exe"
Set-Alias nuget $nugetFilenameWithPath -Scope Script

#=-Script-=#
if (Test-Path "$branchDir\$buildDir"){
	gci -path "$branchDir\$buildDir"|ri -force -recurse
}
C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "$branchDir\$project.sln" /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /Verbosity:quiet
if (! $?){
	#=-If last error code <> 0 -> return exit code 1
	exit 1
}

# Build Uaaa package
$package="Uaaa"
$packageDir="$branchDir\$buildDir\$package"
xcopy "$branchDir\$package.nuspec" "$packageDir" /D /Y /Q
push-location
set-location -path $packageDir
nuget pack $package.nuspec
xcopy *.nupkg "$branchDir\$buildDir" /D /Y /Q
pop-location