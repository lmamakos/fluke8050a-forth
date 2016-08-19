\ -*- mode: forth; indent-tabs-mode: nil; -*-

\ tft driver for ILI9341 chip, uses SPI1 hardware

240 constant TFTWIDTH
320 constant TFTHEIGHT

\ ILI 9341 commands - the $100 bit is set as a sentinal to indicate a command

$100 constant ILI9341_NOP                     
$101 constant ILI9341_SWRESET
$104 constant ILI9341_RDDID
$109 constant ILI9341_RDDST

$110 constant ILI9341_SLPIN
$111 constant ILI9341_SLPOUT
$112 constant ILI9341_PTLON
$113 constant ILI9341_NORON

$10A constant ILI9341_RDMODE
$10B constant ILI9341_RDMADCTL
$10C constant ILI9341_RDPIXFMT
$10D constant ILI9341_RDIMGFMT
$10F constant ILI9341_RDSELFDIAG

$120 constant ILI9341_INVOFF
$121 constant ILI9341_INVON
$126 constant ILI9341_GAMMASET
$128 constant ILI9341_DISPOFF
$129 constant ILI9341_DISPON

$12A constant ILI9341_CASET
$12B constant ILI9341_PASET
$12C constant ILI9341_RAMWR
$12E constant ILI9341_RAMRD

$130 constant ILI9341_PTLAR
$136 constant ILI9341_MADCTL
$13A constant ILI9341_PIXFMT

$1B1 constant ILI9341_FRMCTR1
$1B2 constant ILI9341_FRMCTR2
$1B3 constant ILI9341_FRMCTR3
$1B4 constant ILI9341_INVCTR
$1B6 constant ILI9341_DFUNCTR

$1C0 constant ILI9341_PWCTR1
$1C1 constant ILI9341_PWCTR2
$1C2 constant ILI9341_PWCTR3
$1C3 constant ILI9341_PWCTR4
$1C4 constant ILI9341_PWCTR5
$1C5 constant ILI9341_VMCTR1
$1C7 constant ILI9341_VMCTR2

$1DA constant ILI9341_RDID1
$1DB constant ILI9341_RDID2
$1DC constant ILI9341_RDID3
$1DD constant ILI9341_RDID4

$1E0 constant ILI9341_GMCTRP1
$1E1 constant ILI9341_GMCTRN1

\
$80 constant ILI9341_MADCTL_MY
$40 constant ILI9341_MADCTL_MX
$20 constant ILI9341_MADCTL_MV
$10 constant ILI9341_MADCTL_ML
$00 constant ILI9341_MADCTL_RGB
$08 constant ILI9341_MADCTL_BGR
$04 constant ILI9341_MADCTL_MH

\ Color definitions
$0000 constant BLACK             \   0,   0,   0 
$000F constant NAVY              \   0,   0, 128 
$03E0 constant DARKGREEN         \   0, 128,   0 
$03EF constant DARKCYAN          \   0, 128, 128 
$7800 constant MAROON            \ 128,   0,   0 
$780F constant PURPLE            \ 128,   0, 128 
$7BE0 constant OLIVE             \ 128, 128,   0 
$C618 constant LIGHTGREY         \ 192, 192, 192 
$7BEF constant DARKGREY          \ 128, 128, 128 
$001F constant BLUE              \   0,   0, 255 
$07E0 constant GREEN             \   0, 255,   0 
$07FF constant CYAN              \   0, 255, 255 
$F800 constant RED               \ 255,   0,   0 
$F81F constant MAGENTA           \ 255,   0, 255 
$FFE0 constant YELLOW            \ 255, 255,   0 
$FFFF constant WHITE             \ 255, 255, 255 
$FD20 constant ORANGE            \ 255, 165,   0 
$AFE5 constant GREENYELLOW       \ 173, 255,  47 
$F81F constant PINK        

\ needs to be defined previously
\ PB0 constant TFT-D/C  \ register select
\ PA4 constant TFT-CS  \ chip select   (SPI1)
\ PA5 constant TFT-SC  \ serial clock  (SPI1)
\ PA6 constant TFT-DO  \ data from LCD (SPI1)
\ PA7 constant TFT-DI  \ data into LCD (SPI1)

: tft-cmd ( u -- )
    TFT-D/C ioc!   \ D/CX low - command
    +spi           \ SSEL
    >spi
    -spi           \ unselect SSL
    TFT-D/C ios!  \ D/CX high - data
;

