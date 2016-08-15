\ -*- mode: forth; indent-tabs-mode: nil; -*-

\ Louis Mamakos
\ louie@transsys.com
\
\ split out into different file so we can include before font definitions earlier in
\ flash loading sequence.
\
\ --------------------------------------------------------------------------------------------------------
\ font definition word, to build header

 0 constant fnt-struc-type
 2 constant fnt-struc-stride
 4 constant fnt-struc-x-size
 6 constant fnt-struc-y-size
 8 constant fnt-struc-ascii-low
10 constant fnt-struc-ascii-high
12 constant fnt-struc-bitmap

: fontdef ( ascii-low ascii-high x-size y-size  -- )
    <builds
    10  h,          ( font-type )
    over over       ( ascii-low ascii-high x-size y-size x-size y-size -- )
    swap 7 + 8 / *  \ number of bytes
    h,              ( stride)
    swap h,         ( x-size )
    h,              ( y-size )
    swap
    h,              ( ascii-low )
    h,              ( ascii-high )

  does>
;

\ --------------------------------------------------------------------------------------------------------
