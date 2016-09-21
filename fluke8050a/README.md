Application code for the Fluke 8050A TFT LCD Display project.

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

