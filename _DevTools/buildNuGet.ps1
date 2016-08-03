#=-Global variables-=#
$project="Uaaa"
$branchDir=Split-Path ((Get-Variable MyInvocation -Scope 0).Value).MyCommand.Path | Split-Path -Parent
$buildDir="_Release"
$nugetFilenameWithPath = Resolve-Path "..\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe"
Set-Alias nuget $nugetFilenameWithPath -Scope Script
Set-Alias msbuild "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

#=-Script-=#
if (Test-Path "$branchDir\$buildDir"){
	gci -path "$branchDir\$buildDir"|ri -force -recurse
}
msbuild "$branchDir\$project.sln" /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /Verbosity:quiet
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