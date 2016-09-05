\ -*- mode: forth; indent-tabs-mode: nil; -*-
\
\ Louis Mamakos
\ louie@transsys.com
\ 29 August 2016
\
\ I saved an enormous amount of time by stealing all sorts of good
\ ideas (and even some fonts!) from others who have graciously shared
\ their experiences in blog postings on the Internet.  In particular,
\ these were extremely valuable resources:
\
\     https://sites.google.com/site/kenselectronicsprojects/fluke8050a_display
\     http://vondervotteimittiss.com/belfry/?p=180
\
\ The latter page yielded some wonderful C code and embedded
\ documentation that I liberally borrowed and incorporated into this
\ mutant FORTH implementation you see below.
\
\ Unlike the C implementation that targeted an Atmel part, I used a
\ synchronous polling loop approach.  This seems a reasonable approach
\ given the relatively slow update cycle of the meter (about 420
\ milliseconds) and where rendering the display takes only 60
\ milliseconds, leaving plenty of headroom.  It might be nice to run
\ the FORTH "console" in a separate task to be able to poke around
\ while the display is running, but the added complexity isn't needed.

( "application" code )

\ Conventions and hints:
\
\ most significant digit is either 'blank' for 0 value or 1 for 1
\ value rest of digits are value 15 for blank
\
\ stuff to implement:
\
\ detect overrange configuration, with only leading "1" digit and 4
\ following blank digits -- blank digits are apparently binary 15
\
\ detect improper switch settings with 4 decimal points and no digit
\ segments
\
\ detect conductance switch configuration (k-ohm,200,2k, 2000k, 20M)
\ detect diode test - k-ohm, 2, 200, 20M)
\
\ indicate invalid REL mode after mode change
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

0 variable unscaled-Display-Value  \ 0 - 19999

0 variable strobe-mask        \ mask of strobes that have been seen
\ debugging variables for tracking display strobe performance
0 variable last-strobe-time   \ last time a strobe sequence completed
0 variable current-strobe-time \ just completed strobe time
0 variable strobe-loss        \ lost strobe signal while sampling data
0 variable strobe0-loss       \ lost strobe 0 signal while sampling data
0 variable strobe1-loss       \ lost strobe 0 signal while sampling data
0 variable strobe2-loss       \ lost strobe 0 signal while sampling data
0 variable strobe3-loss       \ lost strobe 0 signal while sampling data
0 variable strobe4-loss       \ lost strobe 0 signal while sampling data

\ first cell is microsecond counter of when strobe fired, second cell is time
\ to process
0 0 2variable strobe0-micros
0 0 2variable strobe1-micros
0 0 2variable strobe2-micros
0 0 2variable strobe3-micros
0 0 2variable strobe4-micros

-1 variable debugging-modes    \ various debugging bits.   Only boolean at the moment
0 variable debugging-flash-free
0 variable debugging-ram-free


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
\ constants with "_bb" suffix indicate memory addresses in the
\ "bit-band" memory region.  Each address is aliased (in this case) to
\ a particular bit in a peripheral register.  This allows individual
\ bit access through one memory reference without needing to perform
\ any masking
\
\ only GPIO read data register access is defined and used in this
\ application
\ -------------------------------------------------------------------------------

\ fetch current state of multiplexed display signals when digit strobe
\ signal fires This is a fairly timing sensitive process, thus the use
\ of bit-banding to sample the signals individually and making this
\ word "inline: which bloats out the chkstrobes word that invokes it,
\ but saves extra call/return..
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

: null-wait-strobe inline ;

: strobe-asserted? ( strobebb -- t/f)  @ inline ;

: strobe-timestamp ( startmicros tstampvar -- )
    2dup           ( startmicros tstampvar startmicros tstampvar -- )
    !              ( startmicros tstampvar -- )
    1 cells +      ( startmicros tstampvar+4  -- )
    micros         ( startmicros tstampvar nowmicros )
    rot -          ( tstamp difference )
    swap !
;


\ -------------------------------------------------------------

