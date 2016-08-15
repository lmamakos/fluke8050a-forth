\ -*- mode: forth; indent-tabs-mode: nil; -*-

( layer "D" - development code )
reset

compiletoram
\
\ most significant digit is either 'blank' for 0 value or 1 for 1 value
\ rest of digits are value 15 for blank
\
\ detect overrange configuration, with only leading "1" digit and
\ 4 following blank digits  -- blank digits are apparently binary 15
\
\ detect improper switch settings wit 4 decimal points and no digit segments
\
\ polarity sign disabled for Vac, mA and k-ohm functions.
\
\ detect conductance switch configuration (k-ohm,200,2k, 2000k, 20M)
\ detect diode test - k-ohm, 2, 200, 20M)
\
\ detect dB with impedence switching mode
\

\ range signal configuration
\       rng_a  rng_b  rng_c  value (a/b/c)
\  0.2    H      H      L      6
\    2    L      H      H      3
\   20    H      L      H      5
\  200    L      L      H      1
\ 2000    H      H      H      7
\  20M    L      L      L      0
\  2mS    L      H      L      2 (Sieverts - conductance)
\ 200nS   H      L      L      4 (Sieverts - conductance)
\
6 constant range_0.2
3 constant range_2
5 constant range_20
1 constant range_200
7 constant range_2000
0 constant range_20M
2 constant range_2mS
4 constant range_200nS
\
\
\       func_a  func_b  func_c  func_d   value (a/b/c/d)
\ V       x        H       L       x      4
\ mA      x        L       H       x      2
\ kOhm    x        L       L       x      0  (also depends on range)
\ dB      x        H       H       x      6
\ S       x        L       L       x      0  (also depends on range)
\ AC      H        x       x       x
\ REL     x        x       x       L
4 constant function_v
2 constant function_mA
0 constant function_kOhm
6 constant function_dB
0 constant function_S
8 constant function_AC
0 constant function_REL


\ variables associated with captured Fluke 8050A signals
0 variable d0  \ 8 - minus, 4 - plus, 2 - dB, 1 - 1
0 variable d1  \ BCD 
0 variable d2  \ BCD
0 variable d3  \ BCD
0 variable d4  \ BCD
0 variable hv
0 variable fluke_func
0 variable fluke_range

0 variable strobe-mask        \ mask of strobes that have been seen

\ debugging variables for tracking display strobe performance
0 variable last-strobe-time   \ last time a strobe sequence completed
0 variable strobe-loss        \ lost strobe signal while sampling data
0 variable strobe0-loss       \ lost strobe 0 signal while sampling data
0 variable strobe-multiple    \ one than one strobe apparently asserted


: getswitches  ( -- )
    fluke_func_d io@                \ FA 0=DC, 1=AC
    fluke_func_c io@       shl +    \ 
    fluke_func_b io@  2 lshift +
    fluke_func_a io@  3 lshift +
    fluke_func !

    fluke_rng_c  io@
    fluke_rng_b  io@       shl +
    fluke_rng_a  io@  2 lshift +
    fluke_range !
;

: getdigit ( bb-io variable -- )
    swap  ( variable bb-io )
    dup  ( variable io bb-io )
    begin
	@ if
	    ( variable bb-io )
	    fluke_z io@                     \ bit 0  ( variable io sum )
	    fluke_y io@       shl +         \ bit 1  ( variable io sum )
	    fluke_x io@  2 lshift +         \ bit 2  ( variable io sum )
	    fluke_w io@  3 lshift +         \ bit 3  ( variable io sum )
	    fluke_dp io@ 8 lshift +         \ bit 4  ( decimal point )
	    fluke_hv io@ hv !
	    swap                    ( variable sum bb-io )
	    @ 0= if
		1 strobe-loss +!
	    then
  	    ( variable sum )
	    swap !
	    0
	else
	    ( variable bb-io )
	    dup dup ( variable bb-io bb-io )
	then
    0= until
;

\ generate bit-band address given bit and offset into bitband region
: _io>bb ( bit offset -- addr )
    swap dup    ( offset bit bit )
    io-base rot +
    $000fffff and 5 lshift  \ offset into bitband region for register
    swap io# 2 lshift +     \ bit offset
    $42000000 +             \ base of bitband region
;

\ generate ARM Cortex bit-band addresses corresponding to bit position in GPIO input register
: ior>bb  ( pin -- addr ) GPIO.IDR _io>bb ;

\ generate ARM Cortex bit-band addresses corresponding to bit position in GPIO input register
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
fluke_rng_a ior>bb constant rng_c_bb
fluke_rng_b ior>bb constant rng_c_bb
fluke_rng_c ior>bb constant rng_c_bb

