[Setup]
AppName=AI Image Chat
AppVersion=1.0.0
DefaultDirName={commonpf}\AI Image Chat
DefaultGroupName=AI Image Chat
OutputBaseFilename=AIImageChat_Setup
Compression=zip
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
UninstallDisplayIcon={app}\AIImageChat.exe

[Files]
; Arquivos principais
Source: "AIImageChat\bin\Release\net8.0-windows\win-x64\publish\AIImageChat.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "api_key.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\AI Image Chat"; Filename: "{app}\AIImageChat.exe"
Name: "{group}\Uninstall AI Image Chat"; Filename: "{uninstallexe}"
Name: "{commondesktop}\AI Image Chat"; Filename: "{app}\AIImageChat.exe"

[Run]
Filename: "{app}\AIImageChat.exe"; Description: "Launch AI Image Chat"; Flags: nowait postinstall skipifsilent
