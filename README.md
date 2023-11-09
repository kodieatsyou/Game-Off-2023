# Game-Off-2023
Repository for submission to Github Game Off 2023. Theme - SCALE

## Links
Competition: https://itch.io/jam/game-off-2023
Github: https://github.com/kodieatsyou/Game-Off-2023
Inspiration: https://www.kickstarter.com/projects/magnetcubes/magnetcubes-cubeclimbers-board-game-motorized-liftpack

## Summary
Art Style: Voxel

## Name Ideas:
* CardClimbers.io
* SummitStack.io
* SlopeSprint.io
* AscentArena.io
* AltitudeArena.io
* MountainMeld.io

## Stack: 
* Engine: Unity
* Language: C#
* Networking: https://www.photonengine.com/PUN

## Type: Turn based .io party

## Gameplay Loop:

Players start by being assigned a turn order (maybe roll a die for turn order). On your turn you move, build 2 blocks, play a power card, and roll the action die. When moving you can move one block adjacent or up from where you are. You can build 2 blocks anywhere in the grid where the block will not be floating, inside a previous block, or outside the grid. You can choose to play a power card. The power cards could either be randomly given out at the start or players could build a deck. The action die will cause some effect to happen such as grapple (the player can move up to 2 blocks up), wind (the player chooses a direction and the wind causes all players, including the one who rolled the die, to move one block in the direction of the wind). The game ends when all blocks have been placed and the person who climbed the highest wins.

## Gameplay:
Up to six players
Mario party style order generation

#### Turns
* Place 2 blocks
* Move to any block on same level
* Roll die for grapple, wind, or power card usage?
* Grapple (Grapple up to the next level)
* Wind (Move all players one block in the cardinal direction of choice)
* Power cards below
* Timed turn (10-15 seconds?)
* Block building
* Build anywhere on the map
* No building inside another player or floating blocks

#### Power cards 
Only use each card once a game, you get a full deck to start

* Block Bomb (delete block(s?))
* Timestop (prevent another player from moving)
* Barrier (Make a block/multiple? untouchable)
* Switch (switch place with another player)
* Kick (kick an adjacent player to an adjacent block)
* Levitate (place block directly underneath yourself)
* Steal (remove a block from under a player and add it to your block pouch)
* Taunt (players adjacent to you cannot move or grapple)
* Ninja (perform a free grapple action that goes 2 blocks)


# TODO
Game Functions
Game Board
Auto-generated 10x10 grid with random already placed blocks to key players into the idea of the game.

Networking
Single host (invite link to invite friends)
Host with free web hosting for UI

Art
UI/UX
Art/Animations/Emotes
UI, Start/End, Menu
Climber Player models
Global scoreboard / Rank
Sounds

If Time Allows
Skybox changes with height
Different modes

