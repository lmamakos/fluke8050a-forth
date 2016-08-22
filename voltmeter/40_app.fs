\ -*- mode: forth; indent-tabs-mode: nil; -*-

( "application" code )
\ reset

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
0 constant range_Z
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
\ REL     x        x       x       L      ( 1 not rel, 0 rel )
4 constant function_v
2 constant function_mA
0 constant function_kOhm
6 constant function_dB
0 constant function_S
8 constant function_AC
1 constant function_notREL
6 constant function_mask

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
-1 variable debugging-modes    \ various debugging bits.   Only boolean at the moment

: get-func-range-switches  ( -- )
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

\ this logic is going to need to be modified because
\ not all digits are strobed when in the "set Z (impedence)"
\ mode.  Interrupt-driven logic with "complete" when
\ strobe4 fires would indicate a complete pass..?

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
: get-strobes
    0 strobe-mask !
    begin
	chkstrobes
	strobe-mask @ $1f =
    until
;

\
\ -----------------------------------------------------------
\

: ary
  <builds
    0 do -1 , loop
  does>
    swap cells +
;

$ff constant char-none       \ special value for "blank" character on display
$100 constant prefix-decimal-point

\ --------------------------------------------------------------------------------------------
\
\ definition word to allocate data that corresponds to a field that will be displayed.
\ this will probably accrete metadate and stuff over time
\
\ this doesnt work because the data constructed in the <builds portion of the definition
\ ends up in flash when compiletoflash is active.  Since there's nothing clever going on
\ with the does> bit, just do a quick and dirty alternative
\
\ : display-field
\  <builds
\    -1 ( initial value )
\    -2 ( previous value )
\    WHITE ( default field color )
\    0  ( other random fields )
\    4  nvariable 
\  does>
\    \ return address of data item
\ ;

: display-field
    -4       ( random future stuff )
    WHITE    ( default foreground color for drawing )
    -2       ( "previous" value )
    -1       ( "current" value of field )
    4  nvariable
;


\ get contents of display field
: field@ ( field -- value )
    @
;

\ set contents of display field
: field! ( value field -- )
    !
;

: field-color@ ( field -- foreground-color )
    2 cells + @
;

: field-color! ( color field -- )
    2 cells + !
;

: field-blink@ ( field -- blink-rate )
    2 cells + 1 + c@
;

: field-blink! ( blink-rate field -- )
    2 cells + 1 + c,
;

    
\ --------------------------------------------------------------------------------------------
\  display definitions and stuff
\

4 constant disp-top             \ top line of display

\ color definitions for various display items
$0200    constant color-disp-bg          \ background color
WHITE    constant color-disp-fg          \ main display number colors
RED      constant color-sep-line-error
DARKGREY constant color-sep-line
$97ef    constant color-V        \ unit legend colors
$fc71    constant color-mA       \ attempts to match color
$a50a    constant color-ohm      \ on Fluke 8050a front
$8410    constant color-sievert  \ panel
$8c71    constant color-dB
$3a99    constant color-z
$0000    constant color-unknown  \ XXX



1 constant sign-plus
2 constant sign-minus

char-none variable state_sign   \ main display sign
char-none variable state_rSign  \ relative display sign

0 variable  func-range-error           \ some sort of error in mode/range combination

\ main value display digits
display-field disp-sign
display-field disp-d0
display-field disp-d1
display-field disp-d2
display-field disp-d3
display-field disp-d4

\ relative value display digits
display-field disp-rel-sign
display-field disp-rel-d0
display-field disp-rel-d1
display-field disp-rel-d2
display-field disp-rel-d3
display-field disp-rel-d4

\ some intial values used for debugging
0         disp-d0   field!
1         disp-d1   field!
2         disp-d2   field!
3         disp-d3   field!
4         disp-d4   field!
sign-plus disp-sign field!