\ poll all strobe signals looking for asserted digit strobes in an
\ effort to make this as efficient as possible, we are checking the
\ bit-banded aliases of the bit in the GPIO input data registers.
\ This avoid having to pass to multiple layers of word calls in the
\ I/O words to hopefully catch the data while the strobe is valid.  we
\ check to see if the strobe is still asserted afterwards and bump a
\ counter if not.  Maybe making this interrupt driven will be more
\ effective, or having a local loop polling each sequential strobe
\ signal in succession

\ this logic is going to need to be modified because not all digits
\ are strobed when in the "set Z (impedence)" mode.  Interrupt-driven
\ logic with "complete" when strobe4 fires would indicate a complete
\ pass..?

: chkstrobes
    st0_bb wait-strobe strobe-asserted? if
        led-on                                   \ we're now in the thick of it..
        micros
	get-digit-data-strobe
	st0_bb @ 0= if  1 strobe0-loss +! then
	d0 !
        $01 strobe-mask @ or strobe-mask !
        strobe0-micros strobe-timestamp
    then

    st1_bb wait-strobe strobe-asserted? if
        micros
	get-digit-data-strobe
	st1_bb @ 0= if   1 strobe1-loss +! then
	d1 !
	$02 strobe-mask @ or strobe-mask !
        strobe1-micros strobe-timestamp
    then

    st2_bb wait-strobe strobe-asserted? if
        micros
	get-digit-data-strobe
	st2_bb @ 0= if   1 strobe2-loss +! then
	d2 !
	$04 strobe-mask @ or strobe-mask !
        strobe2-micros strobe-timestamp
    then

    st3_bb wait-strobe strobe-asserted? if
        micros
	get-digit-data-strobe
	st3_bb @ 0= if   1 strobe3-loss +! then
	d3 !
	$08 strobe-mask @ or strobe-mask !
        strobe3-micros strobe-timestamp
    then

    st4_bb wait-strobe strobe-asserted? if
        micros
	get-digit-data-strobe
	st4_bb @ 0= if   1 strobe4-loss +! then
	d4 !
	$10 strobe-mask @ or strobe-mask !
        strobe4-micros strobe-timestamp
    then
;
    
\ make a pass through and wait for all the digit strobes to occur
: get-strobes
    0 strobe-mask !
    begin
	chkstrobes
\	strobe-mask @ $1f =
        strobe-mask @ $10 and
    until

    \ "strobe-loss" is not a horrible fatal problem.  This is an event
    \ where once a digit strobe has been asserted and we start
    \ processing to collect the display bus signal state, the strobe
    \ is still not asserted at the end.  The bus may still be stable
    \ before being set-up for the next digit, but this is an
    \ indication that the timing budget is running close to or past
    \ margins.
    \
    \ signal strobe width is ~118 microseconds, with about 330
    \ microseconds between each digit strobe's leading edge being
    \ asserted
    \
    \ sum up the individual digit strobe loss counters into one global
    \ counter for each reference.
    
    strobe0-loss @  strobe1-loss @ +  strobe2-loss @ +  strobe3-loss @ +  strobe4-loss @ +  strobe-loss !
;

\ -------------------------------------------------------------
\ --- some debugging code to characterize signal timing
\
: strobe-time-asserted ( strobebb -- duration-us )
    wait-strobe
    micros
    swap
    begin
        dup @
    0= until
    drop
    micros swap -
;

: strobe-time-all cr
    st0_bb strobe-time-asserted ." Strobe 0 duration: " . cr
    st1_bb strobe-time-asserted ." Strobe 1 duration: " . cr 
    st2_bb strobe-time-asserted ." Strobe 2 duration: " . cr 
    st3_bb strobe-time-asserted ." Strobe 3 duration: " . cr
    st4_bb strobe-time-asserted ." Strobe 4 duration: " . cr
;

: strobe-fires cr
    \ strobe 0 time baseline 
    get-strobes
    strobe0-micros @           ." Strobe 0 relative 0 us "       strobe0-micros 1 cells + @ ." svc time " . ." us" cr
    strobe1-micros @ over -    ." Strobe 1 relative " . ." us "  strobe1-micros 1 cells + @ ." svc time " . ." us" cr
    strobe2-micros @ over -    ." Strobe 2 relative " . ." us "  strobe2-micros 1 cells + @ ." svc time " . ." us" cr
    strobe3-micros @ over -    ." Strobe 3 relative " . ." us "  strobe3-micros 1 cells + @ ." svc time " . ." us" cr
    strobe4-micros @ swap -    ." Strobe 4 relative " . ." us "  strobe4-micros 1 cells + @ ." svc time " . ." us" cr
