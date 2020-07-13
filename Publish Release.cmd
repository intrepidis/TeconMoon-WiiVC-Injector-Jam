@echo off

set ASSEMBLY_NAME=TeconMoon WiiVC Injector Jam

set BASE_PATH=%~dp0

if "%VSCMD_VER%" neq "" goto :SETUP_ENVIRONMENT_DONE
:SETUP_ENVIRONMENT
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
:SETUP_ENVIRONMENT_DONE

cd "%BASE_PATH%"

:BUILD_RELEASE
@echo on
msbuild /p:Configuration=Release
@echo off

if %ERRORLEVEL% neq 0 goto :FINISH

:COPY_FILES
set PublishFolder=%BASE_PATH%Publish
rmdir /s /q "%PublishFolder%"
mkdir "%PublishFolder%"

:: Copy the app.
set BuildFolder=%BASE_PATH%%ASSEMBLY_NAME%\bin\Release
call :DO_COPY

goto :FINISH

:DO_COPY
@echo on
copy "%BuildFolder%\%ASSEMBLY_NAME%.exe" "%PublishFolder%\"
copy "%BuildFolder%\%ASSEMBLY_NAME%.exe.config" "%PublishFolder%\"
copy "%BuildFolder%\*.dll" "%PublishFolder%\"
@echo off
::
:START_CONTENT_COPY
if not exist "%BuildFolder%\Content" goto :END_CONTENT_COPY
@echo on
mkdir "%PublishFolder%\Content\"
copy "%BuildFolder%\Content\*.*" "%PublishFolder%\Content\"
@echo off
:END_CONTENT_COPY
::
goto :eof

:FINISH
pause
