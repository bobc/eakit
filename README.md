# eakit

A tool to convert EAGLE(tm) CAD projects to KiCad.


Status : alpha test

- Some significant features may be missing or incomplete
- Only limited testing has been performed

Limitations

- eakit is only supported on Windows 7 or later
- eakit can only read Eagle XML files, so Eagle projects must be saved in Eagle v6 or later 


Licensing

eakit is published under GPL v3
Copyright (c) 2017 Bob Cousins

Additional copyrights : 
https://github.com/opentk/opentk/blob/master/Documentation/License.txt
 

= Installation =

1. download a zipped copy of repository
2. unzip to local lfolder
3. unzip file release/eakit_x_x_x_x.zip
4. Run setup.exe

= Running eakit =

1. Start eakit
2. Select the Eagle project file (schematic or board). 
Note: the Eagle project must be saved in Eagle version 6 or later, eakit can only read Eagle XML files.
3. Select the output folder. It is strongly recommended this an empty folder, as any KiCad files will be overwritten.
4. Clock Convert button.
5. If the conversion is successful, a complete KiCad project is written to the output folder.


= What the conversion does =

converts schematic file to 
- set of component libraries
- set of footprint libraries 
- set of schematic sheets.

- adds no-connect flags to unconnected pins

converts board file to
- KiCad PCB file

== Post conversion notes =

KiCad is not exactly compatible with Eagle, since Eagle has some features that KiCad does not. eakit converts some items
to the nearest equivalent in KiCad

1. Reference designators in KiCad must end in a number
2. Layers

- not converted
                                                                                   





= Information for developers =           

eakit is built using Windows Visual Studio Community 2015.

Additional dependencies.

eakit uses Open Toolkit Library (OpenTK). This can be installed via NuGet package manager
                                         

= Trademarks =

EAGLE(tm) is a registered trademark of Autodesk Inc.
