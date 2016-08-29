( Layer 3 - "core2" other core functions )

<<<fonts>>>

compiletoflash

include ../flib/timer-stm32f1.fs
\ include ../flib/pwm-stm32f1.fs
\ include ../flib/adc-stm32f1.fs

\ these may be useful if we ever connect up
\ the second uart interface for data logging or
\ similar use
include ../flib/ring.fs
include ../flib/uart2-stm32f1.fs
include ../flib/uart2-irq-stm32f1.fs

\\ XXX \\ include ../flib/spi-stm32f1.fs
include spi-stm32f1.fs  \ include alternative SPI implementation that optimizes transmission only

include tft-ili9341.fs
include graphics.fs

include font.fs

\ initialize 
: init-35_core
    init     \ hook previous version
    ." [Pause..]"
    1500 ms key? if
	." (Common init aborted)"
    else
	cr
	tft-init
	3 ili9341-setRotation
	BLACK tft-bg !
	clear
	NAVY tft-bg ! WHITE tft-fg !  splashFluke splashFlukeX splashFlukeY 100 100 bitmap
	BLACK tft-bg !
	CYAN tft-fg !
	bmow8x16 fnt-select	s" Louis Mamamkos <louie@transsys.com>"  25 200 fnt-drawstring
	." [core2 Application Init]" cr  ." ok." cr
    then
;

: init
    init-35_core
;

cornerstone <<<core2>>>