\ display main multimeter measurement digits
: dispMainDigit ( digit -- )
    dup $100 and if
        \ display leading "." character
        dp_lg fnt-select
        1 fnt-drawchar
        digit_lg fnt-select
    then

    $ff and \ strip possible decimal points
    dup  char-none =
    over $0f = or if
        drop -1
    else
        $0f and [char] 0 +   \ convert to ASCII
    then
    fnt-drawchar
;

: display-Main
    symbolSign fnt-select
    color-disp-fg tft-fg !
    
    0 disp-top 25 + fnt-goto
    disp-sign field@ dup char-none = if drop -1 then fnt-drawchar
    fnt-getpos drop disp-top fnt-goto      \ remove offset for symbol

    digit_lg fnt-select
    disp-d0 field@ dispMainDigit
    disp-d1 field@ dispMainDigit
    disp-d2 field@ dispMainDigit
    disp-d3 field@ dispMainDigit
    disp-d4 field@ dispMainDigit
;

\ display relative offset multimeter digits
: display-Rel
;

\ display dB/impedence
: display-Z
;

\ display multimeter mode and units (V, ohms, mA, etc.)

258            constant modeDispMode-x
disp-top 4 +   constant modeDispMode-y

263            constant modeDispUnit-x
disp-top 42 +  constant modeDispUnit-y

display-field disp-mode      3 disp-mode field!
display-field disp-unit      1 disp-unit field!

display-field disp-rel-mode
display-field disp-rel-unit

: display-ModeUnit
    color-disp-fg tft-fg !
    symbolMode fnt-select
    modeDispMode-x modeDispMode-y fnt-goto
    disp-mode field@ fnt-drawchar

    tft-fg @
    disp-unit field-color@ tft-fg !
    symbolUnit fnt-select
    modeDispUnit-x modeDispUnit-y fnt-goto
    disp-unit field@ fnt-drawchar
    tft-fg !
;

symbolUnit fnt-select  ( select symbolUnit font so we can extract font height )

10                 constant sepline-x0
modeDispUnit-y fnt-height @ + 8 + constant sepline-y0
modeDispUnit-x     constant sepline-x1
sepline-y0 3 +     constant sepline-y1

: display-separator-line
    sepline-x0 sepline-y0  ( push two corners  )
    sepline-x1 sepline-y1  ( on the stack      )
    func-range-error @ 0= if
        darkgrey           ( followed by color )
    else
        red
    then
    fillrect    ( fill rectangle for thin line )
;

: display-Update
    display-Main
    display-ModeUnit
    display-separator-line
    display-Z
    display-Rel
;

\ reset/clear display
: display-Clear
    color-disp-bg tft-bg !
    clear
;


\ -----------------------------------------------------------

: compute-func-kOhm ( -- unit )
    color-ohm disp-unit field-color!
    fluke_range @ case
        range_2mS   of UNIT_mS
            color-sievert disp-unit field-color! endof
        range_200nS of UNIT_nS
            color-sievert disp-unit field-color! endof
        range_20M   of UNIT_MOhm endof
        range_0.2   of UNIT_Ohm endof
        true ?of ( else ) UNIT_kOhm endof
    endcase
;

: compute-func-v ( -- unit )
    color-V disp-unit field-color!
    fluke_range @ case
        range_0.2 of UNIT_mV endof
        range_20M of
            ( XXX invalid range ) UNIT_V
            RED disp-unit field-color!
            1 func-range-error !
        endof
        true ?of ( else ) UNIT_V  endof
    endcase
;

: compute-func-mA ( -- unit )
    color-mA disp-unit field-color!
    fluke_range @ case
        range_20M of
            ( XXX invalid range ) UNIT_mA
            RED disp-unit field-color!
            1 func-range-error !
        endof
        range_0.2 of UNIT_microA endof
        true ?of ( else ) UNIT_mA endof
    endcase
;

