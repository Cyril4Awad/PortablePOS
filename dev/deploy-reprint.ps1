# Deploys the Reprint Documents web service build to the local IIS site (run as Administrator)
# Crystal Reports runtime 13.0.2000 (32 and 64 bit) must be installed on the server : the DLLs are read from the GAC

$site    = 'C:\inetpub\wwwroot\PortablePOS'
$build   = 'D:\PortablePOSBackend\StockSearch_WS\bin'
$reports = 'C:\Users\cyril.awad\source\Workspaces\Workspace\ERP_Project\ERP_Solution\POSOperations\POSOReports'

# 1. New service DLL
Copy-Item "$build\PortablePOS_WS.dll", "$build\PortablePOS_WS.pdb", "$build\PortablePOS_WS.xml" "$site\bin" -Force
Write-Host 'DLLs copied' -ForegroundColor Green

# 2. ReportsFolder in the deployed Web.config (the rest of the file, connection string included, is kept)
$configPath = "$site\Web.config"
$config = [IO.File]::ReadAllText($configPath)
if ($config -notmatch 'ReportsFolder') {
    $keys = "    <add key=""ReportsFolder"" value=""$reports""/>`r`n  </appSettings>"
    $config = ([regex]'\s*</appSettings>').Replace($config, "`r`n" + $keys, 1)
    [IO.File]::WriteAllText($configPath, $config)
    Write-Host 'Web.config updated' -ForegroundColor Green
} else {
    Write-Host 'Web.config already has ReportsFolder' -ForegroundColor Yellow
}

# 3. The identity of the app pool reads the reports (the folder is under a user profile)
icacls $reports /grant "IIS AppPool\DefaultAppPool:(OI)(CI)RX" | Out-Null
Write-Host 'Read access to the reports given to DefaultAppPool' -ForegroundColor Green

# 4. Restart the service
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:DefaultAppPool
& "$env:windir\system32\inetsrv\appcmd.exe" list apppool DefaultAppPool
