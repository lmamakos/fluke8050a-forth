\ derived from TFT_8050A project by
\ Michael Damkier
\ Hamburg, Germany
\ (michael@vondervotteimittiss.com)
\ http://vondervotteimittiss.com/belfry/?p=180

\ // y offset from Y_DIGIT_LG
\ #define OFFSET_MODE_LG 0
\ // y offset from Y_DIGIT_SM
\ #define OFFSET_MODE_SM 0



-1 constant MODE_NONE
1 constant MODE_DC
2 constant MODE_AC
3 constant MODE_REL

\ 40 constant symbolMode_x
\ 32 constant symbolMode_y
\ symbolMode_x 7 + 8 / symbolMode_y * constant symbolMode_sz

1 3 40 32  ( asc-min asc-max x y ) fontdef symbolMode
\   DC 
  $00 c, $00 c, $00 c, $00 c, $00 c,
  $00 c, $00 c, $00 c, $00 c, $00 c,
  $00 c, $3F c, $E0 c, $00 c, $3E c,
  $00 c, $3F c, $F0 c, $00 c, $FF c,
  $00 c, $3F c, $F8 c, $01 c, $FF c,
  $00 c, $3C c, $7C c, $03 c, $E1 c,
  $00 c, $3C c, $3E c, $03 c, $C0 c,
  $00 c, $3C c, $1E c, $07 c, $80 c,
  $00 c, $3C c, $1E c, $07 c, $80 c,
  $00 c, $3C c, $1E c, $07 c, $80 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0F c, $0F c, $00 c,
  $00 c, $3C c, $0E c, $07 c, $80 c,
  $00 c, $3C c, $1E c, $07 c, $80 c,
  $00 c, $3C c, $1E c, $07 c, $80 c,
  $00 c, $3C c, $3E c, $03 c, $C0 c,
  $00 c, $3C c, $7C c, $03 c, $E1 c,
  $00 c, $3F c, $F8 c, $01 c, $FF c,
  $00 c, $3F c, $F0 c, $00 c, $FF c,
  $00 c, $3F c, $C0 c, $00 c, $3E c,
  $00 c, $00 c, $00 c, $00 c, $00 c,

\   AC
  $00 c, $00 c, $00 c, $00 c, $00 c,
  $00 c, $00 c, $00 c, $00 c, $00 c,
  $00 c, $00 c, $E0 c, $00 c, $3E c,
  $00 c, $00 c, $F0 c, $00 c, $FF c,
  $00 c, $00 c, $F0 c, $01 c, $FF c,
  $00 c, $00 c, $F0 c, $03 c, $E1 c,
  $00 c, $01 c, $F0 c, $03 c, $C0 c,
  $00 c, $01 c, $F0 c, $07 c, $80 c,
  $00 c, $01 c, $F8 c, $07 c, $80 c,
  $00 c, $01 c, $F8 c, $07 c, $80 c,
  $00 c, $03 c, $B8 c, $0F c, $00 c,
  $00 c, $03 c, $B8 c, $0F c, $00 c,
  $00 c, $03 c, $BC c, $0F c, $00 c,
  $00 c, $03 c, $BC c, $0F c, $00 c,
  $00 c, $03 c, $9C c, $0F c, $00 c,
  $00 c, $07 c, $9C c, $0F c, $00 c,
  $00 c, $07 c, $1C c, $0F c, $00 c,
  $00 c, $07 c, $1E c, $0F c, $00 c,
  $00 c, $07 c, $1E c, $0F c, $00 c,
  $00 c, $07 c, $0E c, $0F c, $00 c,
  $00 c, $0F c, $0E c, $0F c, $00 c,
  $00 c, $0F c, $0E c, $0F c, $00 c,
  $00 c, $0F c, $FF c, $0F c, $00 c,
  $00 c, $0F c, $FF c, $07 c, $80 c,
  $00 c, $1F c, $FF c, $07 c, $80 c,
  $00 c, $1E c, $07 c, $07 c, $80 c,
  $00 c, $1E c, $07 c, $03 c, $C0 c,
  $00 c, $1C c, $07 c, $83 c, $E1 c,
  $00 c, $1C c, $07 c, $81 c, $FF c,
  $00 c, $3C c, $07 c, $80 c, $FF c,
  $00 c, $3C c, $03 c, $80 c, $3E c,
  $00 c, $00 c, $00 c, $00 c, $00 c,

