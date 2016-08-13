( Layer 3 - "core2" other core functions )

<<<fonts>>>

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

include font.fs

\ initialize 
: init
    init     \ hook previous version
    1000 ms key? if
	." (Common init aborted)"
    else
	tft-init
	3 ili9341-setRotation
	BLACK tft-bg !
	clear
	NAVY tft-bg ! WHITE tft-fg !  splashFluke splashFlukeX splashFlukeY 100 100 bitmap
	BLACK tft-bg !
	CYAN tft-fg !
	." [core2 Application Init]" cr   ." ok." cr
    then
;

cornerstone <<<core2>>>

