# One More Spin

A three-reel slot machine built in Unity for the slot game assignment. Choose a bet, pull the lever, and win by matching all three symbols on the middle row.

**Play online:** https://devgogia029.github.io/one-more-spin/Build/WebGL/

The build is about 15 MB, so the first load can be slow on a weak connection. Sound starts after your first click, because browsers block audio until then.

## Running the WebGL Build Locally

The build won't run by double-clicking `index.html`, so it needs a small local server:

1. Open a terminal in `Build/WebGL`
2. Run `python -m http.server 8000`
3. Open http://localhost:8000

To open the project in the editor, use Unity 6000.4.1f1 and load `Assets/Scenes/MainScene`.

## How to Play

- Use the arrows to change your bet (10 to 100)
- Click the lever to spin
- Click **AUTO** to play 10 spins automatically, and click it again to stop
- You start with 1000 coins. If you run out, a popup lets you start again

## Payouts

| Symbol | Payout (x bet) |
|---|---|
| 7 | 30 |
| Cherries | 15 |
| Bell | 10 |
| BAR | 5 |

All four symbols are equally likely, so a win comes up about once every 16 spins, and the payouts return roughly 94% over time.

## Bonus Features

- **Auto Spin:** 10 automatic spins that can be stopped at any time
- An animated lever in place of a Spin button
- Sound effects and background music
- The bet lowers by itself if you can no longer afford it

## My Approach

- The result is decided first, then the reels animate to land on it, so what you see always matches the payout.
- Each reel is a long strip of symbols that slides and slows down. It starts from the symbols already on screen, so there is no jump between spins.
- Auto Spin reuses the normal spin, so manual and automatic spins behave the same way.
- The code is split into three scripts: `SlotReel` (one reel's animation), `SlotMachineManager` (game flow, betting, payouts and auto spin) and `AudioManager` (sound).

## Project Structure

- `Assets/Scripts`: the three scripts
- `Assets/Scenes`: `MainScene`
- `Assets/Prefabs`: the game over popup
- `Assets/Animations`: the lever pull animation
- `Assets/UI`: artwork supplied for the assignment
- `Assets/Sounds`: sound effects and music
- `Build/WebGL`: the WebGL build

## If I Had More Time

- A win celebration animation
- Saving the balance between sessions

## Credits

- Art: supplied by the hiring team
- Sound effects: synthesised by Claude (AI assistant) using Python
- Background music: Casino by Echobrainz, from pixabay, licence: Pixabay Content Licence
