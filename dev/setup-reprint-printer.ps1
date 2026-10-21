# Reprint Documents : connects the shared HP of SSTEAM1 for this PC and deploys the web service that prints on the server printers
# Run as Administrator (since the PrintNightmare fix, only an administrator can install the driver of a shared printer)

$printer = '\\0.0.0.0\HP LaserJet P2050 Series PCL6'
$site    = 'C:\inetpub\wwwroot\PortablePOS'
$build   = 'D:\PortablePOSBackend\StockSearch_WS\bin'

# 1. Connection for the current user (installs the HP driver from SSTEAM1)
#    Add-Printer can stay waiting after the connection is made : skipped when the printer is already there
if (Get-Printer -Name $printer -ErrorAction SilentlyContinue) {
    Write-Host "Already connected : $printer" -ForegroundColor Yellow
} else {
    Add-Printer -ConnectionName $printer
    Write-Host "Connected : $printer" -ForegroundColor Green
}

# 2. Same connection for every user of the PC, the Windows user of the IIS app pool included
& rundll32.exe printui.dll,PrintUIEntry /ga /n"$printer"
Restart-Service Spooler -Force
Write-Host 'Connection added for all users, print spooler restarted' -ForegroundColor Green

# 3. Web service with PrintReprintDocument / GetReprintPrinters
Copy-Item "$build\PortablePOS_WS.dll", "$build\PortablePOS_WS.pdb", "$build\PortablePOS_WS.xml" "$site\bin" -Force
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:DefaultAppPool
Write-Host 'Web service deployed' -ForegroundColor Green

Get-Printer | Where-Object Name -like '*P2050*' | Select-Object Name, Type, DriverName, PrinterStatus
