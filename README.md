# ShaRPG [![Build Status](https://travis-ci.com/ashleydavies/Sharpg.svg?branch=develop)](https://travis-ci.com/ashleydavies/Sharpg)
ShaRPG, or Sharp RPG, is an excellent example of bad puns, and a total re-write of my [unfinished] Java RPG engine, but in C#.

It is not compatible with the data, as many of the XML formats have changed.

It runs a simple 'bytecode' (integer array) VM as a scripting language, with a rudimentary ASM-style language and a small compiler to morph that into acceptable code.

# Status

## Completed & information
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
   * Dialog does not yet have conditions but these will be implemented based on code running atop the VM
   * Dialog is presented in a GUI form and engageable through clicking on NPCs that have dialog associated
   * Dialog can be navigated through clicking replies in the GUI
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
 * Inventory
   * You can view your inventory with a GUI
   * You can move items around in your inventory
   
## In development

Currently, I am working on a higher-level scripting language for the engine, primarily for fun rather than because it will be more helpful than actually implementing important game features.

You can see the progress in ScriptCompiler/, ShaRPG/VM/ and ScriptCompilerTests/ for CI (the first CI in the project!)

A snippet of code that currently works, compiling into the assembly and then being assembled into the byte code, is:

~~~
print "Hello, world";

function void testt() {
    print "Test 2 called!";
}

function void test() {
    print "Called a function";
    print 5;
    testt();
}

print 5;
test();
print "And returned!";
test();
print "And returned again!";
print 5 + 2 * 6 / 3 - 4 + 8 * (2 + 1) / (3 + 1 - 1);
~~~

As you can see, it's currently a fairly bare-bones primitive language. It also currently lacks variables, since the only stack in play is a stack machine for expressions.

I am hoping to move away from the stack machine to a register machine architecture for the VM, and that has already happened at a higher level (the assembly code assumes a register machine), but
the assembler currently manipulates this into code for the underlying stack machine VM, which works for now.

Current development is focussing on adding the heap and stack, which will enable variables and function parameters.

After this, I hope to flesh out the typing and try to make functions first class types and enable user type definitions.

Also on the agenda is a built-in library which is compiled with your scripts depending on which bits of it you use; it's likely this will be through some kind of C-style #include syntax.

The hope is that this scripting language can enable very flexible dialog, map behaviour, and AI behaviour, while also allowing specialised built-ins and syntax which would be trickier with
a typical dynamic scripting language I could plug and play.

Also, I find it much more fun to do this from scratch than set up bindings for Lua/Python/Ruby/your favourite scripting language here.

## Upcoming / planned

The following are (very broad) features I intend to implement in the future:

 * Inventory system
   * Items should be able to be on the floor, picked up, and dropped
 * NPC inventory system
   * Should use a component based system to share logic with player
   * But player should have an inventory GUI and NPCs should have a shop component accessible through dialog
 * World map
   * Going to the border of the map should move you to the world map, where you can travel to the next location or go to previous locations
   * Should load from a dynamic format
 * Combat
   * Combat with NPCs. NPCs should be able to be friendly and hostile. Hostile should engage on sight, whereas friendly will just talk to you. You may attack friendly NPCs which will turn them (and possibly their friends) hostile.
 * Companions
   * You should be able to recruit companions, who follow you around and may also have their own inventory
 * Quests
   * You should be able to see and accept quests, potentially from NPCs or events, and complete them
 * Menu
   * There should be a menu screen
   * Settings
 * Sound
   * There should be sounds for walking, ambience, etc...
 * Proper art
   * Take a look at the resources/images folder... Need I say more...
 * Saving/loading
 * Level system with abilities to level up e.g. agility, swordsmanship
 * Proper scripting language for the VM
 * Nicer NPC editor (There is an existing dialog editor in my old RPG project which I can definitely repurpose)
 * Loading screens
   * Currently everything loads instantly, but this will likely change throughout development. I will likely do a loading screen when this actually presents a problem, though keeping it in mind to avoid embedding too much loading logic too strongly into the main synchronous game loop may be a good idea.
 * ... An actual game!
   * If everything above is implemented, the only thing left to do is actually create games on top of this engine. Finally! :)
     * Though at the current rate I am developing, I'll probably die of old age before I get around to this

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
