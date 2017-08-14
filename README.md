# ShaRPG
ShaRPG, or Sharp RPG, is an excellent example of bad puns, and a total re-write of my [unfinished] Java RPG engine, but in C#.

It is not compatible with the data, as many of the XML formats have changed.

It runs a simple 'bytecode' (integer array) VM as a scripting language, with a rudimentary ASM-style language and a small compiler to morph that into acceptable code.

# Status

The following show the completed parts of / technical information about each main component:

 * Core information
   * Uses XML for maps, entities, etc.
   * Wraps around the SFML framework (ideally only the Utils namespace should ever reference SFML so the underlying library can be swapped out, but it leaks into a small handful classes currently for convenience)
 * Map loading
   * Uses Tiled map editor, parsing the XML.
   * Player start location loaded from map
 * Entities
   * Entities based on a component system with inter-component messaging
   * Entities can follow a predetermined path embedded in the XML of the map
   * Entities spawn location is loaded dynamically from the map
   * Entities follow A\* algorithm for pathing
 * Dialog
   * Dialog is embedded inside each entity's XML, and allows an uncapped number of prompts and replies.
   * Replies may lead to different prompts (with different replies), end the discussion, or execute arbitrary VM code (See scripting)
   * Dialog does not yet have conditions but these will be implementedd based on code running atop the VM
   * Dialog is not yet visible in-game -- this is currently being worked on
 * Scripting / VM
   * Crude assembly-style language compiles into integer-based code format
   * String data can be embedded and referenced
   * Conditionals, registers, stack, and more currently functional
   * Plans for a full scripting language compiling into this assembly language (see relevant branch - not much progress yet)
 * AI dialog
 * GUI
   * Contains a powerful custom GUI system
   * Components include:
     * GUI window (renders a border and background)
     * Text container (with custom text wrapping)
     * Vertical flow container (renders children one below the other; height is sum of children's height)
     * Column container (dual column, one fixed, other stretches to fill space. Height is max of both column heights)
     * Padding container (simply adds padding around child component)

# Scripting

Currently the scripting must be done in an unpleasant assembly-style language. An example file which loops five times is shown below, along with it's compiled version (with strings embedded). This can be executed as part of dialog, and eventually as part of map triggers, and all sorts of other events.

~~~~
.data
STRING loopmsg Looping...
STRING endmsg Finished looping!
.text
MOV r1 0
LABEL START
CMP r1 5
JEQ END

PRINT loopmsg

ADD r6 1

INC r1
JMP START
LABEL END

PRINT endmsg
~~~~

When compiled, the following form is produced:

`34,3,15,11,32,76,111,111,112,105,110,103,46,46,46,18,32,70,105,110,105,115,104,101,100,32,108,111,111,112,105,110,103,33,1,0,8,1,9,1,1,5,12,15,62,1,0,20,9,6,1,1,2,8,6,9,1,6,8,1,13,38,1,1,20`

The first few numbers contain metadata about the data stored -- lengths, locations, etc. Then follows the data (character encoded), and then finally the code follows. The preceding 34 indicates execution should begin at the 34th index.

This can be embedded where mentioned. The execution is shown below:

~~~~
 Looping...
 Looping...
 Looping...
 Looping...
 Looping...
 Finished looping!
~~~~

# Warning
The default branch of the repository is on develop, since there is no stable release.
