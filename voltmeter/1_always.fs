\ layer "A" - base definitions always present

compiletoflash

include ../mlib/multi.fs
include ../lib/disassembler-m3.fs
include ../mlib/hexdump.fs

\ not happy about needing this here, but debugging code in io-stm32f1.fs wants to know
4 constant io-ports  \ A..D

include ../flib/io-stm32f1.fs
include ../flib/hal-stm32f1.fs

cornerstone <<always>>

