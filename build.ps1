Param(
    [string]$option
)
if ($option){
    Write-Host "Rebuilding projects..." -ForegroundColor Cyan
    dotnet build ./**/project.json $option
}
else{
    Write-Host "Building projects..." -ForegroundColor Cyan
    dotnet build ./**/project.json
}
Write-Host "Finished."
