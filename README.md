# Altitude Arena

[AltitudeArena.io](https://altitudearena.io)

Repository for submission to Github Game Off 2023. Theme - SCALE
Heavily inspired by the Kickstarter Project [CubeClimbers](https://www.kickstarter.com/projects/magnetcubes/magnetcubes-cubeclimbers-board-game-motorized-liftpack).

Stack:
* Engine: Unity
* Programming Language: C#
* Multiplayer Networking: [PUN](https://www.photonengine.com/PUN)

Important Links
* [Competition Hub](https://itch.io/jam/game-off-2023)

## Summary
Altitude Arena is a voxel art stylized, turn-based strategy combat arena, party game with a focus on building and climbing up a mountain. 

The __goal__ of the game is to reach the top of the board, determined by a specific level. Once a single player reaches the top level they are determined the winner with other places being determined by their level and other factors such as impact on the game.

### Lobbies
Lobbies are created and hosted by a single player in which they can either provide a link to a private lobby or open their lobby to public players. Players looking for a game can find one in the server browser.
Up to 6 players can play in a lobby at a time.
Hosts can customize different aspects of the game.

## Gameplay
Everything the player does revolves around their turn.
During their turn, players will have the opportunity to move to any block on their current level, place 2 blocks anywhere on the board, and roll the [Action Die](). Each player will only have __15 seconds__ to complete their turn.

### Player Order
The order in which players take their turn is determined by the roll of a dice, where the higher the roll the earlier you will go in the order. Ties are determined by RNG.

### Levels
Levels on the board are defined by the y-axis. Each level represents an increase in the height of the placed blocks. Each block stacked on another increases the level by 1.

#### Moving
During their turn, players will have the opportunity to move to any block on the board on their current level which does not contain another player.

#### Placing Blocks and The Block Pouch
Players may place blocks anywhere on the board following a few rules:
  1. Blocks must be placed on top of a currently existing block and may not have any air underneath.
  2. Blocks may not be placed on top themselves or other players.

Each player has a Block Pounch with two block slots unless modifed by a power card. These slots are filled before each turn and consumed during the turn.

### Action Die
A 6-sided dice that provides the player with a chances to perform an action:
* Use Grappling Hook _(3/6 chance)_ - Move up by up-to 2 levels on the board
* Create Wind Gust _(1/6 chance)_ - Create a wind gust from the North, South, East, Or West of the board which causes all players to move 1 space in that direction.
* Use A [Power Card]() _(2/6 chance)_

### Power Card
Power Cards are stored by the player in their deck. When used, power cards are then removed from the deck (burned) and are unusable for the rest of the game. Power cards which can completly change the layout of the board, affect other players, or so much more.

#### Types
* Block Bomb - Removes Blocks from the board.
* Timestop - Skip another player's next turn.
* Barrier - Make a block untouchable.
* Switch - Switch places with another player.
* Kick - Kick an adjacent player in the direction you are facing 
* Levitate - Place block directly underneath yourself
* Steal - Remove a block from under a player and add it to your block pouch for next turn.
* Ninja - Perform a free grapple action.

## Contributing
Create a branch linking to a listed issue. This will be your feature branch.
After finishing the issue task, create a pull request into the main branch to be approved by [@kodieatsyou](https://github.com/kodieatsyou), [@TorinM](https://github.com/TorinM), and [@maypd13](https://github.com/maypd13)
