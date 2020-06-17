; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Crossout Log Viwer"
#define MyAppVersion "0.1.1"
#define MyAppPublisher "Prophet Lamb"
#define MyAppURL "https://github.com/ProphetLamb-Organistion/CrossoutLogViewer"
#define MyAppExeName "CrossoutLogViewer.GUI.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{FE8512CD-0A5D-44A9-8EE1-A9C3112D5FAE}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; The [Icons] "quicklaunchicon" entry uses {userappdata} but its [Tasks] entry has a proper IsAdminInstallMode Check.
UsedUserAreasWarning=no
LicenseFile=.\LICENSE.rtf
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=.\
OutputBaseFilename=Crossout-Log-Viewer_Setup
SetupIconFile=.\CrossoutLogViewer.GUI\App.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Dirs]      
Name: "{app}"; Permissions: everyone-full
Name: "{app}\config"; Permissions: everyone-full
Name: "{app}\images"; Permissions: everyone-full
Name: "{app}\data"; Permissions: everyone-full

[Files]
Source: ".\publish\CrossoutLogViewer.GUI.exe"; DestDir: "{app}"; Flags: ignoreversion; Permissions: everyone-full
Source: ".\publish\*"; DestDir: "{app}"; Flags: ignoreversion; Permissions: everyone-full
Source: ".\publish\images\*"; DestDir: "{app}\images"; Flags: ignoreversion; Permissions: everyone-full
Source: ".\publish\config\assets.json"; DestDir: "{app}\config"; Flags: ignoreversion; Permissions: everyone-full
Source: ".\publish\config\maps.json"; DestDir: "{app}\config"; Flags: ignoreversion; Permissions: everyone-full
Source: ".\publish\config\NLog.config"; DestDir: "{app}\config"; Flags: ignoreversion; Permissions: everyone-full
Source: ".\publish\config\stripes.json"; DestDir: "{app}\config"; Flags: ignoreversion; Permissions: everyone-full
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
