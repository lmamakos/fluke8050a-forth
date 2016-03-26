\
\ PC13 is LED
\ PB6  -- SCL
\ PB7  -- SDA

$50  constant EEPROM
PC13 constant LED

: init-led  ( -- )
  OMODE-PP LED io-mode!
  LED ioc! ;
: led-on     ( -- ) LED ioc! ;
: led-off    ( -- ) LED ios! ;
: led-blink  ( -- ) ;


: ee-nak? ( nak -- ) if ." nak" then ;

: eeaddr ( memoryloc -- page wordaddr)
  dup $ff and swap 8 rshift 7 and ;

: eerd ( addr -- value)
  dup eeaddr EEPROM + i2c-tx ee-nak?  
  >i2c ee-nak?
  eeaddr nip ( drop wordaddr ) EEPROM + i2c-rx ee-nak? 
  true i2c> 
  i2c-stop  ;

: eewr ( val addr -- )
  eeaddr EEPROM + i2c-tx ee-nak?
  >i2c ee-nak?  ( write address )   >i2c ee-nak?  ( write data value )
  i2c-stop ;

: eedump
 1024 0 do
  cr i h.4 ." :"
  16 0 do space
    i j + eerd h.2
  loop
16 +loop ;

init-led
+i2c
