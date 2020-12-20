@echo off

::Note: INTREPIDIS_SOURCECODE_PATH should equal something like %USERPROFILE%\Desktop\Home\Coding
call "%INTREPIDIS_SOURCECODE_PATH%\Intrepidis Suite\Commands\cn_dev.bat"

set ASSEMBLY_NAME=TeconMoon WiiVC Injector Jam

set BASE_PATH=%~dp0
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
