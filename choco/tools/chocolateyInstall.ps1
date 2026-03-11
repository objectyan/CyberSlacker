# 1. 变量准备 
$version = '0.0.1'
$baseUrl = "https://github.com/objectyan/CyberSlacker/releases/download/v$version/CyberSlacker_$version"

# 2. 定义三架构下载地址
$url32  = "$baseUrl`_x86.msi"
$url64  = "$baseUrl`_x64.msi"
$urlArm = "$baseUrl`_arm64.msi"

# 3. 【核心优化】动态判定 64 位槽位该给谁
# 默认给 x64
$finalUrl64 = $url64

# 检测当前是否为 ARM64 架构
$isArm64 = $env:PROCESSOR_ARCHITECTURE -eq 'ARM64' -or $env:PROCESSOR_ARCHITEW6432 -eq 'ARM64'

if ($isArm64) {
    Write-Host "检测到 ARM64 架构，将下载原生 ARM 版 MSI..."
    $finalUrl64 = $urlArm
}

# 4. 配置安装参数
$packageArgs = @{
  packageName   = 'laborlogic.cyberslacker'
  fileType      = 'msi'
  
  # 如果是 32 位系统装 x86，如果是 64 位系统(含ARM)装对应的 64 位包
  url           = $url32
  url64bit      = $finalUrl64
  
  silentArgs    = "/qn /norestart"
  validExitCodes= @(0, 3010, 1641)
}

# 5. 执行安装
Install-ChocolateyPackage @packageArgs