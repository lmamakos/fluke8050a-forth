( layer "D" - development code )
reset

compiletoram

: blink ( -- )  \ blink the on-board LED until a key is pressed
  OMODE-PP LED io-mode!  begin  LED iox!  100 ms  key? until ;

\ -----------------------------------------------------------------------------

: keyloop begin dup 1000 ms execute key? until drop ;

: func.
    fluke_func_a io@ ." FA=" . 32 emit
    fluke_func_b io@ ." FB=" . 32 emit
    fluke_func_c io@ ." FC=" . 32 emit
    fluke_func_d io@ ." FD=" . cr
;

: range.
    fluke_rng_a io@ ." RNGA=" . 32 emit
    fluke_rng_b io@ ." RNGB=" . 32 emit
    fluke_rng_c io@ ." RNGC=" . 32 emit
;

\ most significant digit is either 'blank' for 0 value or 1 for 1 value
\ rest of digits are value 15 for blank

\ detect overrange configuration, with only leading "1" digit and
\ 4 following blank digits  -- blank digits are apparently binary 15

\ detect improper switch settings wit 4 decimal points and no digit segments

\ polarity sign disabled for Vac, mA and k-ohm functions.

\ detect conductance switch configuration (k-ohm,200,2k, 2000k, 20M)
\ detect diode test - k-ohm, 2, 200, 20M)

\ detect dB with impedence switching mode

0 variable strobe-loss
0 variable d0  \ 8 - minus, 4 - plus, 2 - dB, 1 - 1
0 variable d1  \ BCD 
0 variable d2  \ BCD
0 variable d3  \ BCD
0 variable d4  \ BCD
0 variable hv
0 variable fluke_func
0 variable fluke_range

: getswitches  ( -- )
    fluke_func_a io@                \ FA 0=DC, 1=AC
    fluke_func_b io@       shl +    \ 
    fluke_func_c io@  2 lshift +
    fluke_func_d io@  3 lshift +
    fluke_func !

    fluke_rng_a  io@
    fluke_rng_b  io@       shl +
    fluke_rng_c  io@  2 lshift +
    fluke_range !
;

: getdigit ( io variable -- )
    swap  ( variable io )
    dup  ( variable io io )
    begin
	io@ if
	    ( variable io )
	    fluke_z io@                     \ bit 0  ( variable io sum )
	    fluke_y io@       shl +         \ bit 1  ( variable io sum )
	    fluke_x io@  2 lshift +         \ bit 2  ( variable io sum )
	    fluke_w io@  3 lshift +         \ bit 3  ( variable io sum )
	    fluke_dp io@ 8 lshift +         \ bit 4  ( decimal point )
	    fluke_hv io@ hv !
	    swap  ( variable sum io )
	    io@ 0= if
		1 strobe-loss +!
	    then
	    ( variable sum )
	    swap !
	    0
	else
	    ( variable io )
	    dup dup ( variable io io )
	then
    0= until	
;


0 variable strobe-mask

: check-strobe ( io variable -- detected? )
    swap  ( variable io )
    dup  ( variable io io )
    io@ if
	( variable io )
	fluke_z io@                     \ bit 0  ( variable io sum )
	fluke_y io@       shl +         \ bit 1  ( variable io sum )
	fluke_x io@  2 lshift +         \ bit 2  ( variable io sum )
	fluke_w io@  3 lshift +         \ bit 3  ( variable io sum )
	fluke_dp io@ 8 lshift +         \ bit 4  ( decimal point )
	fluke_hv io@ hv !
	swap  ( variable sum io )
	io@ 0= if
	    1 strobe-loss +!
	    2drop
	    false
	else
	    ( variable sum )
	    swap !
	    true
	then
    else
	2drop  ( variable io )
	false
    then
;

\ generate ARM Cortex bit-band addresses corresponding to bit position in GPIO registers
: io>bb  ( pin -- addr )
    dup
    io-base GPIO.IDR +   \ get GPIOx_IDR
    $000fffff and 5 lshift  \ offset into bitband region for register
    swap io# 2 lshift +       \ bit offset
    $42000000 +          \ base of bitband region
;

