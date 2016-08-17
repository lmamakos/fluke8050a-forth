\ -*- mode: forth; indent-tabs-mode: nil; -*-

( "application" code )

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


\ these variables are associated with captured Fluke 8050A signals
\ minimal processing is done as they are acquired
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
0 variable current-strobe-time \ just completed strobe time
0 variable strobe-loss        \ lost strobe signal while sampling data
0 variable strobe0-loss       \ lost strobe 0 signal while sampling data


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

\ -- NOTE -----------------------------------------------------------------------
\ constants with "_bb" suffix indicate memory addresses in the "bit-band"
\ memory region.  Each address is aliased (in this case) to a particular
\ bit in a peripheral register.  This allows individual bit access through
\ one memory reference without needing to perform any masking
\
\ only GPIO read data register access is defined and used in this application
\ -------------------------------------------------------------------------------

\ fetch current state of multiplexed display signals when digit strobe signal fires
\ This is a fairly timing sensitive process, thus the use of bit-banding to
\ sample the signals individually and making this word "inline: which bloats out
\ the chkstrobes word that invokes it, but saves extra call/return..
: get-digit-data-strobe
    z_bb  @                     \ bit 0  ( variable io sum )
    y_bb  @       shl +         \ bit 1  ( variable io sum )
    x_bb  @  2 lshift +         \ bit 2  ( variable io sum )
    w_bb  @  3 lshift +         \ bit 3  ( variable io sum )
    dp_bb @  8 lshift +         \ bit 4  ( decimal point )
    hv_bb @        hv !         \ high voltage indicator
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
    st0_bb wait-strobe strobe-asserted? if
	get-digit-data-strobe
	st0_bb @ 0= if 1 strobe-loss +! 1 strobe0-loss +! then
	d0 !
	$01 strobe-mask @ or strobe-mask !
    then

    st1_bb wait-strobe strobe-asserted? if
	get-digit-data-strobe
	st1_bb @ 0= if 1 strobe-loss +! then
	d1 !
	$02 strobe-mask @ or strobe-mask !
    then

    st2_bb wait-strobe strobe-asserted? if
	get-digit-data-strobe
	st2_bb @ 0= if 1 strobe-loss +! then
	d2 !
	$04 strobe-mask @ or strobe-mask !
    then

    st3_bb wait-strobe strobe-asserted? if
	get-digit-data-strobe
	st3_bb @ 0= if 1 strobe-loss +! then
	d3 !
	$08 strobe-mask @ or strobe-mask !
    then

    st4_bb wait-strobe strobe-asserted? if
	get-digit-data-strobe
	st4_bb @ 0= if 1 strobe-loss +! then
	d4 !
	$10 strobe-mask @ or strobe-mask !
    then
;
    
\ make a pass through and wait for all the digit strobes to occur
: getstrobes
    0 strobe-mask !
    begin
	chkstrobes
	strobe-mask @ $1f =
    until
;

\
\ -----------------------------------------------------------
\

: ary <builds 0 do -1 , loop   does> swap cells + ;

-1 constant char-none       \ special value for "blank" character on display
1 constant sign-plus
2 constant sign-minus

char-none variable state_sign   \ main display sign
char-none variable state_rSign  \ relative display sign
0 variable state_function       \ current function mode
0 variable state_range
0 variable state_rel
0 variable state_relValid
0 variable state_mode           \ what mode symbol is displayed


0 variable redraw
-1 variable prev_state_function
-1 variable prev_state_range
-1 variable prev_state_rel
-1 variable prev_state_relValid
-1 variable prev_state_mode

\ display field variables
-1 variable disp_mainDigits_sign
5 ary disp_mainDigits

-1 variable disp_relDigits_sign
5 ary disp_relDigits

-1 variable disp_zDigits_sign
5 ary disp_zDigits


\ display main multimeter measurement digits
: displayMain
;

\ display relative offset multimeter digits
: displayRel
;

\ display dB/impedence
: displayZ
;

\ display multimeter mode (V, ohms, mA, etc.)
: displayMode
;

\ display units
: displayUnit
;

\ reset/clear display
: displayClear
;


\ -----------------------------------------------------------


: compute-function-range
    
;

: compute-update
    
;



0 variable display-update-time

\
\ -----------------------------------------------------------
\
: drawdigit
    \ allow -1 to pass through to render a space
    \ 15 is also a space/blank on the display
    dup $ffffff00 and $100 = if  \ is decimal point selected (and not a blank?)
        dp_lg fnt-select         \ select decimal point "font"
        1 fnt-drawchar           \ draw it
        digit_lg fnt-select      \ select large number "font"
    then

    dup 15 and 15 = if
        drop -1
    then
    dup 0< not if
        15 and [char] 0 +
    then
    fnt-drawchar
;


: draw-value
    d0 @ $0c and
    case
        0 of -1  endof
        4 of  0  endof        ( plus )
        12 of 0  endof        ( plus + minus segments )
        8 of  1  endof        ( minus )
    endcase
    symbolSign fnt-select
    fnt-drawglyph

    digit_lg fnt-select           

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

: disp-update
    2 2 fnt-goto
    digit_lg fnt-select
    draw-value
;

: status-func.
    s" F: "  fnt-puts
    fluke_func @ 0 <# #s #> fnt-puts
    32 fnt-drawchar    32 fnt-drawchar
;

: status-range.
    s" R: " fnt-puts
    fluke_range @ 0  <# #s #> fnt-puts
    32 fnt-drawchar    32 fnt-drawchar
;

: emit-status-digit
    tft-fg @
    over
    $100 and if
        red tft-fg !
    then
    hex
    swap
    $ff and 0 <# # #s #> fnt-puts
    decimal
    [char] | fnt-drawchar
    tft-fg !
;

: status-digits.
    s" D:" fnt-puts
    d0 @ emit-status-digit
    d1 @ emit-status-digit
    d2 @ emit-status-digit
    d3 @ emit-status-digit
    d4 @ emit-status-digit
;


: status-strobes.
    s" StbLoss: " fnt-puts
    strobe-loss @ 0  <# #s #> fnt-puts
    [char] / fnt-drawchar
    strobe0-loss @ 0  <# #s #> fnt-puts
;
    
: status-line
    bmow8x16 fnt-select
    0 239 16 - fnt-goto
    white tft-fg !
    status-func.
    status-range.
    status-digits.
\    status-strobes.
;

: draw-display
    2 2 fnt-goto
    $97ef tft-fg !
    disp-update
    BLUE tft-fg !
    current-strobe-time @ last-strobe-time @ - 0 <#  # # # #s #>  2 fnt-height @ 2 +  fnt-drawstring
    
    status-line
;

\ complete a measurement / display cycle
: fluke-multimeter-display
    current-strobe-time @ last-strobe-time !
    getstrobes
    millis dup current-strobe-time !
    getswitches
    compute-update
    draw-display
    millis swap - display-update-time !
;
    
: display
    begin
        fluke-multimeter-display
	key?
    until
;

: init
    init  ( previous initialization )
    0 0 fnt-goto
    digit_lg fnt-select
    BLUE tft-fg !
    display
;

init
