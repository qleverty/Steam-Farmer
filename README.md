# SteamFarmer

A simple tool to farm Steam playtime hours. Makes Steam think you're playing a game so you can rack up hours, being AFK. 

Made out of boredom ;)

## How to use

1. Make sure Steam is running and you own the game
2. Run "SteamFarmer.exe" and enter the game's App ID
3. Click "Start"
4. The program will run in background without window
5. Stop it via Task Manager or Steam

You can also use command line:
```
SteamFarmer.exe -id=730          // Start with specific App ID
SteamFarmer.exe -id=730 -auto    // Skip the dialog window
```

The program remembers the last App ID you used.

## Finding Apps' ID

**Easiest way:** Go to [SteamDB](https://steamdb.info/), search for your game, and copy the App ID from the page.

**Alternative:** Open the game's Steam store page. The number in the URL is the App ID.
- Example: `store.steampowered.com/app/730/` → App ID is `730`

---

Made just because my laptop can't run heavy games 24/7
