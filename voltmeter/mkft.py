#!/usr/bin/python

# quick and dirty.  and barely working.

import sys
import string
sys.stdout.write("create bmow8x16 \n")
width = 1
for ch in xrange(0, 128):
    glyph = sys.stdin.read(16)
    if len(glyph) != 16:
        print("Short read of character %d, only %d bytes" % (ch, len(glyph)))
        sys.exit(1)

    sys.stdout.write("( char %3d 0x%02x  " % (ch, ch))
    if chr(ch) in string.printable and ch > 31:
        sys.stdout.write("'%s' ) " % chr(ch))
    else:
        sys.stdout.write("    ) ")

    for b in glyph:
        sys.stdout.write ("$%02x c, " % ord(b))
    sys.stdout.write("\n")

    sys.stdout.write(" \\ ")
    row = 0
    for b in glyph:
        by = ord(b)
        for bit in range(7,-1,-1):
            if (by & 1<<bit):
                sys.stdout.write('#')
            else:
                sys.stdout.write(' ')
        row = row + 1
        if row >= width:
            sys.stdout.write("\n \\ ")
            row = 0

    sys.stdout.write("\n")
        
