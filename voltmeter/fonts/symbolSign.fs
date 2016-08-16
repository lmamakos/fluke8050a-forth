\ derived from TFT_8050A project by
\ Michael Damkier
\ Hamburg, Germany
\ (michael@vondervotteimittiss.com)


\ // y offset from Y_DIGIT_LG
\ #define OFFSET_SIGN_LG 35
\ // y offset from Y_DIGIT_SM
\ #define OFFSET_SIGN_SM 16


-1 constant SIGN_NONE
1  constant SIGN_PLUS
2  constant SIGN_MINUS

\ 32 constant symbolSign_x
\ 32 constant symbolSign_y
\ symbolSign_x 7 + 8 / symbolSign_y * constant symbolSign_sz

1 2 32 32  ( asc-min asc-max x y ) fontdef symbolSign
  \   plus
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $0F c, $E0 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,

  \   minus
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $FF c, $FF c, $FF c, $FE c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,
    $00 c, $00 c, $00 c, $00 c,

calign

\ 12 constant symbolSignSm_x
\ 12 constant symbolSignSm_y
\ symbolSignSm_x 7 + 8 / symbolSignSm_y * constant symbolSignSm_sz

1 2 12 12  ( asc-min asc-max x y ) fontdef symboleSignSm
  \   plus
    $0E c, $00 c,
    $0E c, $00 c,
    $0E c, $00 c,
    $0E c, $00 c,
    $FF c, $E0 c,
    $FF c, $E0 c,
    $FF c, $E0 c,
    $0E c, $00 c,
    $0E c, $00 c,
    $0E c, $00 c,
    $0E c, $00 c,
    $00 c, $00 c,

  \   minus
    $00 c, $00 c,
    $00 c, $00 c,
    $00 c, $00 c,
    $00 c, $00 c,
    $FF c, $E0 c,
    $FF c, $E0 c,
    $FF c, $E0 c,
    $00 c, $00 c,
    $00 c, $00 c,
    $00 c, $00 c,
    $00 c, $00 c,
    $00 c, $00 c,

calign

\ @EOF
