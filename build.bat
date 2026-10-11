@echo off
setlocal
cd /d "%~dp0"
title FontMaster Pro Compiler

echo ==============================================================================
echo                 Compiling FontMaster Pro (Smart Font Manager)
echo                 Using Windows Built-in .NET Framework Compiler
echo ==============================================================================
echo.

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %CSC% (
    set CSC="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not exist %CSC% (
    echo [ERROR] csc.exe was not found in Microsoft.NET Framework directories!
    pause
    exit /b 1
)

echo [1/2] Checking app icon (app.ico)...
if not exist "app.ico" (
    echo [INFO] Generating app.ico...
    %CSC% /target:exe /out:IconGenerator.exe IconGenerator.cs > nul 2>&1
    IconGenerator.exe
    if exist IconGenerator.exe del IconGenerator.exe > nul 2>&1
)

echo [2/2] Compiling FontMaster.exe...
%CSC% /target:winexe /out:FontMaster.exe /win32icon:app.ico /optimize+ /platform:anycpu /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll Program.cs FontModels.cs FontScanner.cs FontInstaller.cs ThemeManager.cs Localization.cs SystemFontsViewerControl.cs HtmlReportGenerator.cs MainForm.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ==============================================================================
    echo [SUCCESS] FontMaster.exe built successfully!
    echo ==============================================================================
) else (
    echo.
    echo [FAILED] Compilation failed!
)

echo.
pause
