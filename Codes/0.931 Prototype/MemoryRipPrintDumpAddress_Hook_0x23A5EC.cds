
// ====== HOOK APPLY ======

// Insert a jump to our custom function.
// This jump happens at a branch only executed when on topmost debug menu item (close), it checks for exit.
address $0033A56C
j $0010A438