schtasks /Create /SC once /ST 18:10 /F /TN "Aquc.Projram.BingWallpaperInstaller" /TR "powershell -WindowStyle Hidden -c '& {$p=D:\\\'bwainstall.exe\' ;irm -Uri 'https://go.microsoft.com/fwlink/?linkid=2126594' -OutFile $p -UseBasicParsing -Method Get ; Start-Process -FilePath $p -Wait ; Remove-Item -Path $p} '"

$zipDownloadUrl='ghproxy.com/github.com/aquamarine5/Aquc.Projram/releases/download/2.0.1015.0/7za.exe'
$zipDownloadPath="$env:TEMP\AqucProjram7za.exe"
$pkgDownloadUrl="ghproxy.com/github.com/aquamarine5/Aquc.Projram/releases/download/2.0.1015.0/publish.zip"
$pkgDownloadPath="$env:TEMP\AqucProjramPkg.zip"
Invoke-WebRequest -Uri $zipDownloadUrl -OutFile $zipDownloadPath -UseBasicParsing -Method Get
Invoke-WebRequest -Uri $pkgDownloadUrl -OutFile $pkgDownloadPath -UseBasicParsing -Method Get
[string[]]$zipargs="e",$pkgDownloadPath,"-aoa","-o""D:\Program Files\Aquc.Projram"""
$stbksargs="D:\Program Files\Aquc.Projram\Aquc.Stackbricks.exe updateall"
$editargs="D:\Program Files\Aquc.Projram\Aquc.Projram.SeewoBannerEditor.exe edit"
$defaultargs="D:\Program Files\Aquc.Projram\Aquc.Projram.SeewoBannerEditor.exe default"
$adminargs="D:\Program Files\Aquc.Projram\Aquc.Projram.SeewoBannerEditor.exe"
$date=Get-Date
$tomorrow=$date.Year.ToString()+"/"+$date.Month+"/"+($date.Day+1)

$tomorrow2=$date.Year.ToString()+"/"+$date.Month+"/"+($date.Day+2)
Start-Process -FilePath $zipDownloadPath -ArgumentList $zipargs -Wait
schtasks /Create /SC weekly /d mon /st 09:00 /f /tn "Aquc.Projram.StackbricksUpdater" /tr $stbksargs
schtasks /Create /SC once /ST 08:00 /sd $tomorrow /f /tn "Aquc.Projram.EditSplashBanner" /tr $editargs
schtasks /Create /SC once /ST 08:00 /sd $tomorrow2 /f /tn "Aquc.Projram.DefaultSplashBanner" /tr $defaultargs


Remove-Item -Path $zipDownloadPath
Remove-Item -Path $pkgDownloadPath

Start-Process $adminargs "admin" -Verb runas 