;

\ The result is that the strobe pulse-width appears to be about 118 microseconds
\ strobe-time-all:
\ Strobe 0 duration: 117
\ Strobe 1 duration: 118
\ Strobe 2 duration: 118
\ Strobe 3 duration: 118
\ Strobe 4 duration: 118
\
\ typical strobe-fires: (after having run a real loop)
\ Strobe 0 relative 0 us svc time 4 us
\ Strobe 1 relative 332 us svc time 3 us
\ Strobe 2 relative 664 us svc time 3 us
\ Strobe 3 relative 1061 us svc time 4 us
\ Strobe 4 relative 1393 us svc time 4 us
\
\ -----------------------------------------------------------

$ff constant char-none       \ special value for "blank" character on display
$100 constant prefix-decimal-point

\ --------------------------------------------------------------------------------------------
\
\ definition word to allocate data that corresponds to a field that
\ will be displayed.  this will probably accrete metadate and stuff
\ over time
\
\ this doesnt work because the data constructed in the <builds portion
\ of the definition ends up in flash when compiletoflash is active.
\ Since there's nothing clever going on with the does> bit, just do a
\ quick and dirty alternative
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
$0200    variable color-disp-bg-var      \ background color
WHITE    variable color-disp-fg-var      \ main display number colors
YELLOW   variable color-disp-rel-fg-var  \ relative display number colors

lightgrey constant color-status-line

RED      constant color-disp-over-range
ORANGE   constant color-disp-bar-graph
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
0 variable  disp-over-range            \ input signal is over-range

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

dp_lg      variable dp-font
digit_lg   variable digit-font
bmow8x16   variable status-font

0 variable #display-updates    \ counter of display updates

: blink? #display-updates @ 1 and 0<> ;

\ display main multimeter measurement digits
: dispDigit ( digit -- )
    dup $100 and if
        \ display leading "." character
        dp-font @ fnt-select
        1 fnt-drawchar
        digit-font @ fnt-select
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
    dp_lg dp-font !
    digit_lg digit-font !
    
    symbolSign fnt-select
    disp-over-range @   blink? not and if
        color-disp-over-range tft-fg !
    else
        color-disp-fg-var @ tft-fg !
    then
    
    0 disp-top 25 + fnt-goto
    disp-sign field@ dup char-none = if drop -1 then fnt-drawchar
    fnt-getpos drop disp-top fnt-goto      \ remove offset for symbol

    digit-font @ fnt-select
    disp-d0 field@ dispDigit
    disp-d1 field@ dispDigit
    disp-d2 field@ dispDigit
    disp-d3 field@ dispDigit
    disp-d4 field@ dispDigit

    disp-over-range @ if
        tft-fg @              \ get current foreground color
        blink? if             \ to get blinking behavior on alternative updates
            bmow8x16 fnt-select
            color-disp-over-range tft-fg !
            s" INPUT OVER RANGE" 70 disp-top 25 + fnt-drawstring
        else
            70 disp-top 25 +  280 disp-top 42 + color-disp-bg-var @ fillrect
        then
        tft-fg !
    then
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
    color-disp-fg-var @ tft-fg !
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


\ --------------------------------------------------------------------

symbolUnit fnt-select  ( select symbolUnit font so we can extract font height )

10                                 constant sepline-x0
modeDispUnit-y fnt-height @ + 8 +  constant sepline-y0
ili9341_width @ sepline-x0 -       constant sepline-x1
sepline-y0 3 +                     constant sepline-y1

: display-separator-line
    sepline-x0 sepline-y0  ( push two corners  )
    sepline-x1 sepline-y1  ( on the stack      )
    func-range-error @ 0= if
        disp-unit field-color@  ( followed by color )
    else
        red
    then
    fillrect    ( fill rectangle for thin line )
