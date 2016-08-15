\ -*- mode: forth; indent-tabs-mode: nil; -*-
\
\ device wants SPI data LSB first, on the rising edge of the clock
\

include spi-stm32f1.fs

\         A/0
\         ---
\    F/5 |   | B/1
\         --- G/6
\    E/4 |   | C/2
\         ---
\         D/3
\
\
create 7segment-digit-map ( digits 0 - 9, A - F )
   $3f c, $06 c, $5b c, $4f c, $66 c, $6d c, $7d c, $07 c,
   $7f c, $6f c, $77 c, $7c c, $58 c, $5e c, $79 c, $71 c,

calign

: spi-7map ( int -- bitmap )
    $0f and 7segment-digit-map + c@
;

: spi-led-cmd
    +spi >spi -spi
; 

: spi-led-data ( data addr -- )
    +spi >spi >spi -spi
;

$40 constant pt6961-autoinc

: spi-led-digits ( d4 d3 d2 d1 -- )
    pt6961-autoinc spi-led-cmd
    +spi
    $c0 >spi
    >spi    \ $CO first digit
    0 >spi  \ $C1 dummy
    >spi    \ $C2 second digit
    0 >spi  \ $C3 dummy
    >spi    \ $C4 third digit
    0 >spi  \ $C5 dummy
    >spi    \ $C6 fourth digit
    -spi
;

: spi-led-clear
    0 0 0 0 spi-led-digits
;

: spi-led-init
    spi-slow-init
    $02 spi-led-cmd  \ DISPLAY_6x12
    spi-led-clear
    $8F spi-led-cmd  \ DISPLAY_14_16
;

: spi-led-int ( i -- )
    10 /mod swap spi-7map $c6 spi-led-data
    10 /mod swap spi-7map $c4 spi-led-data
    10 /mod swap spi-7map $c2 spi-led-data
    10 /mod swap spi-7map $c0 spi-led-data
    drop
;

