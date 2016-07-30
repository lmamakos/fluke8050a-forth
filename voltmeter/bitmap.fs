
0 variable rast-xsize
0 variable rast-ysize
0 variable rast-cptr
0 variable rast-x
0 variable rast-y

: set-pixel ( pixel -- )
    0<> if
	\ pixel is turned on
	tft-fg @
\	42 emit
    else
	\ pixel is turned off
	tft-bg @
\	$20 emit
    then
    h>tft  \ push 16 bit pixel value out
;

: rast ( addr xsize-pixels ysize-pixels x-position y-position  -- )
    rast-y !
    rast-x !
    rast-ysize !
    rast-xsize !
    rast-cptr !
    rast-x @  rast-y @  rast-x @ rast-xsize @ + 1-   rast-y @ rast-ysize @ + 1-  setwindow

    +spi
    \ now, iterate over all the bits and write each pixel value out, including
    \ both the foreground and background pixels

    rast-ysize @ 0  do                 \ for each row of pixels
	\ for each row, always start anew with the next byte in the bitmap
\( XXX )	cr 
	rast-cptr @ c@          \ get byte
	1 rast-cptr +!          \ increment to next byte
	$80                     \ mask
	( byte mask )
	rast-xsize @ 0  do  ( byte mask )
	    ?dup 0= if  \ have we walked off the end of the byte?
		drop    \ byte
		rast-cptr @ c@ ( byte )
		1 rast-cptr +! \ increment to next byte
		$80    \ ( byte mask )
	    then
	    ( byte mask )
	    2dup  ( byte mask byte mask )
	    and   ( byte mask pixel )
	    -rot  ( pixel byte mask )
	    shr   \ shift mask bit towards LSB 
	    rot   ( byte mask pixel )

	    set-pixel
	loop  \ per-colume pixel in row
	2drop         \ drop byte and last mask
    loop  \ per row
    -spi
;

: rast-test 42 ascii>bitpattern 8 8 0 0 rast  ;
: bm ( charoffset -- ) 16 * bmow8x16 + 8 16 0 0 rast ;
: digits-test 256 * digits32x64 +  32 64 0 0 rast  ;
: digits-loop-test
    1000 0 do
	i 10 mod 256 * digits32x64 +
	32 64
	i 9 mod 32 *
	i 9 / 3 mod 64 * rast
    loop
;


: showdigit ( n x -- )
    >r
    256 * digits +
    32 64 r> 8 + 0 raster
;

: showdigit-rast ( n x -- )
    >r
    0 max 9 min
    256 * digits32x64 + 32 64
    r> 0 rast
;

: shownum-rast ( u -- )
    0 max 999999 min
    10 /mod 10 /mod 10 /mod 10 /mod 10 /mod
    0 showdigit 32 showdigit 64 showdigit 96 showdigit 128 showdigit  160 showdigit
;
    

: digits-bench-1
    millis
    200000 0 do
	i 100000 mod shownum-rast
    loop
    millis swap - .
;


: digits-bench
    millis
    1000 0 do
	i 10 mod 256 * digits32x64 + 32 64 0 0 rast
    loop
    millis swap - ." rast millis elapsed: " . cr

    millis
    1000 0 do
	i 10 mod 128 showdigit
    loop
    millis swap - ." raster millis elapsed: " . cr
;

: chars-bench
    cr
    millis
    5000 0 do
	i 30 mod 10 * font-x ! 0 font-y !
	i 64 mod 32 + ascii>bitpattern drawcharacterbitmap
    loop
    millis swap - ." old millis: " .

    cr
    millis
    5000 0 do
	i 1 and if
	    i 64 mod 32 + 
	else
	    32
	then
	ascii>bitpattern
	8 8
	( x ) i 30 mod 10 *
	( y ) 20
	rast
    loop
    millis swap - ." rast millis: " .
    cr    
;