; \  display-separator-line

\ --------------------------------------------------------------------

0               constant relDisp-x 
sepline-y1 6 +  constant relDisp-y1     \ line 1
relDisp-y1 36 + constant relDisp-y2     \ line 2

\ display relative offset multimeter digits
\ if this was refactored "correctly", display-Main and display-Rel
\ could be collapsed together..
: display-Rel-data
    dp_small  dp-font !
    digit_sm  digit-font !
    
    symbolSignSm fnt-select
    color-disp-rel-fg-var @ tft-fg !

    0 relDisp-y1 12 + fnt-goto

    disp-rel-sign field@ dup char-none = if drop -1 then fnt-drawchar
    fnt-getpos drop relDisp-y1 fnt-goto      \ remove offset for symbol

    digit-font @ fnt-select
    disp-rel-d0 field@ dispDigit
    disp-rel-d1 field@ dispDigit
    disp-rel-d2 field@ dispDigit
    disp-rel-d3 field@ dispDigit
    disp-rel-d4 field@ dispDigit
    fnt-blankchar

    symbolMode_small fnt-select
    fnt-getpos ( save current position on stack )
    disp-rel-mode field@ fnt-drawchar

    18 + ( bump y position )  fnt-goto

    symbolUnitSmall fnt-select
    disp-rel-unit field@ fnt-drawchar

    symbolMode fnt-select
    modeDispMode-x relDisp-y1 fnt-goto
    MODE_REL fnt-drawchar
    
;  \ display-Rel

\ blank relative display area when REL mode is disabled
: display-Rel-clear
    relDisp-x relDisp-y1
    ili9341_width @ relDisp-y2 color-disp-bg-var @ fillrect
;

false variable func_REL
false variable func_REL-previous

: display-Rel
    \ if REL function is engaged then render the REL data on the display
    func_REL @ if
        display-Rel-data
        true func_REL-previous !
    else
        func_REL-previous @ if  \ if we just turned off the REL display
            display-Rel-clear
            false func_REL-previous !
        then
    then
;
    
\ --------------------------------------------------------------------

                         5  constant bar-min-x
                       300  constant bar-length
    bar-length bar-min-x +  constant bar-max-x
                       148  constant bar-y
                         5  constant bar-thickness
                     20000  constant maxval
                  0 maxval  2constant maxval.f
 0 bar-length  maxval.f f/  2constant scale-factor.f

: scale-to-bar ( zeroTo2K -- )
   maxval min   0 max   ( ensure between 0 and maxval )
   0 swap               ( convert t fixed point )
   scale-factor.f  f*
   0,5 d+
   swap drop
;

\ display horizonal bar graph near bottom of screen, scaled from 0 -
\ 20000 units to correspond to full-scale display.  Show absolute
\ value, though we might want to do some centered graph when in REL
\ mode..
\
: display-bar ( length -- )
  maxval min   0 max   ( ensure between 0 and maxval )
  dup
  bar-min-x +   bar-y bar-thickness +
  bar-min-x bar-y
  2swap color-disp-bar-graph fillrect

  bar-min-x + 1 + bar-y
  bar-min-x  bar-max-x + bar-y bar-thickness + color-disp-bg-var @ fillrect
;

: render-pointer ( offset color )
    swap
    bmow8x16 fnt-select
    bar-min-x + 4 ( half char width ) -    bar-y 4 - fnt-goto
    tft-fg !
    1 fnt-drawchar
;

\ 0-63  red   0-63 green     0-63 blue normalized to..
\ 5 bits red - 6 bits green - 5 bits blue
: mkcolor ( r g b -- color )
    2/ $1f min
    swap $3f min  5 lshift or
    swap 2/ $1f min 11 lshift or ;

create pointer-colors  color-disp-bg-var @ h,
6 3 0 mkcolor h,   \ orange colors for pointer
12 6 0 mkcolor h,
18 9 0 mkcolor h,
24 12 0 mkcolor h,
30 15 0 mkcolor h,
36 18 0 mkcolor h,
42 22 0 mkcolor h,
55 28 0 mkcolor h,
63 32 0 mkcolor h,

