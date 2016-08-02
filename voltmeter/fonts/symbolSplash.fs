\ derived from TFT_8050A project by
\ Michael Damkier
\ Hamburg, Germany
\ (michael@vondervotteimittiss.com)

\ FLUKE 8050A

120 constant splashFlukeX
41  constant splashFlukeY
create splashFluke
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $F0 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $0F c,
  $F0 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $0F c,
  $F0 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $0F c,
  $F1 c, $FF c, $FF c, $8E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $E0 c, $0F c, $C7 c, $FF c, $FF c, $0F c,
  $F1 c, $FF c, $FF c, $8E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $E0 c, $3F c, $07 c, $FF c, $FF c, $0F c,
  $F1 c, $FF c, $FF c, $8E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $E0 c, $7E c, $07 c, $FF c, $FF c, $0F c,
  $F1 c, $E0 c, $00 c, $0E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $E1 c, $F8 c, $07 c, $80 c, $00 c, $0F c,
  $F1 c, $E0 c, $00 c, $0E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $E7 c, $F0 c, $07 c, $80 c, $00 c, $0F c,
  $F1 c, $E0 c, $00 c, $0E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $EF c, $C0 c, $07 c, $80 c, $00 c, $0F c,
  $F1 c, $FF c, $FF c, $8E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $FF c, $00 c, $07 c, $FF c, $FF c, $0F c,
  $F1 c, $FF c, $FF c, $8E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $FE c, $00 c, $07 c, $FF c, $FF c, $0F c,
  $F1 c, $FF c, $FF c, $8E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $FE c, $00 c, $07 c, $FF c, $FF c, $0F c,
  $F1 c, $E0 c, $00 c, $0E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $FF c, $C0 c, $07 c, $80 c, $00 c, $0F c,
  $F1 c, $E0 c, $00 c, $0E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $EF c, $E0 c, $07 c, $80 c, $00 c, $0F c,
  $F1 c, $E0 c, $00 c, $0E c, $00 c, $07 c, $80 c, $01 c, $E1 c, $E3 c, $F8 c, $07 c, $80 c, $00 c, $0F c,
  $F1 c, $E0 c, $00 c, $0F c, $FF c, $E7 c, $FF c, $FF c, $E1 c, $E0 c, $FE c, $07 c, $FF c, $FF c, $0F c,
  $F1 c, $E0 c, $00 c, $0F c, $FF c, $E3 c, $FF c, $FF c, $C1 c, $E0 c, $3F c, $07 c, $FF c, $FF c, $0F c,
  $F1 c, $E0 c, $00 c, $0F c, $FF c, $E3 c, $FF c, $FF c, $C1 c, $E0 c, $0F c, $C7 c, $FF c, $FF c, $0F c,
  $F0 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $0F c,
  $F0 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $0F c,
  $F0 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $0F c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c,
  $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c,
  $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c, $00 c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,
  $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c, $FF c,

  $AA c, $77 c, $AA c, $77 c, $AA c, $77 c,
calign

\ @EOF
