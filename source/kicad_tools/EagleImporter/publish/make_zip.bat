@echo off

rem 
rem make package zip
rem

set zip=pkzip.exe
set zzzname=eakit
set version=0_1_0_1
rem set flags=-silent
set flags=

echo Zipping files..
del %zzzname%.zip
%zip% -add %flags% %zzzname%.zip EagleImporter.application setup.exe "Application Files\EagleImporter_%version%\*.*"  -Path -recurse

echo Calculating sum..
CertUtil -hashfile %zzzname%.zip SHA256 >hash.txt




set zip=
set zzzname=
set flags=





 