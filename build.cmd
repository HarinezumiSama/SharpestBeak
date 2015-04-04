@echo off
cls

set SB_MSBUILD=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set SB_CFG=%~1
set SB_PLATFORM=%~2
set SB_PROJECT=%~dp0\SharpestBeak.sln
set SB_LOG=%~f0.log

if /i "%SB_CFG%" equ "d" set SB_CFG=Debug
if /i "%SB_CFG%" equ "r" set SB_CFG=Release
if /i "%SB_CFG%" equ "" set SB_CFG=Debug
if /i "%SB_PLATFORM%" equ "" set SB_PLATFORM=Any CPU

echo ****************************************************************************************************
echo Building "%SB_PROJECT%" (%SB_CFG% / %SB_PLATFORM%)...
echo.

if exist "%SB_LOG%" del /f /q "%SB_LOG%"
echo Rebuilding "%SB_PROJECT%" (%SB_CFG% / %SB_PLATFORM%)... >>"%SB_LOG%"
date /t >>"%SB_LOG%"
time /t >>"%SB_LOG%"
echo. >>"%SB_LOG%"

echo * Cleaning...
echo * Cleaning... >>"%SB_LOG%"
call :DO_BUILD Clean
if errorlevel 1 goto ERROR
echo * Cleaning done.
echo * Cleaning done. >>"%SB_LOG%"
echo.

echo. >>"%SB_LOG%"
echo ******************** >>"%SB_LOG%"
echo. >>"%SB_LOG%"

echo * Rebuilding...
echo * Rebuilding... >>"%SB_LOG%"
call :DO_BUILD Rebuild
if errorlevel 1 goto ERROR
echo * Rebuilding done.
echo * Rebuilding done. >>"%SB_LOG%"
echo.

echo. >>"%SB_LOG%"
echo * Build complete >>"%SB_LOG%"
date /t >>"%SB_LOG%"
time /t >>"%SB_LOG%"
echo. >>"%SB_LOG%"

goto END

:DO_BUILD
"%SB_MSBUILD%" "%SB_PROJECT%" /t:%1 /p:Configuration="%SB_CFG%" /p:Platform="%SB_PLATFORM%" /nr:false /m /clp:verbosity=minimal /fl /flp:verbosity=normal;LogFile="%SB_LOG%";Append;Encoding=UTF-8
if errorlevel 1 exit /b 1
goto :EOF

:ERROR
echo.
pause
goto END

:END
set SB_MSBUILD=
set SB_CFG=
set SB_PLATFORM=
set SB_PROJECT=
set SB_LOG=
