\ font rendering definitions
\
\ Copyright (C) 2016
\ Louis Mamakos  <louie@transsys.com>
\

\ initial implementation presumes only fixed with fonts, with contiguous array of glyphs available, though not
\ covering entire ASCII set


\ current character rendering position - should probably have x,y refer to baseline rather than top left..
0 variable fnt-x
0 variable fnt-y


\ cached characteristics of current font.  maybe turn them into accessors from font structure
\ at some point
0 variable fnt-current     \ current font descriptor
0 variable fnt-bitmap      \ pointer to base bitmap
0 variable fnt-width       \ width of character in pixels
0 variable fnt-height      \ height of character in pixels
0 variable fnt-stride      \ number of bytes per character
0 variable fnt-min-ascii   \ minimum ASCII character available
0 variable fnt-max-ascii   \ maximum ASCII character available

: fnt-select ( fontstructure -- )
    dup fnt-struc-type + h@ 10 <> if
	." invalid font structure detected"
	dup 32 dump
    then
    dup fnt-struc-stride + h@      fnt-stride !
    dup fnt-struc-x-size + h@      fnt-width !
    dup fnt-struc-y-size + h@      fnt-height !
    dup fnt-struc-bitmap +         fnt-bitmap !
    dup fnt-struc-ascii-low +  h@  fnt-min-ascii !
    dup fnt-struc-ascii-high + h@  fnt-max-ascii !
    fnt-current !
;

\ change next font rendering position
: fnt-goto ( x y -- )
    fnt-y !
    fnt-x !
;

: fnt-blankchar ( )                      \ blank current character position and advance
    fnt-x @ fnt-y @
    over fnt-width @ +
    over fnt-height @ +
    fillrectbg
    fnt-width @ fnt-x +!     \ increment to next position
;

: fnt-char>glyph ( c -- c-addr )         \ get n'th character glyph bitmap address
    fnt-stride @ * fnt-bitmap @ +
;

: fnt-ascii>glyph ( c -- c-addr )  \ translate ASCII to address of bitmap
    fnt-min-ascii @ umax fnt-max-ascii @ umin
    fnt-min-ascii @ - fnt-char>glyph
;

: fnt-drawbitmap ( c-addr -- )
    fnt-width @ fnt-height @ fnt-x @ fnt-y @ bitmap
    fnt-width @ fnt-x +!     \ increment to next position
                             \ XXX check for wraparound or truncation at end of display?
;

: fnt-drawchar ( c -- )
    dup 0< if
	fnt-blankchar
    else
	fnt-ascii>glyph fnt-drawbitmap
    then
;

\ draw the n'th glyph bitmap
: fnt-drawglyph ( c -- )
    dup 0< if
	fnt-blankchar
    else
	fnt-char>glyph fnt-drawbitmap
    then
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

\ draw string at "current" position
: fnt-puts ( addr u -- )
    fnt-x @ fnt-y @ fnt-drawstring
;
