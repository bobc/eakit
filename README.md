 eakit
=======

A tool to convert EAGLE(tm) CAD projects to KiCad.


Status : alpha test
-------------------

- Some significant features may be missing or incomplete
- Only limited testing has been performed

Limitations
-----------

- eakit is only supported on Windows 7 or later
- requires .NET 4
- eakit can only read Eagle XML files, so Eagle projects must be saved in Eagle v6 or later 


Licensing
---------

eakit is published under GPL v3 see LICENSE
Copyright (c) 2017 Bob Cousins

Additional copyrights : 
- OpenTK: https://github.com/opentk/opentk/blob/master/Documentation/License.txt
- LibTessDotNet: https://github.com/speps/LibTessDotNet/blob/master/LICENSE.txt
- NewStroke font data: http://vovanium.ru/sledy/newstroke/en
 

Installation
------------

1. Download latest version from https://github.com/bobc/eakit/releases
2. Unzip to local folder
  - hash_xxxx.txt contains an SHA256 hash of the release zip
3. Run setup.exe

Privacy
-------
eakit does the following:
- read/writes file locations on your PC that you specify at runtime
- stores settings in %APPDATA%\RMC\Eagle_Importer\Eagle_Importer.config.xml
  - position and size of main window, last source file name, last destination folder

eakit does not make any network connections 

Running eakit
-------------

1. Start eakit
2. Select the Eagle project file (schematic or board). *Note: the Eagle project must be saved in Eagle version 6 or later, eakit can only read Eagle XML files.*
3. Select the output folder. It is strongly recommended this an empty folder, as any KiCad files will be overwritten.
4. Click Convert button.
5. If the conversion is successful, a complete KiCad project is written to the output folder.


What the conversion does
------------------------

Converts schematic file to 
- set of component libraries
- set of footprint libraries 
- set of schematic sheets
- adds no-connect flags to unconnected pins
- fixes up references and footprints refs

Converts board file to
- KiCad PCB file
- fixes up references and footprints refs

Writes conversion report

Post conversion notes
---------------------

KiCad is not exactly compatible with Eagle, since Eagle has some features that KiCad does not. eakit converts some items
to the nearest equivalent in KiCad.

1. Reference designators in KiCad must end in a number
2. Layers

Features not supported
----------------------
                                                                                   
TODO




Information for developers
--------------------------           

eakit is built using Windows Visual Studio Community 2015.

Additional dependencies.

eakit uses: 

Open Toolkit Library (OpenTK). This can be installed via NuGet package manager

https://github.com/speps/LibTessDotNet is used via git submodule
                                         

Trademarks
----------

Eagle(tm) is a registered trademark of Autodesk Inc.