: tft-data ( u -- )
    TFT-D/C ios!   \ D/CX high - data
    +spi           \ SSEL
    >spi
    -spi           \ unselect SSL
;
    
: h>tft ( u -- )
    \ write half-word (16 bits) to LCD,  assumes TFT-D/C is already set
    \ and device is selected with +spi active
    dup 8 rshift >spi  >spi
; 

BLACK variable tft-bg
RED   variable tft-fg

create ILI9341-init-commands
$1ef            h, $03 h, $80 h, $02 h,
$1cf            h, $00 h, $c1 h, $30 h,
$1ed            h, $64 h, $03 h, $12 h, $81 h,
$1e8            h, $85 h, $00 h, $78 h,
$1cb            h, $39 h, $2c h, $00 h, $34 h, $02 h,
$1f7            h, $20 h,
$1ea            h, $00 h, $00 h,
ILI9341_PWCTR1  h, $23 h,               \ Power control
ILI9341_PWCTR2  h, $10 h,               \ Power control
ILI9341_VMCTR1  h, $3e h, $28 h,        \ VCM control
ILI9341_VMCTR2  h, $86 h,               \ VCM control
ILI9341_MADCTL  h, ILI9341_MADCTL_MX ILI9341_MADCTL_BGR or h, \ memory access control
ILI9341_PIXFMT  h, $55 h,
ILI9341_FRMCTR1 h, $00 h, $18 h,
ILI9341_DFUNCTR h, $08 h, $82 h, $27 h, \ display function control
$1F2            h, $00 h,               \ 3Gamma function disable
ILI9341_GAMMASET h, $01 h,              \ Gamma curve selected
ILI9341_GMCTRP1 h, $0f h, $31 h, $2b h, $0c h, $0e h, $08 h, $4e h,
( continued  )     $f1 h, $37 h, $07 h, $10 h, $03 h, $0e h, $09 h, $00 h,
ILI9341_GMCTRN1 h, $00 h, $0e h, $14 h, $03 h, $11 h, $07 h, $31 h,
( continued  )     $c1 h, $48 h, $08 h, $0f h, $0c h, $31 h, $36 h, $0f h,
ILI9341_SLPOUT  h,                      \ exit sleep
\ delay here
ILI9341_DISPON  h,
$ffff           h,

: tft-init ( -- )
    OMODE-PP TFT-D/C io-mode!    TFT-D/C ios!
    spi-init
    ili9341-init-commands begin
	dup h@  ( command or data )
	dup $ffff <> while   \ found end of list?
	    dup $100 and if
		tft-cmd
	    else
		tft-data
	    then
	    2+
    repeat 2drop
    200 ms ILI9341_DISPON tft-cmd ;

TFTWIDTH  variable ili9341_width
TFTHEIGHT variable ili9341_height

: ili9341-setRotation ( u -- ) \ 0 - 3 for display rotation selection
    ILI9341_MADCTL tft-cmd
    3 and case
	0 of ILI9341_MADCTL_MX ILI9341_MADCTL_BGR or tft-data
	    TFTWIDTH ili9341_width !
	    TFTHEIGHT ili9341_height !
	endof
	1 of ILI9341_MADCTL_MV ILI9341_MADCTL_BGR or tft-data
	    TFTWIDTH ili9341_height !
	    TFTHEIGHT ili9341_width !
	endof
	2 of ILI9341_MADCTL_MY ILI9341_MADCTL_BGR or tft-data
	    TFTWIDTH ili9341_width !
	    TFTHEIGHT ili9341_height !
	endof
	3 of ILI9341_MADCTL_MX ILI9341_MADCTL_MY or ILI9341_MADCTL_MV or ILI9341_MADCTL_BGR or tft-data
	    TFTWIDTH ili9341_height !
	    TFTHEIGHT ili9341_width !
	endof
    endcase
;

\ set a window in the display that the following 16 bit pixels values will fill.  
: setwindow                           \  ( x0 y0 x1 y1 -- )
    >r                                \  ( x0 y0 x1 -- )    (R: y1 -- )
    swap                              \  ( x0 x1 y0 -- )    (R: y1 -- )
    r>                                \  ( x0 x1 y0 y1 -- ) (R: -- )
    2swap                             \  ( y0 y1 x0 x1 -- )

    ILI9341_CASET tft-cmd             \ column address set
    swap                              \ ( y0 y1 x1 x0 -- )
    dup 8 rshift tft-data             \ send high byte of x0 (col)
    tft-data                          \ send low byte of x0           ( y0 y1 x1 -- )
    dup 8 rshift tft-data             \ send high byte of x1 (col)
    tft-data                          \ send low byte of x1
    
    ILI9341_PASET tft-cmd             \ row address set  ( y0 y1 -- )
    swap                              \ ( y1 y0 -- )
    dup 8 rshift tft-data             \ high byte of y0 (row)
    tft-data                          \ low byte of y0
    dup 8 rshift tft-data             \ high byte of y1 (row)
    tft-data                          \ low byte of y1

    ILI9341_RAMWR tft-cmd             \ write to ram.. and now something should emit pixel data next
                                      \ tft-cmd leaves D/C with "data" selected
