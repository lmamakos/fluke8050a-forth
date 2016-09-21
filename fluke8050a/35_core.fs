\ -*- mode: forth; indent-tabs-mode: nil; -*-
( Layer 3 - "core2" other core functions )

<<<fonts>>>

compiletoflash

include ../flib/timer-stm32f1.fs
\ these are as yet unused.  was some though of using PWM output to
\ control backlight of LCD display, but nothing done yet (if even possible)
\ include ../flib/pwm-stm32f1.fs
\ include ../flib/adc-stm32f1.fs

\ these may be useful if we ever connect up
\ the second uart interface for data logging or
\ similar use.  May want to omit for smaller 64K flash size.
include ../flib/ring.fs
include ../flib/uart2-stm32f1.fs
include ../flib/uart2-irq-stm32f1.fs

\ include ../flib/spi-stm32f1.fs
include spi-stm32f1.fs  \ include alternative SPI implementation that optimizes transmission-only case
include tft-ili9341.fs
include graphics.fs
include font.fs

\ initialize 
: init-35_core
    init     \ hook previous version
    cr
    tft-init
    1 ili9341-setRotation  ( set display rotation to reflect mechanical orientation when mounted )
    BLACK tft-bg !
    clear
    NAVY tft-bg ! WHITE tft-fg !  splashFluke splashFlukeX splashFlukeY 100 100 bitmap
    BLACK tft-bg ! CYAN tft-fg !
    bmow8x16 fnt-select	s" Louis Mamamkos <louie@transsys.com>"  25 200 fnt-drawstring
    ." [core2 Application Init]" cr  ." ok." cr
;

: init
    init-35_core
;

cornerstone <<<core2>>>

