# Human Resources

General purpose Discord bot written in C#

## Default settings

* Prefix - !
* Mark - ⭐
* Marklist - false
* Welcome
  * Enabled - false
  * Time - 10
  * Rank - @everyone
  * Message - Welcome! You'll gain full privileges soon.

## Features

### Settings - Get and set server specific settings

* settings - Returns the bot settings for the guild
* prefix <char> - Set command prefix for the guild (**admin**)
* mark <char> - Set mark for the guild (**admin**)
* marklist <bool> - Set if marked members are also blacklisted (**admin**)
* settings welcome - Returns all the welcome settings for the guild
* settings welcome enable <bool> - Enable or disable guild welcome functionality (**admin**)
* settings welcome time <uint> - Set the guild welcome time (**admin**)
* settings welcome role <string/uint> - Set the first role of new users (**admin**)
* settings welcome message <...string> - Set the new guild welcome message (**admin**)

### Administration - Standard administrative guild functions

* kick <string/uint> ...reason - Kicks user (**kick**)
* ban <string/uint> ...reason - Bans user (**ban**)
* voice kick <string/uint> ...reason - Voice kick user (**kick**)
* voice mute <string/uint> ...reason - Mute user (**mute**)
* voice mute remove <string/uint> - Removes mute from user (**mute**)
* voice deafen <string/uint> ...reason - Deafen user (**deafen**)
* voice deafen remove <string/uint> - Removes deafen from user (**deafen**)
* blacklist <string/uint> ...reason - Blacklist user (**admin**)
* blacklist remove <string/uint> - Removes user from blacklist (**admin**)
* timeout <string/uint> uint=10 - Set user on timeout (**admin**)
* timeout remove <string/uint> - Removes user from timeout (**admin**)
* mark <string/uint> ...reason - Set guild mark on user (**admin**)
* mark remove <string/uint> - Remove guild mark from user (**admin**)

### Twitter - Various Twitter related functions

* twitter <string/uint> - Returns information on Twitter user

## Key

### Parameters

* <> - Required
* ... - Can contain spaces
* (**ABC**) - Requires permission ABC
* ABC=X - Parameter ABC has default value X

### Data types

* char - Single character
* bool - Boolean expression (true/false)
* uint - Number between 0 and 4294967295
* string - String of characters
