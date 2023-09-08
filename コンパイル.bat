@ECHO OFF

SETLOCAL

SET OBJ=IconMaker.exe

TASKKILL /F /IM %OBJ%

SET EXE="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
SET REF="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF"

SET TYP=/t:winexe
SET ICO=-win32icon:app.ico

SET DLL=/r:%REF%\PresentationCore.dll
SET DLL=%DLL% /r:%REF%\PresentationFramework.dll
SET DLL=%DLL% /r:%REF%\WindowsBase.dll

SET RES=
REM SET RES=%RES% /res:imgs/sea_mine.png
REM SET RES=%RES% /res:imgs/battleship.png,battleship


SET OUT=/out:%OBJ%

%EXE% %ICO% %RES% %TYP% %OUT% %DLL% %RES% AssemblyInfo.cs IconMaker.cs

IF ERRORLEVEL 1 (
  ECHO  ***********************
  ECHO  *** コンパイルエラー ***
  ECHO  ***********************
  PAUSE
)
REM PAUSE