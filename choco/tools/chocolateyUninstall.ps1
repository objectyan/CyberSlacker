$packageName = 'laborlogic.cyberslacker'
# 这里的名字必须和截图中的 DisplayName 完全一致
$softwareName = 'CyberSlacker (赛博摸鱼员)'

# 1. 扫描所有可能的卸载路径
$registryPaths = @(
    "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*"
)

# 2. 寻找匹配的项
# 我们同时匹配名字，并获取它的 PSChildName (即 ProductCode)
$installedApp = Get-ItemProperty $registryPaths -ErrorAction SilentlyContinue | 
                Where-Object { $_.DisplayName -eq $softwareName } | 
                Select-Object -First 1

if ($installedApp) {
    $productCode = $installedApp.PSChildName
    Write-Host "检测到已安装程序，正在通过 ProductCode [$productCode] 卸载..."
    
    $packageArgs = @{
        packageName   = $packageName
        fileType      = 'msi'
        file          = $productCode # 只要给 Choco 这个 GUID，它就能卸载
        silentArgs    = "/x $productCode /qn /norestart"
        validExitCodes= @(0, 1605, 3010)
    }

    Uninstall-ChocolateyPackage @packageArgs
} else {
    Write-Warning "未发现 $softwareName 的安装记录，跳过卸载。"
}