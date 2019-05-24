# Human Resources

General purpose Discord bot written in C#

## Default guild settings

* Prefix - !
* Mark - ⭐
* Marklist - false
* Welcome
  * Enabled - false
  * Time - 10
  * Rank - @everyone
  * Message - Welcome! You'll gain full privileges soon.

## Features

### Settings - Get and set guild specific settings

* **s**etting**s** - Returns the bot settings for the guild
* **s**etting**s** **p**refi**x** <char> - Set command prefix for the guild (**admin**)
* **s**etting**s** **m**ar**k** <char> - Set mark for the guild (**admin**)
* **s**etting**s** **m**ark**l**ist <bool> - Set if marked members are also blacklisted (**admin**)
* **s**etting**s** **w**elcome - Returns all the welcome settings for the guild
* **s**etting**s** **w**elcome **e**nable <bool> - Enable or disable guild welcome functionality (**admin**)
* **s**etting**s** **w**elcome **t**ime <uint> - Set the guild welcome time (**admin**)
* **s**etting**s** **w**elcome **r**ole <string/uint> - Set the first role of new users (**admin**)
* **s**etting**s** **w**elcome **m**essage <...string> - Set the new guild welcome message (**admin**)

### Administration - Standard administrative guild functions

* **k**ick <string/uint> ...reason - Kicks user (**kick**)
* **b**an <string/uint> ...reason - Bans user (**ban**)
* **v**oice **k**ick <string/uint> ...reason - Voice kick user (**kick**)
* **v**oice **m**ute <string/uint> ...reason - Mute user (**mute**)
* **v**oice **m**ute remove <string/uint> - Removes mute from user (**mute**)
* **v**oice **d**eafen <string/uint> ...reason - Deafen user (**deafen**)
* **v**oice **d**eafen **r**emove <string/uint> - Removes deafen from user (**deafen**)
* **bl**acklist <string/uint> ...reason - Disable bot usage of user (**admin**)
* **bl**acklist **r**emove <string/uint> - Removes user from blacklist (**admin**)
* **t**imeout <string/uint> uint=10 - Set user on timeout (**admin**)
* **t**imeout **r**emove <string/uint> - Removes user from timeout (**admin**)
* **m**ar**k** <string/uint> ...reason - Set guild mark on user (**admin**)
* **m**ar**k** **r**emove <string/uint> - Remove guild mark from user (**admin**)

### Twitter - Various Twitter related functions

* **tw**itter <string/uint> - Returns information on Twitter user

## Key

### Parameters

* <> - Required
* ... - Can contain spaces
* (**ABC**) - Requires permission ABC
* ABC=X - Parameter ABC has default value X
* **C**ommand**N**ame - Bold letters alias of command

### Data types

* char - Single character
* bool - Boolean expression (true/false)
* uint - Number between 0 and 4294967295
* string - String of characters
