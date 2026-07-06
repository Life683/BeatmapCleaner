# BeatmapCleaner (for osu! Lazer)

### Support the original developer of BeatmapExport

Without them, this repository wouldn't exist!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E5AF13X)

For issues or if an update is required, you can create an issue or discussion on GitHub.

<hr />

# Purpose/Functionality

BeatmapCleaner is a program/tool that can remove unneeded files in beatmaps. This includes, hitsounds, skins, and backgrounds.

osu! Lazer does not have a "Songs/" folder as "stable" osu! does. Lazer's files are stored under hashed filenames and other information about the beatmap is contained in a local "Realm" database on your PC.

# Basic Clean Task Screenshot

Cleaning beatmaps is done through the clean media files button. You can filter which ones will be cleaned.

<img width="1423" height="699" alt="image" src="https://github.com/user-attachments/assets/1f1eee28-ef8c-4bc7-b3cd-8d5420923ef4" />

# Download/Usage

Executables are available from the [Releases section here on GitHub](https://github.com/Life683/BeatmapCleaner/releases), also found on the right of the main page (below About). 

If your Lazer database is in the default location (%appdata%\osu), you should be able to simply run the application. If you changed the database location when installing osu! (Lazer), the program will allow you to locate your database.

The directory needed in the Lazer storage contains another directory named "files". This folder can also be opened from in-game if you moved it and are unsure where it is located. 

## Beatmap Clean

This new storage format which osu! uses results in a better experience while playing the game. However, a result of this system is that you can not easily remove unneeded files from beatmaps.

This utility allows you to remove unneeded beatmaps files, such as backgrounds.

BeatmapCleaner includes a beatmap filter system allowing you to select a portion of your library to only clean certain maps (for example, above a certain star rating, specific artists/mappers, specific gamemodes, specific collections, etc). You can also simply clean your entire library at once.

# Running on macOS/Linux

## $${\color{red}I\ DO\ NOT\ OWN\ A\ MAC\ OR\ LINUX\ DEVICE\ AND\ CANNOT\ GUARANTEE\ THIS\}$$
## $${\color{red}WILL\ WORK\ FOR\ YOU!\}$$

## For Intel-based Macs, if the application is blocked:
> - Download the latest macOS build from the Releases section (`mac-BeatmapExporter.zip`).
> - Click on the downloaded zip to extract `BeatmapExporter` (.app file)
> - Click on `BeatmapExporter`, the program will be blocked, close the security warning
> - Go to System Settings -> Privacy & Security -> scroll to the bottom
> - BeatmapExporter should appear as a blocked program with an "Open Anyway" button available. 
> - Another prompt may come up allowing you to press "Open Anyway" again.

Depending on your system, if the application is still blocked, you could attempt signing the application yourself. There is an [unverified user post (#50)](https://github.com/kabiiQ/BeatmapExporter/discussions/50) detailing this.

If you are not able to do this (for example, you do not have administrator access to the computer), you may be out of luck. Other versions of macOS may have better luck following the Linux method instead. 

Some older versions of macOS may allow the program to run right away but instead restrict its access to your osu! files or to creating exports. In this case, use the Linux method to launch via Terminal instead. 

## For Apple Silicon (M-series chips):

macOS running on the M-series chips may need to run the mac-BeatmapExporter build under Rosetta. See [post #55](https://github.com/kabiiQ/BeatmapExporter/discussions/55) for a guide.

## Linux/macOS Terminal:

Modern Linux distros may allow you to simply click on the file and run it after a warning, otherwise you may need to use your system's Terminal to make the program executable and then run it. 

If you are not familiar with Terminal, you may need to look up how to open Terminal in the specific folder you have downloaded BeatmapExporter into. 

> Run the following command:
`chmod +x linux-BeatmapExporter`, which marks BeatmapExporter.app as executable so that you can run it.
> 
> Then you can run the program with `./linux-BeatmapExporter` from the Terminal window.

## Note on Windows DPI Scaling

It has been observed that the GUI application does not look as intended when using Windows DPI scaling. 
If you have a Windows laptop, especially a high resolution display, it is likely you are using Windows DPI scaling by default.

While I have made some changes to improve this as well as handling low resolution environments, if the scale is high enough it is very likely the program will not look like the screenshot.
If your setup is so extreme that buttons are cut off, you may need to override the scaling settings for this program.
