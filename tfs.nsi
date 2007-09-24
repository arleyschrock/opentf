; tfs.nsi
;--------------------------------

!include "MUI.nsh"

!define OURNAME "OpenTF v0.5.1"

; The name of the installer
Name "${OURNAME}"

; The file to write
OutFile "opentf.exe"

; The default installation directory
InstallDir $PROGRAMFILES\Opentf

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
  File class\lib\net_2_0\Microsoft.TeamFoundation.Client.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.Common.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.VersionControl.Client.dll
  File class\lib\net_2_0\Microsoft.TeamFoundation.VersionControl.Common.dll
  File tools\tf\tf.exe

	; not sure of a good way to do this yet...
	File C:\Mono-1.2.4\lib\mono\2.0\Mono.GetOptions.dll

  WriteUninstaller "$INSTDIR\Uninstall.exe"  

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Opentf" "DisplayName" "Opentf (remove only)"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Opentf" "UninstallString" "$INSTDIR\Uninstall.exe"

SectionEnd ; end the section

Section "Uninstall"

  Delete "$INSTDIR\Microsoft.TeamFoundation.Client.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.Common.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.VersionControl.Client.dll"
  Delete "$INSTDIR\Microsoft.TeamFoundation.VersionControl.Common.dll"
  Delete "$INSTDIR\tf.exe"
  Delete "$INSTDIR\Uninstall.exe"
  Delete "$INSTDIR\Mono.GetOptions.dll"
  RMDir "$INSTDIR"

 DeleteRegKey HKEY_LOCAL_MACHINE "SOFTWARE\Opentf"
 DeleteRegKey HKEY_LOCAL_MACHINE "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Opentf"

SectionEnd

