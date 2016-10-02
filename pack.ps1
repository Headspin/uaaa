#=-Global variables-=#
$solutionDir=Split-Path ((Get-Variable MyInvocation -Scope 0).Value).MyCommand.Path
$releaseDir="output"
#=-Script-=#
# Clean release directory
if (Test-Path "$solutionDir\$releaseDir"){
	gci -path "$solutionDir\$releaseDir"|ri -force -recurse
}else{
	mkdir "$solutionDir/$releaseDir"
}
# Build Uaaa.Core package
$project = "Uaaa.Core"
if (Test-Path "$solutionDir/src/$project/bin/release"){
	gci -Path "$solutionDir/src/$project/bin/release"|ri -Force -Recurse
}
dotnet pack "$solutionDir/src/$project/project.json" -c Release
if (! $?){ 
	#=-If last error code <> 0 -> return exit code 1
	exit 1 
}
copy-item -Path "$solutionDir/src/$project/bin/release/*.nupkg" -Destination "$solutionDir/$releaseDir"

# Build Uaaa.Data.Sql package
$project = "Uaaa.Data.Sql"
if (Test-Path "$solutionDir/src/$project/bin/release"){
	gci -Path "$solutionDir/src/$project/bin/release"|ri -Force -Recurse
}
Write-Host "$solutionDir/src/$project/project.json"

dotnet pack "$solutionDir/src/$project/project.json" -c Release
if (! $?){ 
	#=-If last error code <> 0 -> return exit code 1
	exit 1 
}
copy-item -Path "$solutionDir/src/$project/bin/release/*.nupkg" -Destination "$solutionDir/$releaseDir"
