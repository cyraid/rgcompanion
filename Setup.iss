#define MyAppName "Ruffle Game Companion"
#define MyAppVersion "1.1"
#define MyAppPublisher "Dennis Fehr"
#define MyAppExeName "RGCompanion.exe"
#define Macro8UserAppCfgDir "Macromedia\Flash 8\en\Configuration"
#define Macro8FirstRunDir "Macromedia\Flash 8\en\First Run"

[Setup]
AppId={{40C2183B-7380-4563-A4C6-5F01FD09EA33}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=bin\Inno
OutputBaseFilename=RGCompanionSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UsedUserAreasWarning=no
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net6.0-windows\publish\rgcompanion\*"; Excludes: "assets\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "assets\ruffle_game.exe"; DestDir: "{app}\assets"; Flags: ignoreversion
Source: "assets\extras\TestMovie\TestMovie.jsfl"; DestDir: "{commonpf32}\{#Macro8FirstRunDir}\Commands"; Flags: ignoreversion; Check: CheckMacro8Dir('Commands')
Source: "assets\extras\TestMovie\TestMovie.dll"; DestDir: "{commonpf32}\{#Macro8FirstRunDir}\External Libraries"; Flags: ignoreversion; Check: CheckMacro8Dir('External Libraries')
Source: "assets\extras\TestMovie\TestMovie.jsfl"; DestDir: "{localappdata}\{#Macro8UserAppCfgDir}\Commands"; Flags: ignoreversion; Check: CheckMacro8AppDir('Commands')
Source: "assets\extras\TestMovie\TestMovie.dll"; DestDir: "{localappdata}\{#Macro8UserAppCfgDir}\External Libraries"; Flags: ignoreversion; Check: CheckMacro8AppDir('External Libraries')

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
Function CheckMacro8Dir(Dir: String): Boolean;
Begin
  Result := DirExists(ExpandConstant('{commonpf32}') + '\' + ExpandConstant('{#Macro8FirstRunDir}') + '\' + Dir);
End; // Function //

Function CheckMacro8AppDir(Dir: String): Boolean;
Begin
  Result := DirExists(ExpandConstant('{localappdata}') + '\' + ExpandConstant('{#Macro8UserAppCfgDir}') + '\' + Dir);
End; // Function //