: compute-func-dB ( -- unit )
    color-dB disp-unit field-color!
    fluke_range @  case
        range_Z of color-sievert  disp-unit field-color! UNIT_Z endof
        true ?of ( else ) UNIT_dB endof
    endcase
;

0 variable func_REL

: compute-function-range
    fluke_func @ function_AC and if MODE_AC else MODE_DC then   \ determine AC or DC
    fluke_func @ function_mask and function_kOhm = if drop char-none then \ except if in Ohms, then blank
    \ XXX also check for units as impedence
    disp-mode field!

    fluke_func @ function_mask and
    case
        function_kOhm of  compute-func-kOhm endof   \ also function_S
        function_v    of  compute-func-v    endof
        function_mA   of  compute-func-mA   endof
        function_dB   of  compute-func-dB   endof
        true         ?of ( else ) char-none endof   \ default others
    endcase
    disp-unit field!

    fluke_func @ function_notREL and 0= func_REL !
;

: compute-main-display
    d0 @ $0c and
    case
        0 of  char-none  endof
        4 of  sign-plus  endof        ( plus )
        12 of sign-plus  endof        ( plus + minus segments )
        8 of  sign-minus endof        ( minus )
    endcase
    disp-sign field!

    d0 @ 1 and
    dup 0= if
        drop char-none
    then
    disp-d0 field!
    d1 @ disp-d1 field!
    d2 @ disp-d2 field!
    d3 @ disp-d3 field!
    d4 @ disp-d4 field!
;

\ figure out what to do about the "REL" relative display

: compute-relative-display
    func_REL 0= func-range-error 0= and dup  ( split into two ifs to avoid Jump Too Far )
    if  ( not relative mode and not some error )
        \ snapshot mode/units
        disp-unit field@        disp-rel-unit field!
        disp-unit field-color@  disp-rel-unit field-color!
        disp-mode field@        disp-rel-mode field!
        disp-mode field-color@  disp-rel-mode field-color!
    then
    if
        \ snapshot the relevant display digits while not in "relative" mode
        disp-sign field@  disp-rel-sign field!
        disp-d0 field@    disp-rel-d0 field!
        disp-d1 field@    disp-rel-d1 field!
        disp-d2 field@    disp-rel-d2 field!
        disp-d3 field@    disp-rel-d3 field!
        disp-d4 field@    disp-rel-d4 field!
    then
;


: compute-update
    0 func-range-error !
    compute-function-range
    compute-main-display
    compute-relative-display
;

0 variable display-update-time
0 variable #display-updates

\ --------------------------------------------------------------------------------------
\ status line stuff
\
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

: status-updates.
    s" " fnt-puts
    tft-fg @ tft-bg @
    #display-updates field@ 1 and
    if
        2dup
        tft-fg ! tft-bg ! 
    then
    #display-updates field@ 0 <# #s #> fnt-puts
    tft-bg ! tft-fg !
;

: status-line
    bmow8x16 fnt-select
    0 239 16 - fnt-goto
    WHITE tft-fg !
    status-func.
    status-range.
    status-digits.
    \ status-strobes.
    status-updates.
;

\ --------------------------------------------------------------------------------------

\ complete a measurement / display cycle
: fluke-multimeter-display
    #display-updates field@ 1 + #display-updates field!
    current-strobe-time @ last-strobe-time !
    get-strobes
    millis dup current-strobe-time !
    get-func-range-switches
    compute-update
    display-update
    debugging-modes if status-line then
    millis swap - display-update-time !
;
    
: display
    begin
        millis dup current-strobe-time !  last-strobe-time ! 
        fluke-multimeter-display
    key? until
;

: display-initialize
    display-Clear
    get-func-range-switches
    compute-update
    display-Update
    fluke_func @ function_notREL and 0= if 1 debugging-modes +! then
;


: init
    init-35_core  ( previous initialization )
    key? 0= if
        display-initialize
        display
    else
        cr ." [Auto start aborted]" cr
    then
;

( until loaded into flash ) init
