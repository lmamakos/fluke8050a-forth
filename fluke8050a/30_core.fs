( layer "C" - core hardware drivers and librarys )

<<<always>>>

compiletoflash

( font definition words )
include fontdefs.fs

( include all the fonts )

( include fonts/8x16.fs )
include fonts/8x16.fs

( include fonts/digit_lg.fs )
include fonts/digit_lg.fs

( include fonts/digit_sm.fs )
include fonts/digit_sm.fs

( include fonts/digit_tiny.fs )
( not presently needed )
include fonts/digit_tiny.fs

( include fonts/symbolMode.fs )
include fonts/symbolMode.fs

( include fonts/symbolSign.fs )
include fonts/symbolSign.fs

( include fonts/symbolUnit.fs ) 
include fonts/symbolUnit.fs

( include fonts/symbolSplash.fs ) 
include fonts/symbolSplash.fs

( include fonts/digits32x64.fs )
( not presently needed - may want to omit if only 64K of flash available )
include fonts/digits32x64.fs

cornerstone <<<fonts>>>
