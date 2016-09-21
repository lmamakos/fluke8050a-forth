\ -*- mode: forth; indent-tabs-mode: nil; -*-

\ layer "A" - base definitions always present

compiletoflash

\ the following 3 files are handy to have, but not required.
\ Including them probably pushes the size of flash needed over 64K.
\ The disassembler, in particular, takes up quite a bit of space but
\ is handy to have around.
\
\ If you include these and need support more than 64K of flash, you'll
\ need a custom-built version of mecrisp Forth with a definition
\ changed and the code all re-assembled.
include ../mlib/multi.fs
include ../lib/disassembler-m3.fs
include ../mlib/hexdump.fs

\ not happy about needing this here, but debugging code in io-stm32f1.fs wants to know
4 constant io-ports  \ A..D

include ../flib/io-stm32f1.fs
include ../flib/hal-stm32f1.fs

\ define some hardware constants specific to the Maple Mini SBC
PB1 constant LED
PB8 constant maple-mini-button

PB0 constant TFT-D/C 
\ PA4 is default SSEL for LCD display in SPI driver

PB2  constant fluke_dp       \ decimal point signal to Fluke LCD
PA8  constant fluke_hv       \ "High Voltage" signal to Fluke LCD

PC13 constant fluke_func_a   \  AC/DC
PC14 constant fluke_func_b   \  V
PC15 constant fluke_func_c   \  mA
PB10 constant fluke_func_d   \  relative mode switch

PB7  constant fluke_w        \ MSB
PB6  constant fluke_x        \
PB4  constant fluke_y        \
PB3  constant fluke_z        \ LSB

PA15 constant fluke_rng_a    \ range button state
PA14 constant fluke_rng_b
PA13 constant fluke_rng_c

PB15 constant fluke_st0      \ Fluke LCD display digit strobes
PB14 constant fluke_st1
PB13 constant fluke_st2
PB12 constant fluke_st3
PB11 constant fluke_st4


\ generate bit-band address given bit and offset into bitband region
: _io>bb ( bit offset -- addr )
    swap dup    ( offset bit bit )
    io-base rot +
    $000fffff and 5 lshift  \ offset into bitband region for register
    swap io# 2 lshift +     \ bit offset
    $42000000 +             \ base of bitband region
;

\ there are a few things we need to do that are very timing sensitive.
\ These words allow the code to access to GPIO signals by doing word
\ read/write access for each pin to the "bit-band" address region
\ defined in the Cortex-M3 address space.  No shifting/mask/and/or
\ operations needed, just a single memory access.  This takes about
\ 100ns rather than going through a series of forth words to translate
\ an abstract "pin" value to I/O port register and bit mask to do it
\ the hard way..
\
\ generate ARM Cortex bit-band addresses corresponding to bit position
\ in GPIO input register
: ior>bb  ( pin -- addr ) GPIO.IDR _io>bb ;

\ generate ARM Cortex bit-band addresses corresponding to bit position
\ in GPIO output register
: iow>bb  ( pin -- addr ) GPIO.ODR _io>bb ;

fluke_st0   ior>bb constant st0_bb
fluke_st1   ior>bb constant st1_bb
fluke_st2   ior>bb constant st2_bb
fluke_st3   ior>bb constant st3_bb
fluke_st4   ior>bb constant st4_bb
fluke_w     ior>bb constant w_bb
fluke_x     ior>bb constant x_bb
fluke_y     ior>bb constant y_bb
fluke_z     ior>bb constant z_bb
fluke_dp    ior>bb constant dp_bb
fluke_hv    ior>bb constant hv_bb
fluke_rng_a ior>bb constant rng_a_bb
fluke_rng_b ior>bb constant rng_b_bb
fluke_rng_c ior>bb constant rng_c_bb

\ fast LED on/off words, possibly useful for timing experiments.  Use
\ bit-band acces to speed up operations.  Changing state of LED takes
\ about 100ns with inlined word definition, which expands to something
\ like this:
\  000131BA: F248  movw r0 #8184
\  000131BC: 1084
\  000131BE: F2C4  movt r0 #4221
\  000131C0: 2021
\  000131C2: 2101  movs r1 #1
\  000131C4: 6001  str r1 [ r0 #0 ]
\
LED         iow>bb constant LED_bbw
: led-on   1 LED_bbw ! inline ;
: led-off  0 LED_bbw ! inline ;


: fl-input ( pin -- )
  IMODE-FLOAT swap io-mode!
;

: button? (  )   maple-mini-button io@ 0<> ; \ check if button on maple mini is pressed

: init-fluke-inputs
  fluke_func_a fl-input   fluke_w fl-input    fluke_rng_a fl-input    fluke_st0 fl-input   fluke_dp fl-input
  fluke_func_b fl-input   fluke_x fl-input    fluke_rng_b fl-input    fluke_st1 fl-input   fluke_hv fl-input
  fluke_func_c fl-input   fluke_y fl-input    fluke_rng_c fl-input    fluke_st2 fl-input
  fluke_func_d fl-input   fluke_z fl-input                            fluke_st3 fl-input
;

: init ( -- )  \ board initialisation
  -jtag  \ disable JTAG, we only need SWD
  OMODE-PP LED io-mode!     LED ios!   \ turn on LED
  72MHz
  flash-kb . ." KB <voltmeter> " hwid hex. cr
  init-fluke-inputs
  1000 systick-hz
;

cornerstone <<<always>>>
