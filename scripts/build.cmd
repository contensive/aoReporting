rem echo off

rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >build [versionNumber]
rem versionNumber is YY.MM.DD.build-number, like 20.5.8.1
rem

c:
cd \Git\aoReportingBasics\scripts

set solutionName=aoReporting
set collectionName=aoReportingBasics
set collectionPath=..\collections\aoReportingBasics\
set binPath=..\server\aoReportingCSharp\bin\debug\
set deploymentFolder=C:\Deployments\aoReportingBasics\Dev\
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\
set NuGetLocalPackagesFolder=C:\NuGetLocalPackages\


set year=%date:~12,4%
set month=%date:~4,2%
if %month% GEQ 10 goto monthOk
set month=%date:~5,1%
:monthOk
set day=%date:~7,2%
if %day% GEQ 10 goto dayOk
set day=%date:~8,1%
:dayOk
set versionMajor=%year%
set versionMinor=%month%
set versionBuild=%day%
set versionRevision=1
rem
rem if deployment folder exists, delete it and make directory
rem
:tryagain
set versionNumber=%versionMajor%.%versionMinor%.%versionBuild%.%versionRevision%
if not exist "%deploymentFolder%%versionNumber%" goto :makefolder
set /a versionRevision=%versionRevision%+1
goto tryagain
:makefolder
md "%deploymentFolder%%versionNumber%"

 

rem ==============================================================
rem

del %collectionPath%\*.dll
del %collectionPath%\%collectionName%.zip

 

rem ==============================================================
rem
echo build 
rem
cd ..\server

dotnet build aoReportingCSharp/aoReporting.csproj --configuration Debug  /property:Version=%versionNumber% /property:AssemblyVersion=%versionNumber% /property:FileVersion=%versionNumber%
:: "%msbuildLocation%msbuild.exe" %solutionName%
if errorlevel 1 (
   echo failure building
   pause
   exit /b %errorlevel%
)
cd ..\scripts



rem ==============================================================
rem
echo Build addon collection
rem

rem remove old DLL files from the collection folder
del "%collectionPath%"\*.DLL
del "%collectionPath%"\*.config

rem copy bin folder assemblies to collection folder
copy "%binPath%*.dll" "%collectionPath%"

rem create new collection zip file
c:
cd %collectionPath%
del "%collectionName%.zip" /Q
"c:\program files\7-zip\7z.exe" a "%collectionName%.zip"
xcopy "%collectionName%.zip" "%deploymentFolder%%versionNumber%" /Y
cd ..\..\scripts