\   Rel
  $00 c, $00 c, $00 c, $00 c, $00 c,
  $00 c, $00 c, $00 c, $00 c, $00 c,
  $FF c, $00 c, $FF c, $C3 c, $C0 c,
  $FF c, $C0 c, $FF c, $C3 c, $C0 c,
  $FF c, $E0 c, $FF c, $C3 c, $C0 c,
  $F1 c, $E0 c, $F0 c, $03 c, $C0 c,
  $F1 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F1 c, $E0 c, $F0 c, $03 c, $C0 c,
  $F7 c, $E0 c, $F0 c, $03 c, $C0 c,
  $FF c, $C0 c, $FF c, $C3 c, $C0 c,
  $FF c, $80 c, $FF c, $C3 c, $C0 c,
  $FF c, $00 c, $FF c, $C3 c, $C0 c,
  $F7 c, $80 c, $F0 c, $03 c, $C0 c,
  $F7 c, $80 c, $F0 c, $03 c, $C0 c,
  $F7 c, $80 c, $F0 c, $03 c, $C0 c,
  $F3 c, $C0 c, $F0 c, $03 c, $C0 c,
  $F3 c, $C0 c, $F0 c, $03 c, $C0 c,
  $F3 c, $C0 c, $F0 c, $03 c, $C0 c,
  $F1 c, $E0 c, $F0 c, $03 c, $C0 c,
  $F1 c, $E0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $F0 c, $03 c, $C0 c,
  $F0 c, $F0 c, $FF c, $C3 c, $FF c,
  $F0 c, $78 c, $FF c, $C3 c, $FF c,
  $F0 c, $78 c, $FF c, $C3 c, $FF c,
  $00 c, $00 c, $00 c, $00 c, $00 c,

calign

\ ------------------------------------------------------------------------------------
\ 19 constant symbolMode_small_x
\ 14 constant symbolMode_small_y

1 3 19 14  ( asc-min asc-max x y ) fontdef symbolMode_small
\   DC
    $FC c, $03 c, $80 c,
    $FE c, $07 c, $C0 c,
    $C3 c, $0C c, $60 c,
    $C3 c, $0C c, $60 c,
    $C1 c, $98 c, $00 c,
    $C1 c, $98 c, $00 c,
    $C1 c, $98 c, $00 c,
    $C1 c, $98 c, $00 c,
    $C1 c, $98 c, $00 c,
    $C1 c, $98 c, $00 c,
    $C3 c, $0C c, $60 c,
    $C3 c, $0C c, $60 c,
    $FE c, $07 c, $C0 c,
    $FC c, $03 c, $80 c,

\   AC
    $1C c, $03 c, $80 c,
    $1C c, $07 c, $C0 c,
    $3E c, $0C c, $60 c,
    $36 c, $0C c, $60 c,
    $36 c, $18 c, $00 c,
    $63 c, $18 c, $00 c,
    $63 c, $18 c, $00 c,
    $63 c, $18 c, $00 c,
    $7F c, $18 c, $00 c,
    $FF c, $98 c, $00 c,
    $C1 c, $8C c, $60 c,
    $C1 c, $8C c, $60 c,
    $C1 c, $87 c, $C0 c,
    $C1 c, $83 c, $80 c,

calign

\ ------------------------------------------------------------------------------------

