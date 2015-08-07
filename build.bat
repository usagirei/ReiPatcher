@echo off

set MSBUILD=C:\Program Files (x86)\MSBuild\14.0\Bin
set GIT=%PROGRAMFILES(x86)%\Git\bin
set BUILD=%CD%\.build

set PATH=%PATH%;%MSBUILD%;%GIT%;%BUILD%

if not exist "%BUILD%" (
	mkdir %BUILD%
)
if not exist "%BUILD%\nuget.exe" (
	echo ---------- Installing NuGet
	powershell -command "(new-object System.Net.WebClient).DownloadFile('https://nuget.org/nuget.exe','%BUILD%\nuget.exe')"
)
if not exist "%MSBUILD%\msbuild.exe" (
	echo MSBuild Not Found, Aborting
	goto end
)

echo ---------- Restoring Packages
REM nuget.exe update -Self
nuget.exe install Source\packages.config -OutputDirectory packages
echo ---------- Building Solution
msbuild.exe Source\.build
:end

pause