@echo off

rem 
rem make package zip
rem

set version=%1

set release_dir=..\..\..\..\release

set zip=pkzip.exe
set zzzname=eakit_%version%
rem set flags=-silent
set flags=

echo Zipping files..
del %zzzname%.zip
%zip% -add %flags% %zzzname%.zip .\EagleImporter.application setup.exe "Application Files\EagleImporter_%version%\*.*"  -Path

echo Calculating sum..
CertUtil -hashfile %zzzname%.zip SHA256 >hash_%version%.txt

copy %zzzname%.zip  %release_dir%
copy hash_%version%.txt %release_dir%

set zip=
set zzzname=
set flags=
 