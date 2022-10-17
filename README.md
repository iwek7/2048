Simple 2048 clone written to test how Godot 4 works with C#.

This is how it looks:

![Gameplay](screenshot.png?raw=true)

Some implementation details, thoughts and lessons learnt:
- I tested architecture in which there is separation between logic and rendering. In `logic` package you will find pure C# implementation of 2048. After feeding user input to logic it returns list of changes that are applied by rendering part of code.
- Decoupling logic from rendering allowed me to write nice unit tests.
- Decoupling caused some overhead, because I needed to devise some kind of api between logic and rendering part of code, this led to some duplication.
- 2048 implementation uses method in which whole board is transposed so that every move is considered to be to the left. After applying move to the left reverse transposition is applied. This way I avoided writing a lot of ifs in already complicated block movement code. On the other hand I had to write transposition logic, but it was pretty easy.
- All animations were implemented using tweens and it works pretty good but code looks pretty bad (as does most of rendering code).
- I have no idea how to structure C# project so it is pretty messy.
- I used controls to structure grid in rendering code, however, they are meant for more static GUI elements. As a result I struggled quite a bit with GUI framework. I believe rendering implementation would be MUCH simpler if I used Node2D based structure. 
- Finding good color schemes is hard, as always.

All in all it was more fun to implement it that I expected, Godot4 beta2 works pretty great.




















