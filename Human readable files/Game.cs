using System;
using System.Collections.Generic;
using System.Linq;

namespace Reversi
{
    public class Game
    {
        public const byte Empty = 0, White = 1, Black = 2, Valid = 3, Tie = 4;
        // static string[] tileChars = new string[4] { "  ", "\u25CF ", "\u25CB ", "* " };
        static string[] tileChars = new string[4] { "  ", "1 ", "2 ", "* " };
        static int[][] dirs = { new int[2] {-1, 1},  new int[2] {0, 1},  new int[2] { 1, 1},
                            new int[2] {-1, 0},                      new int[2] { 1, 0},
                            new int[2] {-1, -1}, new int[2] {0, -1}, new int[2] { 1, -1} };
        static Random rng = new Random();

        public static int ToIndex1D(int x, int y)
        {
            if (IsOnBoard(x, y))
            {
                return y * 8 + x;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static int ToIndex1D(Tuple<int, int> index2D)
        {
            int x = index2D.Item1, y = index2D.Item2;
            if (IsOnBoard(x, y))
            {
                return y * 8 + x;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static Tuple<int, int> ToIndex2D(int index)
        {
            if (index < 64)
            {
                return Tuple.Create(index % 8, index / 8);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static byte[,] NewBoard()
        {
            byte[,] board = new byte[8, 8];
            board[3, 3] = board[4, 4] = Black;
            board[3, 4] = board[4, 3] = White;
            return board;
        }

        public static byte OtherTile(byte tile)
        {
            if (tile == Black)
            {
                return White;
            }
            if (tile == White)
            {
                return Black;
            }
            throw new ArgumentException("tile must have value 1 or 2.");
        }

        public static string ValueAsString(byte tile)
        {
            if (tile == Empty)
            {
                return "Empty";
            }
            else if (tile == Black)
            {
                return "Black";
            }
            else if (tile == White)
            {
                return "White";
            }
            else if (tile == Tie)
            {
                return "Tie";
            }
            else return "UNKNOWN TILE NAME";
        }

        public static bool IsOnBoard(int x, int y)
        {
            return 0 <= x && x <= 7 && 0 <= y && y <= 7;
        }

        public static string GenerateString(byte[,] board)
        {
            // Creates a string representation of the board object.
            string final = "";
            for (int y = 0; y < 8; y++)
            {
                string row = "";
                for (int x = 0; x < 8; x++)
                {
                    if (board[x, y] == Empty) row += "  ";
                    else row += board[x, y] + " ";
                }
                final += row + "\n";
            }
            return final;
        }

        public static void RenderBoard(byte[,] board, int indent = 0)
        {
            // Creates a string representation of the board object and outputs it to console.
            Console.Write(new string(' ', indent * 4 + 4));
            for (int colLabel = 0; colLabel < 8; colLabel++)
            {
                Console.Write($" { colLabel}");
            }
            Console.WriteLine();
            Console.WriteLine(new string(' ', indent * 4) + "   " + new string('\u2584', 19));
            for (int rowIndex = 0; rowIndex < 8; rowIndex++)
            {
                Console.Write(new string(' ', indent * 4) + $" {rowIndex} \u2588 ");
                for (int colIndex = 0; colIndex < 8; colIndex++)
                {
                    Console.Write(tileChars[board[colIndex, rowIndex]]);
                }
                Console.Write('\u2588');
                Console.WriteLine();
            }
            Console.WriteLine(new string(' ', indent * 4) + "   " + new string('\u2580', 19));
        }

        public static int GetScore(byte[,] board, byte tile)
        {
            int score = 0;
            foreach (byte cell in board)
            {
                if (cell == tile)
                {
                    score++;
                }
            }
            return score;
        }

        public static byte GetWinner(byte[,] board)
        {
            int blackScore = GetScore(board, Black);
            int whiteScore = GetScore(board, White);
            if (blackScore == 0 || whiteScore == 0 || blackScore + whiteScore == 64 ||
                (GetValidMoves(board, Black).Count + GetValidMoves(board, White).Count == 0))
            {
                if (blackScore > whiteScore) return Black;
                else if (whiteScore > blackScore) return White;
                else return Tie;
            }
            return Empty;
        }

        public static List<Tuple<int, int>> GetFlippedPieces(byte[,] board, Tuple<int, int> move, byte tile)
        {
            int moveX = move.Item1, moveY = move.Item2;
            List<Tuple<int, int>> flippedPieces = new List<Tuple<int, int>>();
            if (board[moveX, moveY] == Empty)
            {
                foreach (int[] dir in dirs)
                {
                    List<Tuple<int, int>> dirFlippedPieces = new List<Tuple<int, int>>();
                    int x = moveX + dir[0];
                    int y = moveY + dir[1];
                    if (IsOnBoard(x, y) && board[x, y] == OtherTile(tile))
                    {
                        dirFlippedPieces.Add(Tuple.Create(x, y));
                        x += dir[0];
                        y += dir[1];
                        while (IsOnBoard(x, y))
                        {
                            if (board[x, y] == tile)
                            {
                                flippedPieces.AddRange(dirFlippedPieces);
                                break;
                            }
                            if (board[x, y] == OtherTile(tile))
                            {
                                dirFlippedPieces.Add(Tuple.Create(x, y));
                            }
                            else if (board[x, y] == Empty)
                            {
                                break;
                            }
                            x += dir[0];
                            y += dir[1];
                        }
                    }
                }
            }
            return flippedPieces;
        }

        public static List<Tuple<int, int>> GetValidMoves(byte[,] board, byte tile)
        {
            List<Tuple<int, int>> validMoves = new List<Tuple<int, int>>();
            for (int X = 0; X < 8; X++)
            {
                for (int Y = 0; Y < 8; Y++)
                {
                    if (board[X, Y] == Empty)
                    {
                        bool doneMove = false;
                        foreach (int[] dir in dirs)
                        {
                            if (doneMove)
                            {
                                break;
                            }
                            int x = X + dir[0];
                            int y = Y + dir[1];
                            if (IsOnBoard(x, y) && board[x, y] == OtherTile(tile))
                            {
                                x += dir[0];
                                y += dir[1];
                                while (IsOnBoard(x, y))
                                {
                                    if (board[x, y] == tile)
                                    {
                                        validMoves.Add(Tuple.Create(X, Y));
                                        doneMove = true;
                                        break;
                                    }
                                    else if (board[x, y] == Empty)
                                    {
                                        break;
                                    }
                                    x += dir[0];
                                    y += dir[1];
                                }
                            }
                        }
                    }
                }
            }
            return validMoves;
        }

        public static void MakeMove(byte[,] board, Tuple<int, int> move, byte tile)
        {
            List<Tuple<int, int>> flippedPieces = GetFlippedPieces(board, move, tile);
            foreach (Tuple<int, int> flippedPiece in flippedPieces)
            {
                board[flippedPiece.Item1, flippedPiece.Item2] = tile;
            }
            if (flippedPieces.Count > 0)
            {
                board[move.Item1, move.Item2] = tile;
            }
        }

        public static int CountCorners(byte[,] board, byte tile)
        {
            int corners = 0;
            if (board[0, 0] == tile) corners++;
            if (board[0, 7] == tile) corners++;
            if (board[7, 0] == tile) corners++;
            if (board[7, 7] == tile) corners++;
            return corners;
        }

        public static int Evaluation(byte[,] board)
        {
            // Heuristical evaluation function that quantifies the attractiveness of a board,
            // relative to the black player.
            int evaluation = 0;
            int blackScore = GetScore(board, Black);
            int whiteScore = GetScore(board, White);
            int blackMobility = GetValidMoves(board, Black).Count;
            int whiteMobility = GetValidMoves(board, White).Count;
            if (blackScore == 0)
            {
                return -200000;
            }
            else if (whiteScore == 0)
            {
                return 200000;
            }
            if (blackScore + whiteScore == 64 || blackMobility + whiteMobility == 0)
            {
                if (Black < whiteScore)
                {
                    return -100000 - whiteScore + blackScore;
                }
                else if (blackScore > whiteScore)
                {
                    return 100000 + blackScore - whiteScore;
                }
                else
                {
                    return 0;
                }
            }
            evaluation += blackScore - whiteScore;
            if (blackScore + whiteScore > 55)
            {
                return (blackScore - whiteScore);
            }
            evaluation += (blackMobility - whiteMobility) * 10;
            evaluation += (CountCorners(board, Black) - CountCorners(board, White)) * 100;
            return evaluation;
        }

        public static int MinimaxAlphaBeta(byte[,] board, int depth, int a, int b, byte tile, bool isMaxPlayer)
        {
            // The heart of our AI. Minimax algorithm with alpha-beta pruning to speed up computation.
            // Higher search depths = greater difficulty.
            if (depth == 0 || GetWinner(board) != Empty)
            {
                return Evaluation(board);
            }
            int bestScore;
            if (isMaxPlayer) bestScore = int.MinValue;
            else bestScore = int.MaxValue;
            List<Tuple<int, int>> validMoves = GetValidMoves(board, tile);
            if (validMoves.Count > 0)
            {
                foreach (Tuple<int, int> move in validMoves)
                {
                    byte[,] childBoard = board.Clone() as byte[,];
                    MakeMove(childBoard, move, tile);
                    int nodeScore = MinimaxAlphaBeta(childBoard, depth - 1, a, b, OtherTile(tile), !isMaxPlayer);
                    if (isMaxPlayer)
                    {
                        bestScore = Math.Max(bestScore, nodeScore);
                        a = Math.Max(bestScore, a);
                    }
                    else
                    {
                        bestScore = Math.Min(bestScore, nodeScore);
                        b = Math.Min(bestScore, b);
                    }
                    if (b <= a)
                    {
                        break;
                    }
                }
            }
            else
            {
                return MinimaxAlphaBeta(board, depth, a, b, OtherTile(tile), !isMaxPlayer);
            }
            return bestScore;
        }

        public static Tuple<int, int> GetAIMove(byte[,] board, int depth, byte tile)
        {
            // The "convienence" function that allows us to use our AI algorithm.
            List<Tuple<int, int>> validMoves = GetValidMoves(board, tile);
            validMoves = validMoves.OrderBy(a => rng.Next(-10, 10)).ToList();
            if (validMoves.Count > 0)
            {
                int bestScore;
                if (tile == Black)
                {
                    bestScore = int.MinValue;
                }
                else if (tile == White)
                {
                    bestScore = int.MaxValue;
                }
                else
                {
                    return null;
                }
                Tuple<int, int> bestMove = validMoves[0];
                if (GetScore(board, Black) + GetScore(board, White) > 55)
                {
                    depth = 100;
                }
                foreach (Tuple<int, int> move in validMoves)
                {
                    byte[,] childBoard = board.Clone() as byte[,];
                    MakeMove(childBoard, move, tile);
                    int nodeScore;
                    if (tile == Black)
                    {
                        nodeScore = MinimaxAlphaBeta(childBoard, depth - 1, int.MinValue, int.MaxValue, OtherTile(tile), false);
                        if (nodeScore > bestScore)
                        {
                            bestScore = nodeScore;
                            bestMove = move;
                        }
                    }
                    else
                    {
                        nodeScore = MinimaxAlphaBeta(childBoard, depth - 1, int.MinValue, int.MaxValue, OtherTile(tile), true);
                        if (nodeScore < bestScore)
                        {
                            bestScore = nodeScore;
                            bestMove = move;
                        }
                    }
                }
                return bestMove;
            }
            return null;
        }

        public static Tuple<int, int> GetMaxScoreMove(byte[,] board, byte tile)
        {
            // A much more naive version of AI compared to GetAIMove or MinimaxAlphaBeta.
            // This function just selects the move with highest score.
            // This is the "easy" AI mode.
            List<Tuple<int, int>> validMoves = GetValidMoves(board, tile);
            if (validMoves.Count > 0)
            {
                int bestScore = int.MinValue;
                Tuple<int, int> bestMove = validMoves[0];
                foreach (Tuple<int, int> move in validMoves)
                {
                    byte[,] childBoard = board.Clone() as byte[,];
                    MakeMove(childBoard, move, tile);
                    int nodeScore = GetScore(board, tile) * rng.Next(-5, 5);
                    if (nodeScore > bestScore)
                    {
                        bestScore = nodeScore;
                        bestMove = move;
                    }
                }
                return bestMove;
            }
            return null;
        }

    }
}