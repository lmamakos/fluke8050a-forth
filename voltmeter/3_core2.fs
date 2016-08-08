\ Layer 3 - "core2" other core functions

<<<core>>>

compiletoflash

include tft-ili9341.fs
include graphics.fs

\ initialize 
: init
    init     \ hook previous version
    1000 ms key? if
	." (Common init aborted)"
    else
	tft-init
	3 ili9341-setRotation
	clear
	NAVY tft-bg !   CYAN tft-fg !
	splashFluke splashFlukeX splashFlukeY 100 100 bitmap

	." Application Init" cr
    then
;

cornerstone <<<core2>>>

