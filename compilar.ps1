param([switch]$Autocontenido)
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
$rutaConexion = Join-Path $PSScriptRoot 'build\win-x64\conexion.txt'
$conexionAnterior = if (Test-Path $rutaConexion) { [IO.File]::ReadAllText($rutaConexion) } else { $null }
try {
    dotnet restore src/SistemaInventario.sln
    if ($LASTEXITCODE -ne 0) { throw 'Falló la restauración de paquetes.' }
    dotnet build src/SistemaInventario.sln -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación.' }
    dotnet run --project tests/SistemaInventario.Pruebas -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de negocio.' }
    $incluirRuntime = if ($Autocontenido) { 'true' } else { 'false' }
    dotnet publish src/SistemaInventario.Presentacion/SistemaInventario.Presentacion.csproj -c Release -r win-x64 --self-contained $incluirRuntime -o build/win-x64
    if ($LASTEXITCODE -ne 0) { throw 'Falló la publicación.' }
    Write-Host 'Listo: build\win-x64\SistemaInventario.Presentacion.exe'
}
finally {
    if ($null -ne $conexionAnterior -and (Test-Path (Split-Path $rutaConexion))) { [IO.File]::WriteAllText($rutaConexion, $conexionAnterior) }
    Pop-Location
}
