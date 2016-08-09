\ Layer 3 - "core2" other core functions

<<<core>>>

compiletoflash


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
	YELLOW tft-bg !   BLACK tft-fg !
	splashFluke splashFlukeX splashFlukeY 100 100 bitmap
	BLACK tft-bg !
	CYAN tft-fg !
	." Application Init" cr
    then
;

cornerstone <<<core2>>>

