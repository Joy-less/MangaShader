# Manga Shader

Manga tone shader for Godot 4.

## Screenshots

<img src="https://github.com/Joy-less/MangaShader/blob/main/Example1.png?raw=true" width=1024 />

## Setup

1. Import the shader to your project.
2. Add a `ColorRect` to your scene and set:
- `Anchors Preset` to `Full Rect`
- `Mouse Filter` to `Ignore`
- `Material` to a new shader material with the shader
3. Ensure the `ColorRect`'s `Z Index` is lower than any GUIs you don't want to be affected.

## Special Thanks

- Exuin for making the original shader (CC0): https://godotshaders.com/shader/screentone-black-spaced-pixels