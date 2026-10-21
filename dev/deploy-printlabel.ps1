# Deploys the Print Label web service build to the local IIS site (run as Administrator)
# The current files were backed up to dev\backups\deploy-before-printlabel-* before this script was made

$site  = 'C:\inetpub\wwwroot\PortablePOS'
$build = 'D:\PortablePOSBackend\StockSearch_WS\bin'

# 1. New service DLL + BarTender SDK
Copy-Item "$build\PortablePOS_WS.dll", "$build\PortablePOS_WS.pdb", "$build\PortablePOS_WS.xml", "$build\Seagull.BarTender.Print.dll" "$site\bin" -Force
Write-Host 'DLLs copied' -ForegroundColor Green

# 2. Print Label settings in the deployed Web.config (the rest of the file, connection string included, is kept)
$configPath = "$site\Web.config"
$config = [IO.File]::ReadAllText($configPath)
if ($config -notmatch 'LabelDesignsFolder') {
    $keys = "    <add key=""LabelDesignsFolder"" value=""C:\App\LabelDesigns""/>`r`n    <add key=""LabelPrinterName"" value=""""/>`r`n  </appSettings>"
    $config = ([regex]'\s*</appSettings>').Replace($config, "`r`n" + $keys, 1)
    [IO.File]::WriteAllText($configPath, $config)
    Write-Host 'Web.config updated' -ForegroundColor Green
} else {
    Write-Host 'Web.config already has the Print Label settings' -ForegroundColor Yellow
}

# 3. Restart the service
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:DefaultAppPool
& "$env:windir\system32\inetsrv\appcmd.exe" list apppool DefaultAppPool