\ fetch current state of multiplexed display signals.
: get-signals
    z_bb @                     \ bit 0  ( variable io sum )
    y_bb @       shl +         \ bit 1  ( variable io sum )
    x_bb @  2 lshift +         \ bit 2  ( variable io sum )
    w_bb @  3 lshift +         \ bit 3  ( variable io sum )
    dp_bb @ 8 lshift +         \ bit 4  ( decimal point )
    hv_bb @       hv !         \ high voltage indicator
inline ;


: wait-strobe ( strobebb -- strobebb )
    begin
        dup @
    until
    inline ;

: strobe-asserted? ( strobebb -- t/f)  @ inline ;

\ poll all strobe signals looking for asserted digit strobes
\ in an effort to make this as efficient as possible, we are checking the
\ bit-banded aliases of the bit in the GPIO input data registers.  This
\ avoid having to pass to multiple layers of word calls in the
\ I/O words to hopefully catch the data while the strobe is valid.
\ we check to see if the strobe is still asserted afterwards and
\ bump a counter if not.  Maybe making this interrupt driven will
\ be more effective, or having a local loop polling each sequential
\ strobe signal in succession
: chkstrobes
    \ check for simultaneous strobe assertion.  "Shouldn't happen"
    \ st0_bb @ st1_bb @ + st2_bb @ + st3_bb @ + st4_bb @ +
    \ 1 > if  1 strobe-multiple +! then

    
    st0_bb wait-strobe strobe-asserted? if
	get-signals
	st0_bb @ 0= if 1 strobe-loss +! 1 strobe0-loss +! then
	d0 !
	$01 strobe-mask @ or strobe-mask !
    then

    st1_bb wait-strobe strobe-asserted? if
	get-signals
	st1_bb @ 0= if 1 strobe-loss +! then
	d1 !
	$02 strobe-mask @ or strobe-mask !
    then

    st2_bb wait-strobe strobe-asserted? if
	get-signals
	st2_bb @ 0= if 1 strobe-loss +! then
	d2 !
	$04 strobe-mask @ or strobe-mask !
    then

    st3_bb wait-strobe strobe-asserted? if
	get-signals
	st3_bb @ 0= if 1 strobe-loss +! then
	d3 !
	$08 strobe-mask @ or strobe-mask !
    then

    st4_bb wait-strobe strobe-asserted? if
	get-signals
	st4_bb @ 0= if 1 strobe-loss +! then
	d4 !
	$10 strobe-mask @ or strobe-mask !
    then
;

    
: getstrobes
    0 strobe-mask !
    begin
	chkstrobes
	strobe-mask @ $1f =
    until
;

: drawdigit
    \ allow -1 to pass through to render a space
    dup 0< not if
        15 and [char] 0 +
    then
    fnt-drawchar
;

: draw-value
    d0 @ 1 and
    dup 0= if
        drop -1
    then
    drawdigit
    d1 @ drawdigit
    d2 @ drawdigit
    d3 @ drawdigit
    d4 @ drawdigit
;


: dscan
    strobe-loss @
    50 0 do
	st0_bb  d0 getdigit
	st1_bb  d1 getdigit
	st2_bb  d2 getdigit
	st3_bb  d3 getdigit
	st4_bb  d4 getdigit
	2 2 fnt-goto
	digit_lg fnt-select
	draw-value
    loop
    ." lost strobes: " 
    strobe-loss @ swap - . cr
;

: disp-scan
    getstrobes
    getswitches

    2 2 fnt-goto
    digit_lg fnt-select
    draw-value
;


: fluke-display-time
    GREEN tft-fg !
    millis 1000000 mod 0 <# # # # # # #s #>    2 fnt-height @ 2 * 2 +  fnt-drawstring
;

: status-func.
    s" Func: "  fnt-puts
    fluke_func @ 0 <# #s #> fnt-puts
    32 fnt-drawchar    32 fnt-drawchar
;

: status-range.
    s" Rng: " fnt-puts
    fluke_range @ 0  <# #s #> fnt-puts
    32 fnt-drawchar    32 fnt-drawchar
;

: status-strobes.
    s" StbLoss: " fnt-puts
    strobe-loss @ 0  <# #s #> fnt-puts
    [char] / fnt-drawchar
    strobe0-loss @ 0  <# #s #> fnt-puts
    [char] / fnt-drawchar
    strobe-multiple @ 0  <# #s #> fnt-puts
;

    
: status-line
    bmow8x16 fnt-select
    0 239 16 - fnt-goto
    white tft-fg !
    status-func.
    status-range.
    status-strobes.
;

: fluke-display
    begin
	millis last-strobe-time !
	2 2 fnt-goto
	$97ef tft-fg !
	disp-scan

	BLUE tft-fg !
	millis last-strobe-time @ - 0 <#  # # # #s #>  2 fnt-height @ 2 +  fnt-drawstring
	\ fluke-display-time
        status-line
	key?
    until
;

: test-strobes
    begin
	getstrobes
        millis last-strobe-time !
        getstrobes
	millis dup last-strobe-time @ - .
        status-line
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

init
