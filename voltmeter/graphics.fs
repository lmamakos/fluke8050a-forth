\ -*- mode: forth; indent-tabs-mode: nil; -*-

\ graphics primitives
\ adapted from mecrisp-stellaris 2.2.1a (GPL3)
\ needs these words:
\ : clear ( -- ) ;
\ : putpixel ( x y -- ) ;
\ : display ( -- ) ;

\ -------------------------------------------------------------
\  Bresenham line
\ -------------------------------------------------------------

0 variable line-x1   0 variable line-y1
0 variable line-sx   0 variable line-sy
0 variable line-dx   0 variable line-dy
0 variable line-err

: line ( x0 y0 x1 y1 -- )

  line-y1 ! line-x1 !

  over line-x1 @ -   dup 0< if 1 else -1 then line-sx !   abs        line-dx !
  dup  line-y1 @ -   dup 0< if 1 else -1 then line-sy !   abs negate line-dy !
  line-dx @ line-dy @ + line-err !

  begin
    2dup putpixel
    2dup line-x1 @ line-y1 @ d<>
  while
    line-err @ 2* >r
    r@ line-dy @ > if line-dy @ line-err +! swap line-sx @ + swap then
    r> line-dx @ < if line-dx @ line-err +!      line-sy @ +      then
  repeat
  2drop
;

\ -------------------------------------------------------------
\  Bresenham ellipse
\ -------------------------------------------------------------

0 variable ellipse-xm   0 variable ellipse-ym
0 variable ellipse-dx   0 variable ellipse-dy
0 variable ellipse-a    0 variable ellipse-b
0 variable ellipse-a^2  0 variable ellipse-b^2
0 variable ellipse-err

: ellipse-putpixel ( y x -- ) ellipse-xm @ + swap ellipse-ym @ + putpixel ;

: ellipse-step ( -- )
    ellipse-dy @        ellipse-dx @        ellipse-putpixel
    ellipse-dy @ negate ellipse-dx @        ellipse-putpixel
    ellipse-dy @ negate ellipse-dx @ negate ellipse-putpixel
    ellipse-dy @        ellipse-dx @ negate ellipse-putpixel

    ellipse-err @ 2* >r
    r@  ellipse-dx @ 2* 1+ ellipse-b^2 @ * <
      if  1 ellipse-dx +! ellipse-dx @ 2* 1+ ellipse-b^2 @ *
        ellipse-err +! then
    r>  ellipse-dy @ 2* 1- ellipse-a^2 @ * negate >
      if -1 ellipse-dy +! ellipse-dy @ 2* 1+ ellipse-a^2 @ *
        negate ellipse-err +! then
;

: ellipse ( xm ym a b -- )

  0 ellipse-dx ! dup ellipse-dy !

  dup ellipse-b ! dup * ellipse-b^2 !
  dup ellipse-a ! dup * ellipse-a^2 !
  ellipse-ym ! ellipse-xm !

  ellipse-b^2 @ ellipse-b @ 2* 1- ellipse-a^2 @ * - ellipse-err !

  begin
    ellipse-step
    ellipse-dy @ 0<
  until

  ellipse-dx @
  begin
    1+
    dup ellipse-a @ <
  while
    0 over        ellipse-putpixel
    0 over negate ellipse-putpixel
  repeat
  drop
;

: circle ( xm ym r -- ) dup ellipse ;