;



0 variable bitmap-xsize
0 variable bitmap-ysize
0 variable bitmap-cptr
0 variable bitmap-x
0 variable bitmap-y

: ili9341-set-pixel ( pixel -- )
    if   	\ pixel is turned on
	tft-fg @
    else	\ pixel is turned off
	tft-bg @
    then
    \ h>tft  \ push 16 bit pixel value out
    dup 8 rshift >spi >spi   ( manual inline expansion of h>tft, don't know why inline doesn't work )
;

: bitmap ( addr xsize-pixels ysize-pixels x-position y-position  -- )
    bitmap-y !
    bitmap-x !
    bitmap-ysize !
    bitmap-xsize !
    bitmap-cptr !
    bitmap-x @  bitmap-y @  bitmap-x @ bitmap-xsize @ + 1-   bitmap-y @ bitmap-ysize @ + 1-  setwindow

    +spi
    \ now, iterate over all the bits and write each pixel value out, including
    \ both the foreground and background pixels

    bitmap-ysize @ 0  do                 \ for each row of pixels
	\ for each row, always start anew with the next byte in the bitmap
	bitmap-cptr @ c@          \ get byte
	1 bitmap-cptr +!          \ increment to next byte
	$80                     \ mask
	( byte mask )
	bitmap-xsize @ 0  do  ( byte mask )
	    ?dup 0= if  \ have we walked off the end of the byte?
		drop    \ byte
		bitmap-cptr @ c@ ( byte )
		1 bitmap-cptr +! \ increment to next byte
		$80    \ ( byte mask )
	    then
	    ( byte mask )
	    2dup  ( byte mask byte mask )
	    and   ( byte mask pixel )
	    -rot  ( pixel byte mask )
	    shr   \ shift mask bit towards LSB 
	    rot   ( byte mask pixel )

            \	    ili9341-set-pixel
            if ( pixel on ) tft-fg else ( pixel off ) tft-bg then @
            dup 8 rshift >spi >spi   ( manual inline expansion of h>tft, don't know why inline doesn't work )

	loop  \ per-colume pixel in row
	2drop         \ drop byte and last mask
    loop  \ per row
    -spi
;

\ clear, putpixel, and display are used by the graphics.fs code

: clear
    0 0 ili9341_width @ 1- ili9341_height @ 1- setwindow
    tft-bg @
    +spi
    ili9341_width @ ili9341_height @ * 0 do
        \ strangely, trying to make h>tft "inline" causes a failure
        \ dup h>tft
        dup dup 8 rshift >spi >spi
    loop
    -spi
    drop \ the background color
;
    
\ -----------------------------------------------------------------------------
\ some graphics utility routines
\
: bitmapsize ( x1 y1 x2 y2 -- nbits )
    rot   -     1+ ( compute y extent )
    -rot swap - 1+ ( compute x extent )
    *
;

\ x1 <= x2 and y1 <= y2
: fillrect ( x1 y1 x2 y2 color -- )
    >r            \ squirrel away color
    2over 2over   \ ( x1 y1 x2 y2 x1 y1 x2 y2 )
    bitmapsize >r
    setwindow
    +spi
    r> r> swap 0 do
        \	dup h>tft
        dup dup 8 rshift >spi >spi
    loop
    -spi
    drop
;
: fillrectbg     ( x1 y1 x2 y2 -- )
    tft-bg @
    fillrect
;

: fillrectfg     ( x1 y1 x2 y2 -- )
    tft-fg @
    fillrect
;

: putpixel ( x y -- )  \ set a pixel in display memory
    2dup  ( x y x y -- )
    setwindow
    tft-fg @
    +spi
\    h>tft
    dup 8 rshift >spi >spi ( manual inline expansion of h>tft )
    -spi ;

: display ( -- ) ;  \ update tft from display memory (ignored)

