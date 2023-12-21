# Brick Game with [C#](https://learn.microsoft.com/pt-br/dotnet/csharp/)

## Overview
An exercise project for GUIs using tools from the [.NET](https://dotnet.microsoft.com/pt-br/download/dotnet/6.0) library [Windows Forms](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/?view=netdesktop-6.0).
The goal is to make a client with some simple games: snake, breakout,
asteroids, and tetris.

The aspect is of a 20x10 grid of fixed `Sprite` objects, which are used
to form `Block` pixels for the construction of each game.

> A `Sprite` is a class extending [`System.Windows.Forms.Panel`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.panel?view=windowsdesktop-6.0), where an outer empty square containing
  a smaller filled square is drawn.
> A `Block` is a logical entity formed with coordinates that 'lights' a 
  `Sprite` to create an illusion of movement.

A manual with the rules and controls of each game can be found on
`...\brickgame_csharp\docs\GameManuals.md`.

## Installation
In the command line, after setting up your directory, download and
play the Brick Game with:

### Windows

```shell
git clone https://github.com/marcantonio64/brickgame_csharp.git
cd brickgame_csharp
dotnet new console --framework net6.0
rm Program.cs
dotnet run
```

## Metadata
**Author:** [marcantonio64](https://github.com/marcantonio64/)

**Contact:** [mafigueiredo08@gmail.com](mailto:mafigueiredo08@gmail.com)

**Date:** 21-Dec-2023

**License:** MIT

**Version:** .NET6.0 / C#10.0