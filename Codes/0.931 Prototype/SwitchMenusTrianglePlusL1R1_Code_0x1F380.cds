// ====== HOOK CODE ======

address $0011F300

// Note: v1 and t0 currently not in use so no need to backup.

// Registers
// s0 = 4D8C00 (gsActivePad)
// button info struct: https://github.com/Sewer56/Sewer56.SonicRiders/blob/94f5962a7589f913f679e774cd0cc50ab556322c/Sewer56.SonicRiders/Structures/Input/PlayerInput.cs (from PC Disassembly)
// 4D8C00 maps to offset 0x8 of the github struct and is a copy used for menus.

// Get current pressed inputs
lw v1, (0xC)(s0)

// Check if L1 + Triangle pressed
andi t0, v1, 0x208
blez t0, :checkR1

  // Decrement Debug Menu
  lw t0, (0x50)(s1) // Get Debug Menu Value
  addi t0, t0, -1
  
  // Check if before first menu (1) and loop
  bgtz t0, :checkL1Add
    addi t0, zero, 0x3F // Set to last menu

  checkL1Add:
  sw t0, (0x50)(s1)

// Check if R1 +  Triangle pressed
checkR1:
andi t0, v1, 0x408
blez t0, :exit
  
  // Increment Debug Menu
  lw t0, (0x50)(s1) // Get Debug Menu Value
  addi t0, t0, 1 
  
  // Check if after last menu and loop
  slti t1, t0, 0x40 // if less than 3F
  bgtz t1, :checkR1Add
    addi t0, zero, 0x01 // Set to last menu
  
  checkR1Add:
  sw t0, (0x50)(s1)

// Re-insert replaced instruction where our jmp was placed and return.
exit:
lw v1, (0xC)(s0)
j $128dd8