10 constant #pointers
0 0 0 0 0 0 0 0 0 0 10 nvariable old-pointers

: display-pointers ( value )
    \ move existing pointers down a notch
    #pointers 1  do
        i cells old-pointers + @
        i 1- cells old-pointers + !
    loop

    \ save new value in newest location
    #pointers 1- cells old-pointers + !

    \ render all the pointers from oldest to newest
    #pointers 0 do
        \ render if next pointer position is different location than
        \ this one, or if current pointer this saves a millisecond or
        \ two in drawing the overall display
        i cells old-pointers + @  i 1+ cells old-pointers + @ <>   i #pointers 1- = or if
            i cells old-pointers + @ 
            i if  i 2* pointer-colors + h@  else  color-disp-bg-var @  then
            render-pointer
        then
    loop
;

\ show bar graph, and then render pointer to current value as well as
\ a history of fading pointers corresponding to previous 9 readings
: display-bar-graph
    unscaled-display-value @ scale-to-bar    display-bar
    unscaled-display-value @ scale-to-bar    display-pointers 
;

\ --------------------------------------------------------------------

\ render the entire display
\
\ Right now, we simply render all of the visible display
\ digits/legends since that happens in about 70 milliseconds.  Meter
\ display update cycles occur about every 420 milliseconds, and it
\ takes about 1.5 milliseconds to strobe out all 4 display digits,
\ leaving plenty of time to draw the display

: display-Update
    display-Main
    display-ModeUnit
    display-separator-line
    display-Z
    display-Rel
    display-bar-graph
;

\ reset/clear display
: display-Clear
    #pointers 0 do  \ clear out old pointer array
        0 old-pointers i cells + !

    loop
    color-disp-bg-var @ tft-bg !
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

    \ see if overrange
    d0 @ 1 and
    d1 @ $0f and $0f =  and
    d2 @ $0f and $0f =  and
    d3 @ $0f and $0f =  and
    d4 @ $0f and $0f =  and disp-over-range !
;

: next-digit
    $ff and  \ ignore decimal point bit
    dup char-none = if
        drop 0
    then
    unscaled-display-value @ 10 * + unscaled-display-value !
;

: compute-bar-length
    0 unscaled-display-value !
    disp-d0 field@ next-digit
    disp-d1 field@ next-digit
    disp-d2 field@ next-digit
    disp-d3 field@ next-digit
    disp-d4 field@ next-digit
;


\ figure out what to do about the "REL" relative display

: compute-relative-display
    func_REL @ 0= func-range-error @ 0= and dup  ( split into two ifs to avoid Jump Too Far )
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
    compute-bar-length
    compute-relative-display
;

0 variable display-update-time
0 variable display-cycle-start-micros
0 variable display-cycle-end-micros

\ --------------------------------------------------------------------------------------
\ status line stuff
\
: status-func.
    s" F:"  fnt-puts
    fluke_func @ 0 <# #s #> fnt-puts
    32 fnt-drawchar
;

: status-range.
    s" R:" fnt-puts
    fluke_range @ 0  <# #s #> fnt-puts
    32 fnt-drawchar
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
    s" St:" fnt-puts
    strobe-loss @ 0  <# #s #> fnt-puts
    [char] / fnt-drawchar
    strobe0-loss @ 0  <# #s #> fnt-puts
    32 fnt-drawchar
;

: status-updates.
    s"  U:" fnt-puts
    tft-fg @ tft-bg @
    blink? if
        2dup
        tft-fg ! tft-bg ! 
    then
    #display-updates @ 0 <# #s #> fnt-puts
    tft-bg ! tft-fg !
;

: status-render-time.
    display-cycle-end-micros @ display-cycle-start-micros @ -  0 max
    ?dup 0<> if
        s"  Drw:" fnt-puts
        0 <# # # # # #s #> fnt-puts
        s" us" fnt-puts
    then
;

0 variable cur-status-line
: status-draw-fields  ( start-line -- )
    2 - cur-status-line !
    0 cur-status-line @ 319 cur-status-line @ color-status-line fillrect
;

