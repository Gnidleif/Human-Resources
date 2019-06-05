# Human Resources

General purpose Discord bot written in C#

## Default guild settings

* Prefix - !
* Mark - ⭐
* Marklist - false
* Welcome
  * Enabled - false
  * Time - 10
  * Rank - First
  * Message - Welcome! You'll gain full privileges soon.
* Markov
  * Step - 3
  * Count - 20
  * Source - 500
  * Chance - 1

## Features

### Settings - Get and set guild specific settings

* **h**elp - Returns link to this Github repository
* **s**ettings - Returns the bot settings for the guild
* **s**ettings **p**refi**x** !char - Set command prefix for the guild (**admin**)
* **s**ettings **m**ar**k** !char - Set mark for the guild (**admin**)
* **s**ettings **m**ark**l**ist !bool - Set if marked members are also blacklisted (**admin**)
* **s**ettings **w**elcome - Returns all the welcome settings for the guild
* **s**ettings **w**elcome **e**nable !bool - Enable or disable guild welcome functionality (**admin**)
* **s**ettings **w**elcome **t**ime !ulong - Set the guild welcome time (**admin**)
* **s**ettings **w**elcome **r**ole !string/ulong - Set the first role of new users (**admin**)
* **s**ettings **w**elcome **m**essage !...string - Set the new guild welcome message (**admin**)
* **s**ettings **m**arkov - Get markov settings 
* **s**ettings **m**arkov **s**tep !uint - Set markov step value (**admin**)
* **s**ettings **m**arkov **c**ount !uint - Set markov word count (**admin**)
* **s**ettings **m**arkov **so**urce !uint - Set markov source count (**admin**)
* **s**ettings **m**arkov **ch**ance !uint - Set markov trigger chance (**admin**)

### Administration - Standard administrative guild functions

* **k**ick !string/ulong ...string - Kicks user (**kick**)
* **b**an !string/ulong ...string - Bans user (**ban**)
* **p**urge !uint - Remove specified amount of messages from channel (**admin**)
* **v**oice **k**ick !string/ulong ...string - Voice kick user (**kick**)
* **v**oice **m**ute !string/ulong ...string - Mute user (**mute**)
* **v**oice **m**ute **r**emove !string/ulong - Removes mute from user (**mute**)
* **v**oice **d**eafen !string/ulong ...string - Deafen user (**deafen**)
* **v**oice **d**eafen **r**emove !string/ulong - Removes deafen from user (**deafen**)
* **bl**acklist !string/ulong ...string - Disable bot usage of user (**admin**)
* **bl**acklist **r**emove !string/ulong - Removes user from blacklist (**admin**)
* **t**ime**o**ut !string/ulong uint=10, ...string - Set user on timeout, set time to 0 for random 10-5000 (**admin**)
* **t**ime**o**ut **r**emove !string/ulong - Removes user from timeout (**admin**)
* **m**ar**k** !string/ulong ...string - Set guild mark on user (**admin**)
* **m**ar**k** **r**emove !string/ulong - Remove guild mark from user (**admin**)

### Twitter - Various Twitter related functions

* **tw**itter !string/ulong bool=false - Returns information on Twitter user, set bool to true for verbose

### General

* **s**ponge**b**ob !...string - Spongebobbify text input
* **8b**all !...string - Ask a question, get an answer

### React - Make the bot react to certain phrases

* **r**eact ulong - Get all or specific reactions  (**admin**)
* **r**eact **a**dd !ulong !rgx !...phrase - Add a new reaction (**admin**)
* **r**eact **r**emove !ulong - Remove bot guild reaction (**admin**)
* **r**eact **m**odify **a**dd !ulong !...phrase - Append phrase to existing reaction (**admin**)
* **r**eact **m**odify **r**emove !ulong !uint - Removes phrase at index from reaction (**admin**)
* **r**eact **m**odify **p**hrase !ulong !uint !...phrase - Modify phrase at index from reaction (**admin**)
* **r**eact **m**odify **re**gex !ulong - Modify regex (**admin**)
* **r**eact **m**odify **e**nable !ulong !bool - Enable or disable specific reaction (**admin**)

## Key

### Parameters

* ! - Required
* ... - Can contain spaces
* (**ABC**) - Requires permission ABC
* ABC=X - Parameter ABC has default value X
* **c**omman**d** **n**ame - Bold letters signify alias of command, spaces are included so the example alias is "cd n"

### Data types

* char - Single character
* bool - Boolean expression (true/false)
* uint - Number between 0 and 4294967295
* ulong - Number between 0 and 18446744073709551615
* string - String of characters
* rgx - Regular expression
