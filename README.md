# Cinema
Cinema is a mod for Muse Dash that adds background video support to custom charts.
## Dependencies
- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6 or higher
- [CustomAlbums Mod](https://github.com/MDMods/CustomAlbums)
## Usage
1. Add a background video in mp4 format to your chart's directory.
2. Create a `cinema.json` file in your chart's directory:
```json
{
    "file_name": "video.mp4",
    "opacity": 0.5
}
```
- `file_name` (required): The name of the video file.
- `opacity` (required): The opacity of the video from 0 to 1.
- `difficulties` (optional): An array of the difficulties to allow the video to be played on. If not specified, the video will be played on all difficulties.
```json
{
    "file_name": "video.mp4",
    "opacity": 0.5,
    "difficulties": [3, 4]
}
```
