$tools = Split-Path $MyInvocation.MyCommand.Definition
$version = '0.0.1' # GitHub Actions 会替换这个

# 定义三架构下载地址
$url64 = "https://github.com/objectyan/CyberSlacker/releases/download/v$version/CyberSlacker_${version}_x64.msi"
$url32 = "https://github.com/objectyan/CyberSlacker/releases/download/v$version/CyberSlacker_${version}_x86.msi"
$urlArm = "https://github.com/objectyan/CyberSlacker/releases/download/v$version/CyberSlacker_${version}_arm64.msi"

$packageArgs = @{
  packageName   = 'laborlogic.cyberslacker'
  fileType      = 'msi'
  # 根据系统架构自动选包
  url           = $url32
  url64bit      = $url64
  # 如果是 ARM 机器，Choco 会尝试用 64 位包，我们这里可以做更精细的判断
  silentArgs    = "/qn /norestart"
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
