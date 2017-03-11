Param(
    [string]$option
)
if ($option){
    Write-Host "Rebuilding projects..." -ForegroundColor Cyan
    dotnet build $option
}
else{
    Write-Host "Building projects..." -ForegroundColor Cyan
    dotnet build
}
Write-Host "Finished."
