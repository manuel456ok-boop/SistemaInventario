# Captura la ventana real; este script no genera interfaces simuladas.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class CapturaInventario {
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect { public int Left,Top,Right,Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdc, uint flags);
}
'@
$modulos = @('01-productos','02-categorias','03-suplidores','04-papelera','05-informes')
foreach ($modulo in $modulos) {
    Read-Host "Abra el módulo $modulo en el inventario. Vuelva aquí y pulse Enter"
    $proceso = Get-Process -Name 'SistemaInventario.Presentacion' | Where-Object MainWindowHandle -ne 0 | Select-Object -First 1
    if (!$proceso) { throw 'Abra primero SistemaInventario.Presentacion.exe.' }
    $rect = New-Object CapturaInventario+Rect
    if (![CapturaInventario]::GetWindowRect($proceso.MainWindowHandle, [ref]$rect)) { throw 'No se pudo localizar la ventana.' }
    $bitmap = New-Object System.Drawing.Bitmap ($rect.Right-$rect.Left),($rect.Bottom-$rect.Top)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $hdc = $graphics.GetHdc()
    try {
        if (![CapturaInventario]::PrintWindow($proceso.MainWindowHandle,$hdc,2)) { throw 'Windows no permitió capturar esta ventana.' }
    } finally { $graphics.ReleaseHdc($hdc); $graphics.Dispose() }
    try { $bitmap.Save((Join-Path $PSScriptRoot "$modulo.png"),[System.Drawing.Imaging.ImageFormat]::Png) }
    finally { $bitmap.Dispose() }
    Write-Host "Guardada $modulo.png. Revise que muestre el módulo correcto."
}
