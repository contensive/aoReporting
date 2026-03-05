
rem all paths are relative to the git scripts folder

set appName=veronica

rem run the script in the same directory where the current script is located
call "%~dp0build.cmd"

rem upload to contensive application
c:
cd %collectionPath%
cc -a %appName% --installFile "%collectionName%.zip"
cd ..\..\scripts
