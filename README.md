# CupheadMusicPlayer

A standalone music player for Cuphead that automatically detects the current scene and plays looping music for each boss fight.

## Features

- Auto-detects the Cuphead scene (bosses, run-and-gun levels, map, shop, title, etc.) and plays the matching music.
- Per-scene music configuration in the **Edit Scenes** editor: pick a friendly scene name, choose the music file for it, set a per-scene volume (or **Global**, which follows the main volume slider), and preview it.
- Music files are set per scene in the editor — no global music folder is needed.
- Fades between tracks, with smarter handling when leaving a platformer (run-and-gun) level.
- Adjustable polling rate and volume, and a dark/light theme toggle.
- Settings are persisted and restored on launch.

## Usage

1. Launch the app and press **Start** (it will wait for Cuphead).
2. Open **Edit Scenes...** to assign a music file to each scene you want to hear.
3. Launch Cuphead and the music will follow the scenes.

## To-do
- Add main category presets so you don't have to add manually

Original version for livesplit on https://github.com/zulanthecolossus999-pixel/CupheadMusicPlayer
