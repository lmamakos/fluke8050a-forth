This contains the files used for the Forth-based alternative display
for a Fluke 8050A multimeter using a graphical 320x240 color TFT LCD
display.

For more information, please see https://madnessinthedarkness.transsys.com/fluke8050a-tft

Louis Mamakos
louis.mamakos@transsys.com
30 October 2016

Files in this repository:
.hg/                   - Mercurial metadata
.hgcheck/              - Mercurial metadata
.hgignore              - Mercurial metadata
.hgtags                - Mercurial metadata
flib/                  - Forth library files, unmodified from jeelabs
fluke8050a/            - Forth application files for Fluke 8050A multimeter
lib/                   - some potentiall reusble Forth libraries
mlib/                  - Forth library files, based on Mecrisp distribution
stm32loader.py         - Python file to load using STM32 serial bootloader
upload.py              - Python file used with SecureCRT to load Forth code

Join the chat at https://gitter.im/fluke8050a-forth/Lobby
