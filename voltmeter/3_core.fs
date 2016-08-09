\ layer "C" - core hardware drivers and librarys

<<<board>>>

compiletoflash

include ../flib/timer-stm32f1.fs
\ include ../flib/pwm-stm32f1.fs
\ include ../flib/adc-stm32f1.fs
include ../flib/ring.fs
include ../flib/uart2-stm32f1.fs
include ../flib/uart2-irq-stm32f1.fs
include ../flib/spi-stm32f1.fs

include tft-ili9341.fs
include graphics.fs

( font rendering subroutines )
include font.fs

( include all the fonts )
( include fonts/8x16.fs )
include fonts/8x16.fs

( include fonts/digit_lg.fs )
include fonts/digit_lg.fs

( include fonts/digit_sm.fs )
include fonts/digit_sm.fs

( include fonts/digit_tiny.fs )
include fonts/digit_tiny.fs

( include fonts/digits32x64.fs )
include fonts/digits32x64.fs

( include fonts/symbolMode.fs )
include fonts/symbolMode.fs

( include fonts/symbolSign.fs )
include fonts/symbolSign.fs

( include fonts/symbolSplash.fs ) 
include fonts/symbolSplash.fs

( include fonts/symbolUnit.fs ) 
include fonts/symbolUnit.fs


cornerstone <<<core>>>
