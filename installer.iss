; --- CyberSlacker (赛博摸鱼员) 安装脚本 ---
#define AppName "CyberSlacker"
#define AppShortName "赛博摸鱼员"
; 接收来自 GitHub Actions 的变量
#ifndef AppArch
  #define AppArch "x64"
#endif


[Setup]
; 这里的 AppId 是程序的唯一身份证，不要随意修改
AppId={{9B7E1234-A1B2-C3D4-E5F6-7890ABCDEF12}}
AppName={#AppName} ({#AppShortName})
AppVersion={#AppVersion}
AppPublisher=Object Yan
AppPublisherURL=https://github.com/objectyan/CyberSlacker
DefaultDirName={localappdata}\{#AppName}
DefaultGroupName={#AppName}
; 安装包图标 (必须确保项目根目录下有这个 app.ico)
SetupIconFile=CyberSlacker\Resources\app.ico
; 压缩方式
Compression=lzma2/max
SolidCompression=yes
PrivilegesRequired=lowest
; 输出文件名
OutputBaseFilename={#AppName}_v{#AppVersion}_{#AppArch}_Setup
; 架构限制
#if AppArch == "x86"
  ArchitecturesAllowed=x86
#else
  ArchitecturesAllowed=x64 arm64
  ArchitecturesInstallIn64BitMode=x64 arm64
#endif

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 【核心逻辑】Source 指向 GitHub Actions 编译出的 publish 文件夹
; GitHub 机器人会自动把文件塞进这里
Source: "publish-{#AppArch}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; 开始菜单快捷方式
Name: "{userprograms}\{#AppShortName}"; Filename: "{app}\{#AppName}.exe"
; 桌面快捷方式
Name: "{userdesktop}\{#AppShortName}"; Filename: "{app}\{#AppName}.exe"; Tasks: desktopicon

[Run]
; 安装完成后询问是否立即启动
Filename: "{app}\{#AppName}.exe"; Description: "{cm:LaunchProgram,{#AppShortName}}"; Flags: nowait postinstall skipifsilent
