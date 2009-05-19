[Setup]
AppName=VataviaMap
AppVerName=VataviaMap-2009-05-19
DefaultDirName={pf}\VataviaMap
DefaultGroupName=VataviaMap
UninstallDisplayIcon={app}\VataviaMap.exe
Compression=lzma
SolidCompression=yes
OutputDir=.
OutputBaseFilename=VataviaMapDesktopInstaller-2009-05-19
ChangesAssociations=yes
SetupIconFile=..\Images\osm.ico
WizardSmallImageFile=..\Images\osm.bmp
WizardImageFile=..\Images\osm135.bmp

[Files]
Source: "..\bin\Release\VataviaMapDesktop.exe"; DestDir: "{app}"
;Source: "VataviaMap-Desktop.chm"; DestDir: "{app}"
;Source: "Readme.txt"; DestDir: "{app}"; Flags: isreadme

[Icons]
Name: "{group}\My Program"; Filename: "{app}\VataviaMapDesktop.exe"

[Components]
Name: "program"; Description: "VataviaMap Desktop Application"; Flags: fixed
Name: "gpx";     Description: "Associate .gpx files"

[Registry]
Root: HKCR; Subkey: ".gpx";                   ValueType: string; ValueName: ""; ValueData: "GPX";          Flags: uninsdeletevalue; Components: gpx
Root: HKCR; Subkey: "GPX";                    ValueType: string; ValueName: ""; ValueData: "GPS Exchange"; Flags: uninsdeletekey;   Components: gpx
Root: HKCR; Subkey: "GPX\DefaultIcon";        ValueType: string; ValueName: ""; ValueData: "{app}\VataviaMapDesktop.exe,0";                Components: gpx
Root: HKCR; Subkey: "GPX\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\VataviaMapDesktop.exe"" ""%1""";       Components: gpx