\ 36 constant lobatt_x
\ 16 constant lobatt_y
0 0 36 16  ( asc-min asc-max x y ) fontdef lowbatt
    $FF c, $FF c, $FF c, $FF c, $C0 c,
    $FF c, $FF c, $FF c, $FF c, $C0 c,
    $C0 c, $00 c, $00 c, $00 c, $C0 c,
    $DF c, $FF c, $E0 c, $00 c, $C0 c,
    $DF c, $FF c, $E0 c, $00 c, $CD c,
    $DF c, $FF c, $F0 c, $00 c, $C5 c,
    $DF c, $FF c, $F0 c, $00 c, $F3 c,
    $DF c, $FF c, $F8 c, $00 c, $FF c,
    $DF c, $FF c, $F8 c, $00 c, $F3 c,
    $DF c, $FF c, $FC c, $00 c, $F7 c,
    $DF c, $FF c, $FC c, $00 c, $CB c,
    $DF c, $FF c, $FE c, $00 c, $C5 c,
    $DF c, $FF c, $FE c, $00 c, $CC c,
    $C0 c, $00 c, $00 c, $00 c, $C5 c,
    $FF c, $FF c, $FF c, $FF c, $C0 c,
    $FF c, $FF c, $FF c, $FF c, $C0 c,

calign

\ 64 constant setZ_x
\ 32 constant setZ_y
0 0 64 32  ( asc-min asc-max x y ) fontdef setZ
    $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c,
    $0F c, $C0 c, $FF c, $E7 c, $FF c, $E0 c, $1F c, $FF c,
    $3F c, $E0 c, $FF c, $E7 c, $FF c, $E0 c, $1F c, $FE c,
    $7F c, $E0 c, $FF c, $E7 c, $FF c, $E0 c, $1F c, $FE c,
    $7C c, $60 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $3E c,
    $F8 c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $3C c,
    $F0 c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $3C c,
    $F0 c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $7C c,
    $F0 c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $78 c,
    $F0 c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $78 c,
    $F8 c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $F8 c,
    $F8 c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $F0 c,
    $7C c, $00 c, $F0 c, $00 c, $3C c, $00 c, $00 c, $F0 c,
    $7E c, $00 c, $F0 c, $00 c, $3C c, $00 c, $01 c, $F0 c,
    $3F c, $00 c, $FF c, $C0 c, $3C c, $00 c, $01 c, $E0 c,
    $1F c, $80 c, $FF c, $C0 c, $3C c, $00 c, $01 c, $E0 c,
    $0F c, $C0 c, $FF c, $C0 c, $3C c, $00 c, $03 c, $E0 c,
    $07 c, $E0 c, $F0 c, $00 c, $3C c, $00 c, $03 c, $C0 c,
    $03 c, $F0 c, $F0 c, $00 c, $3C c, $00 c, $03 c, $C0 c,
    $01 c, $F0 c, $F0 c, $00 c, $3C c, $00 c, $07 c, $C0 c,
    $00 c, $F8 c, $F0 c, $00 c, $3C c, $00 c, $07 c, $80 c,
    $00 c, $78 c, $F0 c, $00 c, $3C c, $00 c, $07 c, $80 c,
    $00 c, $78 c, $F0 c, $00 c, $3C c, $00 c, $0F c, $80 c,
    $00 c, $78 c, $F0 c, $00 c, $3C c, $00 c, $0F c, $00 c,
    $00 c, $78 c, $F0 c, $00 c, $3C c, $00 c, $0F c, $00 c,
    $C0 c, $78 c, $F0 c, $00 c, $3C c, $00 c, $1F c, $00 c,
    $E0 c, $F0 c, $F0 c, $00 c, $3C c, $00 c, $1E c, $00 c,
    $F1 c, $F0 c, $F0 c, $00 c, $3C c, $00 c, $1E c, $00 c,
    $7F c, $E0 c, $FF c, $E0 c, $3C c, $00 c, $3F c, $FE c,
    $3F c, $C0 c, $FF c, $E0 c, $3C c, $00 c, $3F c, $FE c,
    $1F c, $80 c, $FF c, $E0 c, $3C c, $00 c, $3F c, $FE c,
    $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c,

calign

\ @EOF

