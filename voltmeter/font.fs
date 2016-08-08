\ font rendering definitions
\
\ Copyright (C) 2016
\ Louis Mamakos  <louie@transsys.com>
\

0 variable fnt-x
0 variable fnt-y

\ initial implementation presumes only fixed with fonts, with contiguous array of glyphs available, though not
\ covering entire ASCII set

0 variable fnt-bitmap      \ pointer to base bitmap
0 variable fnt-width       \ width of character in pixels
0 variable fnt-height      \ height of character in pixels
0 variable fnt-stride      \ number of bytes per character
0 variable fnt-min-ascii   \ minimum ASCII character available
0 variable fnt-max-ascii   \ maximum ASCII character available

: fnt-select ( fontbitmap x y -- )
    fnt-height !
    fnt-width !
    fnt-bitmap !
    \ compute stride through bitmap array.  assumes continguous bitmaps and no index table
    fnt-width @ 7 + 8 /  fnt-height @ * fnt-stride !
;

: fnt-goto ( x y -- )
    fnt-y !
    fnt-x !
;

: fnt-ascii>glyph ( c -- c-addr )  \ translate ASCII to address of bitmap
    fnt-min-ascii @ umax fnt-max-ascii @ umin
    fnt-min-ascii @ - fnt-stride @ * fnt-bitmap @ +
;

: fnt-drawbitmap ( c-addr -- )
    fnt-width @ fnt-height @ fnt-x @ fnt-y @ bitmap
    fnt-width @ fnt-x +!     \ increment to next position
    \ XXX check for wraparound or truncation at end of display?
;

: fnt-drawchar ( c -- )
    fnt-ascii>glyph fnt-drawbitmap
;

: fnt-get-first-char ( addr len -- addr   len c ) over c@ ;
: fnt-cut-first-char ( addr len -- addr+1 len-1 ) 1- swap 1+ swap ;
: fnt-drawstring ( addr u x y -- )
    fnt-goto
    begin
	dup 0<>
    while
	    fnt-get-first-char fnt-drawchar fnt-cut-first-char
    repeat
    2drop
;

: select-8x8
    font 8 8 fnt-select
    32  fnt-min-ascii !
    127 fnt-max-ascii !
    0 0 fnt-goto
;

: select-digits
    digits32x64 digits32x64_x  digits32x64_y fnt-select
    [char] 0 fnt-min-ascii !
    [char] 9 fnt-max-ascii !
    0 0 fnt-goto
;
