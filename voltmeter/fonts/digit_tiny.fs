\ derived from TFT_8050A project by
\ Michael Damkier
\ Hamburg, Germany
\ (michael@vondervotteimittiss.com)


8  constant digit_tiny_x
13 constant digit_tiny_y
digit_sm_x 7 + 8 / digit_sm_y * constant digit_sm_sz

create digit_tiny

\  0
const uint8_t digit_tiny_0[] PROGMEM = {
  $38 c,
  $44 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $44 c,
  $38 c,

\  1
  $08 c,
  $08 c,
  $18 c,
  $28 c,
  $48 c,
  $08 c,
  $08 c,
  $08 c,
  $08 c,
  $08 c,
  $08 c,
  $08 c,
  $08 c,

\  2
  $38 c,
  $44 c,
  $82 c,
  $82 c,
  $02 c,
  $02 c,
  $04 c,
  $08 c,
  $10 c,
  $20 c,
  $40 c,
  $80 c,
  $FE c,

\  3
  $38 c,
  $44 c,
  $82 c,
  $02 c,
  $02 c,
  $04 c,
  $18 c,
  $04 c,
  $02 c,
  $82 c,
  $82 c,
  $44 c,
  $38 c,

\  4
  $04 c,
  $04 c,
  $0C c,
  $0C c,
  $14 c,
  $24 c,
  $24 c,
  $44 c,
  $84 c,
  $FE c,
  $04 c,
  $04 c,
  $04 c,

\  5
  $7E c,
  $40 c,
  $40 c,
  $80 c,
  $F8 c,
  $C4 c,
  $82 c,
  $02 c,
  $02 c,
  $82 c,
  $82 c,
  $44 c,
  $38 c,

\  6
  $3C c,
  $42 c,
  $82 c,
  $80 c,
  $80 c,
  $B8 c,
  $C4 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $44 c,
  $38 c,

\  7
  $FE c,
  $02 c,
  $04 c,
  $08 c,
  $08 c,
  $10 c,
  $10 c,
  $20 c,
  $20 c,
  $20 c,
  $40 c,
  $40 c,
  $40 c,

\  8
  $38 c,
  $44 c,
  $82 c,
  $82 c,
  $82 c,
  $44 c,
  $38 c,
  $44 c,
  $82 c,
  $82 c,
  $82 c,
  $44 c,
  $38 c,

\  9
  $38 c,
  $44 c,
  $82 c,
  $82 c,
  $82 c,
  $82 c,
  $46 c,
  $3A c,
  $02 c,
  $02 c,
  $82 c,
  $84 c,
  $78 c,

\ @EOF
