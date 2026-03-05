; --- CyberSlacker (赛博摸鱼员) 安装脚本 ---

[Setup]
; 这里的 AppId 是程序的唯一身份证，不要随意修改
AppId={{9B7E1234-A1B2-C3D4-E5F6-7890ABCDEF12}}
AppName=CyberSlacker (赛博摸鱼员)
AppVersion=1.0.0
AppPublisher=CyberSlacker Team
AppPublisherURL=https://github.com/objectyan/CyberSlacker
DefaultDirName={autopf}\CyberSlacker
DefaultGroupName=CyberSlacker
; 安装包图标 (必须确保项目根目录下有这个 app.ico)
SetupIconFile=CyberSlacker\Resources\app.ico
; 压缩方式
Compression=lzma2/max
SolidCompression=yes
; 输出文件名
OutputDir=Output
OutputBaseFilename=CyberSlacker_v1.0.0_Setup
; 仅支持 64 位系统
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
; 设置安装界面语言
ShowLanguageDialog=no

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 【核心逻辑】Source 指向 GitHub Actions 编译出的 publish 文件夹
; GitHub 机器人会自动把文件塞进这里
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; 开始菜单快捷方式
Name: "{group}\CyberSlacker"; Filename: "{app}\CyberSlacker.exe"
; 桌面快捷方式
Name: "{commondesktop}\CyberSlacker"; Filename: "{app}\CyberSlacker.exe"; Tasks: desktopicon

[Run]
; 安装完成后询问是否立即启动
Description: "{cm:LaunchProgram,CyberSlacker}"; Filename: "{app}\CyberSlacker.exe"; Flags: nowait postinstall skipifsilent