fluke_st0 io>bb constant st0bb
fluke_st1 io>bb constant st1bb
fluke_st2 io>bb constant st2bb
fluke_st3 io>bb constant st3bb
fluke_st4 io>bb constant st4bb
fluke_w   io>bb constant wbb
fluke_x   io>bb constant xbb
fluke_y   io>bb constant ybb
fluke_z   io>bb constant zbb
fluke_dp  io>bb constant dpbb
fluke_hv  io>bb constant hvbb

: getsigs
    zbb @                     \ bit 0  ( variable io sum )
    ybb @       shl +         \ bit 1  ( variable io sum )
    xbb @  2 lshift +         \ bit 2  ( variable io sum )
    wbb @  3 lshift +         \ bit 3  ( variable io sum )
    dpbb @ 8 lshift +         \ bit 4  ( decimal point )
    hvbb @       hv !         \ high voltage indicator
;
    
: chkstrobes
    st0bb @ if
	getsigs
	st0bb @ 0= if 1 strobe-loss +! then
	d0 !
	$01 strobe-mask @ or strobe-mask !
    then
    st1bb @ if
	getsigs
	st1bb @ 0= if 1 strobe-loss +! then
	d1 !
	$02 strobe-mask @ or strobe-mask !
    then
    st2bb @ if
	getsigs
	st2bb @ 0= if 1 strobe-loss +! then
	d2 !
	$04 strobe-mask @ or strobe-mask !
    then
    st3bb @ if
	getsigs
	st3bb @ 0= if 1 strobe-loss +! then
	d3 !
	$08 strobe-mask @ or strobe-mask !
    then
    st4bb @ if
	getsigs
	st4bb @ 0= if 1 strobe-loss +! then
	d4 !
	$10 strobe-mask @ or strobe-mask !
    then
;

    
: getstrobes
    0 strobe-mask !
    begin
	chkstrobes
	strobe-mask @ $1f = key? or
    until
;

millis variable last-strobe-time
: test-strobes
    millis last-strobe-time !
    begin
	getstrobes
	millis dup last-strobe-time @ - .
	last-strobe-time !
	key?
    until
;

: parse-display
;

: drawdigit
    15 and [char] 0 + fnt-drawchar
;


: disp-scan
    getstrobes
\    fluke_st0 d0 getdigit
\    fluke_st1 d1 getdigit
\    fluke_st2 d2 getdigit
\    fluke_st3 d3 getdigit
\    fluke_st4 d4 getdigit
    getswitches
    2 2 fnt-goto
    digit_lg fnt-select
    
    d0 @ 1 and drawdigit
    d1 @ drawdigit
    d2 @ drawdigit
    d3 @ drawdigit
    d4 @ drawdigit
;


( premature optimization follows :-)
: inl>spi> ( c -- c )  \ hardware SPI, 8 bits
  SPI1-DR !  begin SPI1-SR @ 1 and until  SPI1-DR @ inline ;
: inl>spi ( c -- ) inl>spi> drop inline ;  \ write byte to SPI
: inlh>tft ( u -- )
    \ write half-word (16 bits) to LCD,  assumes TFT-D/C is already set
    dup 8 rshift inl>spi  inl>spi ; 
: inlclear
    0 0 ili9341_width @ 1- ili9341_height @ 1- setwindow
    tft-bg @
    +spi
    ili9341_width @ ili9341_height @ * 0 do
	dup inlh>tft
    loop
    -spi
    drop \ the background color
;

: fluke-display
    begin
	millis last-strobe-time !
	2 2 fnt-goto
	$97ef tft-fg !
	disp-scan

	BLUE tft-fg !
	millis last-strobe-time @ - 0 <# # # # # # #s #>  2 fnt-height @ 2 +  fnt-drawstring

	key?
    until
;

  0 constant x-min
319 constant x-max
  0 constant y-min
239 constant y-max

: draw-frame
    x-min y-min
    x-max y-min 1+    fillrectfg  ( upper-left to upper right)

    x-max 1-  y-min             
    x-max     y-max   fillrectfg

    x-min     y-max 1-           
    x-max     y-max   fillrectfg

    x-min     y-min
    x-min 1+  y-max 1-  fillrectfg
;
    
: init
    init  ( previous initialization )
    0 0 fnt-goto
    digit_lg fnt-select
    BLUE tft-fg !
    draw-frame
;

: test-str
    digit_lg fnt-select
    3 3 fnt-goto
    42 0 <# # #s #> type
    42 0 <# # #s #> 3 3 fnt-drawstring
;
