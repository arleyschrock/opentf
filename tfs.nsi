; tfs.nsi
;--------------------------------

!include "MUI.nsh"

!define OURNAME "OpenTF v0.6.0"

; The name of the installer
Name "${OURNAME}"

; The file to write
OutFile "opentf-0.6.0.exe"

; The default installation directory
InstallDir $PROGRAMFILES\OpenTF

;--------------------------------

; Pages

  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------

!insertmacro MUI_LANGUAGE "English"

; The stuff to install
Section "" ;No components page, name is not important

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
	File class\lib\net_2_0\Mono.GetOptions.dll
	File class\lib\net_2_0\OpenTF.Common.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.Client.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.Common.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.VersionControl.Client.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.VersionControl.Common.dll
  File tools\tfsbot\bin\tfsbot.exe
  File tools\tfsbot\tfsbot.exe.config
  File tools\opentf\bin\opentf.exe
  File /oname=ChangeLog.txt ChangeLog

  WriteUninstaller "$INSTDIR\Uninstall.exe"  

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenTF" "DisplayName" "OpenTF (remove only)"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenTF" "UninstallString" "$INSTDIR\Uninstall.exe"

SectionEnd ; end the section

Section "Uninstall"

  Delete "$INSTDIR\Microsoft.TeamFoundation.Client.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.Common.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.VersionControl.Client.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.VersionControl.Common.dll"
  Delete "$INSTDIR\tfsbot.exe"
  Delete "$INSTDIR\tfsbot.exe.config"
  Delete "$INSTDIR\opentf.exe"
  Delete "$INSTDIR\Uninstall.exe"
  Delete "$INSTDIR\Mono.GetOptions.dll"
  Delete "$INSTDIR\OpenTF.Common.dll"
	Delete "$INSTDIR\ChangeLog.txt"

  RMDir "$INSTDIR"

 DeleteRegKey HKEY_LOCAL_MACHINE "SOFTWARE\OpenTF"
 DeleteRegKey HKEY_LOCAL_MACHINE "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\OpenTF"

SectionEnd

