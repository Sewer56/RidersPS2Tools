// ====== HOOK CODE ======

address $0010A438

// Backup original parameters
addiu sp, sp, -64
sq ra, $0000(sp)
sq a0, $0010(sp)
sq a1, $0020(sp)
sq a2, $0030(sp)

setreg a0, :text
setreg a1, $9E7CE0
lw a1, 0(a1)
setreg a3, :dumpedflag

jal $105248 //sceprintf
nop

// Wait until dumper program dumps
setreg a3, :dumpedflag
loopflag:

// Sleep the thread so that the text can be printed

nop

lw t0, 0(a3)
blez t0, :loopflag
nop

// Reset the flag
addiu a2, zero, zero
sw a2, 0(a3)

// Restore original parameters
lq ra, $0000(sp)
lq a0, $0010(sp)
lq a1, $0020(sp)
lq a2, $0030(sp)
addiu sp, sp, 64

// Re-insert replaced instruction where our jmp was placed and return.
exit:
lui a0, 0x4D
j $33A570
nop

dumpedflag:
hexcode $0

text:
print "Addr: %d , Size: %d, DumpFlagAddr: %d \n"