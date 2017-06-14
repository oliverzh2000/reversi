# reversi

Jan 21 2017.

Hi, this is Reversi made by Oliver Zhang.

My goal for this app was to make something simple and beautiful, 
yet featuring a highly competent AI capable of defeating most human players. 

This app is written in C# for Windows Presentation Foundation, which
runs off Microsoft's .NET framework.

In the "Release" folder, you will find the Reversi.exe file which 
should be able to be run directly on any modern Windows computer.

In the "Human readable files" folder you will find the source code.

The AI and Game logic for this app are contained in Game.cs.

The AI of this app uses the minimax algorithm with alpha beta pruning to predict
the best possible AI move. Early in the game, the AI will strive for mobility
and corners, while in the endgame it will strive solely for score. On "legendary"
mode, the AI searches to a depth of 4 turns, and will generate the full game tree
whenever there is less than 10 moves until the end.
