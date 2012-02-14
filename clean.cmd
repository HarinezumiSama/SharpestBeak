@echo off

echo.
echo ****************************************************************************************************
echo * Cleaning...
echo.

if /i "%~1" equ "/?" goto HELP
if /i "%~1" equ "-?" goto HELP

set SB_ASK_CLEANUP=1
if /i "%~1" equ "/q" set SB_ASK_CLEANUP=0
if /i "%~1" equ "-q" set SB_ASK_CLEANUP=0

set SB_DIR=
for %%A in ("%~f0\..") do set SB_DIR=%%~fA
if /i "%SB_DIR%" equ "" (
    echo * ERROR: Cannot resolve project directory!
    goto END
)

set SB_HG_EXE=hg.exe
set SB_HG=
for %%A in ("%SB_HG_EXE%") do set SB_HG=%%~f$PATH:A

if /i "%SB_ASK_CLEANUP%" neq "1" goto SKIP_ASK
choice /c 12 /m "Press 1 to continue; 2 to cancel" /t 30 /d 2
if %ERRORLEVEL% neq 1 (
    echo.
    echo * Cancelled.
    goto END
)
:SKIP_ASK

if /i "%SB_HG%" equ "" (
    echo * Cannot find '%SB_HG_EXE%'; falling back to simple cleanup...
    goto SIMPLE_CLEAN
)
if not exist "%SB_DIR%\.hg\*" (
    echo * Project is not under Mercurial control; falling back to simple cleanup...
    goto SIMPLE_CLEAN
)

echo * Using "%SB_HG%" for cleanup...
"%SB_HG%" purge --all --exclude *.suo --exclude "%~nx0" --verbose --repository "%SB_DIR%"
goto DONE

:SIMPLE_CLEAN
echo.
set SB_OUTPUT=%~dp0\_Output_
if exist "%SB_OUTPUT%\*" (
    echo * Deleting directory "%SB_OUTPUT%"...
    rd /s /q "%SB_OUTPUT%"
) else (
    if exist "%SB_OUTPUT%" (
        echo * Deleting file "%SB_OUTPUT%"...
        del /f /q "%SB_OUTPUT%"
    )
)
goto DONE

:HELP
echo.
echo Usage:
echo   "%~nx0" [/Q]
echo Options:
echo   /Q - quiet (no questions on purge when using HG)
goto END

:DONE
echo.
echo * Cleaning done.
goto END

:END
set SB_DIR=
set SB_HG_EXE=
set SB_HG=
set SB_OUTPUT=
set SB_ASK_CLEANUP=
set SB_CHOICE=

echo ****************************************************************************************************
echo.
