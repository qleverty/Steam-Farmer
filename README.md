# Steam Farmer

Simple tool to farm Steam hours by simulating game activity.

## How to use

1. Run the exe
2. Enter game's App ID (you can find it on SteamDB or in the game's store URL)
3. Program runs in background and keeps the game "running" in Steam

Steam must be running before starting the program.

## Command line args

- `-id=123456` - Start with specific App ID
- `-auto` - Skip GUI and use saved/provided App ID

Example: `SteamFarmer.exe -id=730 -auto`
