\ -*- mode: forth; indent-tabs-mode: nil; -*-

: bitmap-test 42 ascii>bitpattern 8 8 0 0 bitmap  ;
: bm ( charoffset -- ) 16 * bmow8x16 + 8 16 0 0 bitmap ;
: digits-test 256 * digits32x64 +  32 64 0 0 bitmap  ;
: digits-loop-test
    1000 0 do
	i 10 mod 256 * digits32x64 +
	32 64
	i 9 mod 32 *
	i 9 / 3 mod 64 * bitmap
    loop
;

: showdigit ( n x -- )
    >r
    256 * digits +
    32 64 r> 8 + 0 bitmap
;

: showdigit-bitmap ( n x -- )
    >r
    0 max 9 min
    256 * digits32x64 + 32 64
    r> 0 bitmap
;

: shownum-bitmap ( u -- )
    0 max 999999 min
    10 /mod 10 /mod 10 /mod 10 /mod 10 /mod
    0 showdigit 32 showdigit 64 showdigit 96 showdigit 128 showdigit  160 showdigit
;
    

: digits-bench-1
    millis
    200000 0 do
	i 100000 mod shownum-bitmap
    loop
    millis swap - .
;


: digits-bench
    millis
    1000 0 do
	i 10 mod 256 * digits32x64 + 32 64 0 0 bitmap
    loop
    millis swap - ." bitmap millis elapsed: " . cr

    millis
    1000 0 do
	i 10 mod 128 showdigit
    loop
    millis swap - ." bitmap millis elapsed: " . cr
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
	bitmap
    loop
    millis swap - ." bitmap millis: " .
    cr    
;
