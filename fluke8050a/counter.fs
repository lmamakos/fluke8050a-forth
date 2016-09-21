millis variable start

: putnum
  0 <# # # # # # # # #S #>  fnt-puts
;

\ --------------------------------------------------------------------

                         5  constant bar-min-x
                       300  constant bar-length
    bar-length bar-min-x +  constant bar-max-x

                     2000   constant maxval
                  0 maxval  2constant maxval.f
 0 bar-length  maxval.f f/  2constant scale-factor.f

: scale-to-bar ( zeroTo2K -- )
   maxval min   0 max   ( ensure between 0 and maxval )
   0 swap               ( convert t fixed point )
   scale-factor.f  f*
   0,5 d+
   swap drop
;

: bar ( length -- )
  maxval min   0 max   ( ensure between 0 and maxval )
  dup
  bar-min-x +   210
  bar-min-x 200
  2swap red fillrect

  bar-min-x + 1 + 200
  bar-min-x  bar-max-x + 210 black fillrect
;


: testerbar ( value -- )
  2000 0 do
    i scale-to-bar bar
  3 +loop

  0 2000 do
    i scale-to-bar bar
  -2 +loop
  ;

  
: doit
digit_lg fnt-select
clear
begin
  millis dup 
  100000000 mod
  orange tft-fg !
  0 0 fnt-goto putnum

  pink tft-fg !
  0 80 fnt-goto
  start @ -  dup putnum

  2000 mod scale-to-bar bar

  key?  until
;
