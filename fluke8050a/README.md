Application code for the Fluke 8050A TFT LCD Display project.

More information on this project on the
[Project's Wiki](https://madnessinthedarkness.transsys.com/projects:fluke8050a:start),
which is a work in progress.

The code layout presumes the use of a loader program to upload the
various Forth files to a running Forth interpreter on the STM32F103
microcontroller.  The uploader program (in ../upload.py) will perform
flow control by sending a line, then waiting for an **ok.** response
before proceeding with the next line.  It will also look for an
**include** directive used to send the contents of a named file.

The **load-all** file will reload all the code into a bare Forth
interpreter by erasing the saved Forth definitions in flash, then
successively load a series of "top-level" files, each of which 
contain some Forth code as well as other **include** directives.

These files are organized in such a way as to enable rapid
development, with relatively static definitions and libraries loaded
first (along with checkpoints) followed by somewhat more volatile
code.

The actual specific "application" code is contained within the
**40_app.fs** file.  During development, this can be loaded and
compiled into RAM rather than flash for very rapid development and
testing cycles.

This software and hardware configuration is targeted to a Maple Mini
"clone" board commonly available on EBay, with an STM32F103CBT6
device with 128KB of Flash and 64KB of RAM.  Deleting some of the
debugging code (notably the disassembler) will enable it to fit in
a smaller part with 64KB of Flash.

There is a binary file, named something like

  `fluke8050a/mecrisp-stellaris-2.2.8-stm32f103xB.bin`

which is an otherwise stock Mecrisp Forth interpreter built for the
STM32F103xx series devices, except rather than conservatively assuming
only 64K of flash (smallest available), it has been tweaked to assume
128K of flash available on the STM32F103xB devices.


