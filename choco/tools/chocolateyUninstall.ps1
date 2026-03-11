$packageName = 'laborlogic.cyberslacker'
$installerType = 'msi'
# 这里的 GUID 必须和 Package.wxs 里的 UpgradeCode 一致
$silentArgs = '/qn /norestart'
Uninstall-ChocolateyPackage -PackageName $packageName -FileType $installerType -SilentArgs $silentArgs