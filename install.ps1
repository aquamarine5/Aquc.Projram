$zipDownloadUrl=''
$zipDownloadPath="$env:TEMP\AqucProjram7za.exe"
$pkgDownloadUrl=""
$pkgDownloadPath="$env:TEMP\AqucProjramPkg.zip"
Invoke-WebRequest -Uri $zipDownloadUrl -OutFile $zipDownloadPath -UseBasicParsing -Method Get
Invoke-WebRequest -Uri $pkgDownloadUrl -OutFile $pkgDownloadPath -UseBasicParsing -Method Get
[string[]]$zipargs="e",$pkgDownloadPath,"-aoa","-o""D:\Program Files\Aquc.Projram"""
Start-Process -FilePath $zipDownloadPath -ArgumentList $zipargs -Wait

Remove-Item -Path $zipDownloadPath
Remove-Item -Path $pkgDownloadPath