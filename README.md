# DayZServerManager

## Introduction
This started as a small program and spiraled out of control a little bit.
It currently features a basic UI based on REACT and a backend that is based on .NET 8.

## Starting
Starting is as simple as running the executable file.
After unpacking all the files into its own directory, you can immediately start it up. It will create and download all the necessary files and folders by itself.

After that you can open the UI at localhost:5172 or if you opened the port in your firewall by another computer in the same network.

First make sure that you go into the Manager Config Editor and the Server Config Editor and change all the parameters and add mods. Make sure that you put in your Steam username and password, because it can't download the server or the mods without a download.

Then go to Home and start the server. Wait in this UI for a bit for the Steam Guard prompt to appear.

After that it will take a bit of time depending on the amount and size of the mods until the server is started.

Also make sure to open the game port and the Steam Query port in your firewall and router. DON'T open your RCON port in your router, only if you know what you are doing.

Now you can join the server and enjoy your very own DayZ server.

## Template
Make sure to check out the template.chernaurs in the mpmissions folder which is a template for creating your very own mission.

### Custom Files
Put in all your mod types, spawnableTypes, events and so on in the CustomFiles folder and add an entry in the cfgeconomycore.xml.

By default the server will create a template.chernarus folder with some example files inside of it.

### Rarity Files
It will also feature three rarity Files, called customFilesRarities, expansionRarities and vanillaRarities.

The customFilesRarities and the expansionRarities are only important, if you have expansion. If not ignore it.

The vanillaRarities will change the types file and therefore the amount of loot for vanilla based upon the rarity given.

The rarities for the numbers are like this:

```
EMPTY = 0
Poor = 1
Common = 2
Uncommon = 3
Rare = 4
Epic = 5
Legendary = 6
Mythic = 7
Exotic = 8
Quest = 9
Collectable = 10
Ingredient = 11
```

The expansionRarities will do the same thing but for the expansion types file.

The customFilesRarities doesn't do anything to any types file.

If you have expansion, all three files will write the raritiy into the HardlineSettings.json of the expansion which you will be able to see in the game.

### Types Changes
The TypesChanges files are to change different lifetimes or rarities manually.

Like if you want to change the lifetime of the cars in vanilla to 3888000, so the bought cars don't despawn.

### Init.c
You can also have a template for the init.c.

For that, you need to make sure that you put all the things that you want to be added to the vanilla init.c inside a "void main()" in the one in the template folder.

It should look something like this:
```
void main()
{
  // Uncomment this, if you added custom buildings to your server and want them to spawn loot
	// After uncommenting, move it to after the economy initialization, let the server run and move all the files from the "storage_1\export" folder into your root mission folder and replace the existing files
	// Restart the server for your changes to take effect
	// GetCEApi().ExportProxyData("10000 0 10000", 20000);	// standard map groups (buildings) export, terrain center and radius needs to be specified
	// GetCEApi().ExportClusterData();						// cluster-type map groups export (fruit trees etc.)
}
```

### Other Files
By default it will copy over all the files and folders inside of your template except for the cfgeconomycore.xml, the rarity files, the types changes files and the init.c.

### Different Maps
If you are using a different map, you need to adjust the parameters inside of the Manager Config and either rename the first created template or let it create a new template folder.

## Features
- It automatically downloads and creates a scheduler for restarting the server regular intervals.
- It automatically updates your mission based upon your template, if your server or any expansion mod got an update.
- Gives you a UI that you can access from outside of your own PC, so you don't need any SSH or Remote Desktop connection to start, stop or manage some of the configs.

## Disclaimer
This is open-source for a reason. I do this as a passion project and my own server, because I couldn't find any alternative on the internet that could also automatically update the mission. Because of this feature alone, it can lower the amount of work a server admin has to do by a substantial margin.

If you have any suggestions or questions, feel free to ask me on my Discord or wherever else you can find me. Other than that, I hope that you are enjoying my program.