\ 160 - 179
\ 180 - 199
\ 200 - 219
\ 220 - 239

162  dup constant status-line-1
20 + dup constant status-line-2
20 + dup constant status-line-3
20 + dup constant status-line-4

: draw-status-lines
    status-line-1  status-draw-fields
    status-line-2  status-draw-fields
    status-line-3  status-draw-fields
    status-line-4  status-draw-fields

    status-font @ fnt-select
    yellow tft-fg !
    s" FLUKE 8050A DIGITAL MULTIMETER (TFT LCD)" 0 status-line-1  fnt-drawstring
;

: status-line
    draw-status-lines

    bmow8x16 fnt-select
    0 222 fnt-goto
    WHITE tft-fg !
    status-func.
    status-range.
    \ status-digits.
    status-strobes.
    status-render-time.
    status-updates.
;

\ --------------------------------------------------------------------------------------

\ complete a measurement / display cycle
: fluke-multimeter-display
    1 #display-updates +! 
    current-strobe-time @ last-strobe-time !
    get-strobes

    \ HORRIBLE HACK
    \
    \ Normally, the digit display update timing is a regular ~420ms
    \ cycle, with a set of 5 digit strobe signals that fire in quick
    \ succession (asserted ~118 microseconds, with about a 330
    \ microsecond spacing between the leading edge of each digit
    \ strobe
    \
    \ When in the mode where the Fluke 8050A is cycling through a
    \ display of impedence values to be selected (when the V and mA
    \ mode switches are both depressed, and the 20M/Set-Z button is
    \ depressed), the digit strobe signal firing timine changes
    \ signficantly to an approximately once per second rate.  There is
    \ a pattern of 5 digit strobes followed immetiately (610
    \ microseconds) by another set of 5 digit strobes.  The first set
    \ appear to be for the last digit display, and the immediately
    \ following set are for the new value.
    \
    \ There's no chance to pick up this second set of strobes.  So the
    \ hack here, is to toss out the first set of strobes and wait for
    \ the second set to fire.  Hopefully, we can pick that up quickly
    \ enough in the ~500 microseconds we have..  If we happen to get
    \ the timing wrong, and come in here just as the second set is
    \ firing, then we will wait a second and get the first set, miss
    \ the immediately following second set and get back into sync.
    \ The odds are small that the timing would line up like that.
    
    disp-unit field@ UNIT_Z = if
        get-strobes ( again.. )
    then
    
    millis current-strobe-time !

    micros display-cycle-start-micros !
    get-func-range-switches
    compute-update
    display-update
    micros display-cycle-end-micros !

    debugging-modes if status-line then
    led-off   \ indicate "idle" time now
;
    
: display-initialize
    display-Clear
    get-func-range-switches
    compute-update
    display-Update
    fluke_func @ function_notREL and 0= if 1 debugging-modes +! then
    draw-status-lines
;

: display
    display-initialize
    cr ." Multimeter display begin.  Any key to exit" cr
    begin
        millis dup current-strobe-time !  last-strobe-time ! 
        fluke-multimeter-display
    key? until
;


: get-memory-stats  ( -- flashfree ramfree )
    compiletoram?
    compiletoflash here
    swap if compiletoram then
    flash-kb 1024 * swap -  debugging-flash-free !

    \ compute free RAM as space between HERE (top of dictionary) and
    \ FLASHVAR-HERE which grows down from the top of RAM
    flashvar-here here -  debugging-ram-free !
;

: init
    get-memory-stats
    init-35_core  ( previous initialization )
    begin
        key? dup
        if key drop then
        not
    until
    
    bmow8x16 fnt-select
    white tft-fg ! blue tft-bg !
    s" [Pause..]" 100 70 fnt-drawstring
    ." [Pause..]" 2000 ms

    \ abort automatic start if either an input character is present
    \ on the serial port, or if the button on the Maple Mini (D32/PB8)
    \ was pressed

    key? 0=  ( no serial in? )   button? not and
    if
        display
    else
        s" [Auto start aborted]" 2dup cr type cr
        bmow8x16 fnt-select
        white tft-fg ! red tft-bg !
        100 70 fnt-drawstring
    then
;

init
