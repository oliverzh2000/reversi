using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.IO;

namespace Reversi
{
    public class Player
    {
        public enum Modes { Human, AIEasy, AINormal, AIHard, AILegendary };
        
        public byte Tile;
        public Modes Mode;

        public static Player GetPlayerFromTurn(int turn, Player player1, Player player2)
        {
            if (turn % 2 == 0)
            {
                return player1;
            }
            else
            {
                return player2;
            }
        }
    }

    /// <summary>
    /// Interaction logic for GameBoardPage.xaml
    /// </summary>
    public partial class GameBoardPage : Page
    {
        byte[,] board = Game.NewBoard();
        List<byte[,]> boardHistories = new List<byte[,]>();
        int turn = 0;

        bool animationPaused = false;
        bool animationPending = false;
        DispatcherTimer animationTimer = new DispatcherTimer();

        Player player1 = new Player() { Tile = Game.White, Mode = Player.Modes.Human };
        Player player2 = new Player() { Tile = Game.Black, Mode = Player.Modes.Human };


        public GameBoardPage()
        {
            InitializeComponent();
            GenerateBoardButtons();
            animationTimer.Interval = TimeSpan.FromMilliseconds(200);
            animationTimer.Tick += ResumeAnimations;
        }


        void GenerateBoardButtons()
        {
            for (int i = 0; i < 64; i++)
            {
                Button cellButton = new Button();
                cellButton.Template = Resources["CellTemplate"] as ControlTemplate;
                cellButton.Click += cellButton_Click;
                BoardUniformGrid.Children.Add(cellButton);
                if (i == 63)
                {
                    cellButton.Loaded += FinalCellButton_Loaded;
                }
            }
        }
        
        void FinalCellButton_Loaded(object sender, RoutedEventArgs e)
        {
            boardHistories.Add(board.Clone() as byte[,]);
            HandleAIMove();
            RenderBoard(board);
        }
        
        private void cellButton_Click(object sender, RoutedEventArgs e)
        {
            Player currentPlayer = Player.GetPlayerFromTurn(turn, player1, player2);
            if (currentPlayer.Mode == Player.Modes.Human)
            {
                int senderIndex = BoardUniformGrid.Children.IndexOf(sender as UIElement);
                Tuple<int, int> move = Game.ToIndex2D(senderIndex);
                HandleNextMove(move);
            }
        }

        void HandleAIMove()
        {
            Player player = Player.GetPlayerFromTurn(turn, player1, player2);
            if (player.Mode != Player.Modes.Human)
            {
                List<Tuple<int, int>> validMoves = Game.GetValidMoves(board, player.Tile);
                Tuple<int, int> move = null;
                if (validMoves.Count > 0)
                {
                    if (player.Mode == Player.Modes.AILegendary)
                    {
                        move = Game.GetAIMove(board, 4, player.Tile);
                    }
                    else if (player.Mode == Player.Modes.AIHard)
                    {
                        move = Game.GetAIMove(board, 3, player.Tile);
                    }
                    else if (player.Mode == Player.Modes.AINormal)
                    {
                        move = Game.GetAIMove(board, 2, player.Tile);
                    }
                    else if (player.Mode == Player.Modes.AIEasy)
                    {
                        move = Game.GetMaxScoreMove(board, player.Tile);
                    }
                }
                HandleNextMove(move);
            }
        }

        void HandleNextMove(Tuple<int, int> move)
        {
            if (Game.GetWinner(board) == Game.Empty)
            {
                Player currentPlayer = Player.GetPlayerFromTurn(turn, player1, player2);
                List<Tuple<int, int>> validMoves = Game.GetValidMoves(board, currentPlayer.Tile);
                if (validMoves.Count > 0 && move != null)
                {
                    if (validMoves.Contains(move))
                    {
                        Game.MakeMove(board, move, currentPlayer.Tile);
                        boardHistories.Add(board.Clone() as byte[,]);
                        turn++;
                        RenderBoard(board);
                    }
                    else return;
                }
                else turn++;

                if (Game.GetWinner(board) != Game.Empty)
                {
                    HandleWinner();
                }
                Player nextPlayer = Player.GetPlayerFromTurn(turn, player1, player2);
                if (nextPlayer.Mode != Player.Modes.Human)
                {
                    HandleAIMove();
                }
            }
            
        }

        void HandleWinner()
        {
            byte winnerTile = Game.GetWinner(board);
            Storyboard winnerBannerIn = this.FindResource("WinnerBannerIn") as Storyboard;
            Storyboard gameOptionsOut = this.FindResource("GameOptionsOut") as Storyboard;
            winnerBannerIn.Begin();
            gameOptionsOut.Begin();
            string player1Text;
            string player2Text;
            string winType = "beat";
            string scoreText;
            if (winnerTile == Game.White || winnerTile == Game.Tie)
            {
                player1Text = Player1Name.Text;
                player2Text = Player2Name.Text;
                scoreText = Game.GetScore(board, Game.White).ToString() + "-" + Game.GetScore(board, Game.Black).ToString();
            }
            else // winner == Game.Black
            {
                player1Text = Player2Name.Text;
                player2Text = Player1Name.Text;
                scoreText = Game.GetScore(board, Game.Black).ToString() + "-" + Game.GetScore(board, Game.White).ToString();
            }
            WinnerBannerScore.Text = scoreText;
            if (winnerTile == Game.Tie)
            {
                WinnerBannerHeading.Text = "It's a tie!";
                winType = "tied with";
            }
            WinnerBannerPlayer1.Text = player1Text;
            WinnerBannerPlayer2.Text = player2Text;
            WinnerBannerWinType.Text = winType;
            WinnerBannerScore.Text = scoreText;
            // Write highscores.
            using (StreamWriter writer = new StreamWriter("highscores.txt", append: true))
            {
                string[] gameInfo = { player1Text, winType, player2Text, scoreText };
                writer.WriteLine(string.Join(",", gameInfo));
            }
        }

        void ResumeAnimations(object sender, EventArgs e)
        {
            if (animationPaused)
            {
                animationTimer.Stop();
                animationPaused = false;
                if (animationPending)
                {
                    RenderBoard(board);
                    animationPending = false;
                }
            }
        }

        void RenderBoard(byte[,] board)
        {
            if (animationPaused)
            {
                animationPending = true;
                return;
            }
            bool changesOccurred = false;
            for (int index = 0; index < 64; index++)
            {
                Tuple<int, int> index2D = Game.ToIndex2D(index);
                int x = index2D.Item1;
                int y = index2D.Item2;
                byte value = board[x, y];

                if (value == Game.White || value == Game.Black || value == Game.Empty)
                {
                    Button cellButton = BoardUniformGrid.Children[index] as Button;
                    cellButton.Tag = Game.ValueAsString(value);
                    changesOccurred = true;
                }
            }
            if (changesOccurred)
            {
                animationPaused = true;
                animationTimer.Start();
            }
            Player currentPlayer = Player.GetPlayerFromTurn(turn, player1, player2);
            if (currentPlayer == player1)
            {
                Player1Display.IsEnabled = true;
                Player2Display.IsEnabled = false;
            }
            else
            {
                Player1Display.IsEnabled = false;
                Player2Display.IsEnabled = true;
            }
            Player1Score.Text = Game.GetScore(board, Game.White).ToString();
            Player2Score.Text = Game.GetScore(board, Game.Black).ToString();

            //Trace.WriteLine(Game.GenerateString(board));
        }


        private void ToggleGameOptions()
        {
            if (GameOptions.Visibility == Visibility.Visible)
            {
                Storyboard gameOptionsOut = this.FindResource("GameOptionsOut") as Storyboard;
                gameOptionsOut.Begin();
            }
            else
            {
                Storyboard gameOptionsIn = this.FindResource("GameOptionsIn") as Storyboard;
                gameOptionsIn.Begin();
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleGameOptions();
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus(); // Clears focus from the name entry textboxes.
        }

        private void Player_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try // try catch clause because this raises nullreference error on first call.
            {
                if (Player1Mode.SelectedIndex == 0)
                {
                    player1.Mode = Player.Modes.Human;
                }
                if (Player2Mode.SelectedIndex == 0)
                {
                    player2.Mode = Player.Modes.Human;
                }
                if (Player1Mode.SelectedIndex == 1)
                {
                    if (player2.Mode == Player.Modes.Human)
                    {
                        // Only one player is allowed to be AI.
                        if (Player2Mode.SelectedIndex == 1)
                        {
                            Player2Mode.SelectedIndex = 0;
                            player2.Mode = Player.Modes.Human;
                        }
                        switch (Player1Difficulty.SelectedIndex)
                        {
                            case 0:
                                player1.Mode = Player.Modes.AIEasy;
                                break;
                            case 1:
                                player1.Mode = Player.Modes.AINormal;
                                break;
                            case 2:
                                player1.Mode = Player.Modes.AIHard;
                                break;
                            case 3:
                                player1.Mode = Player.Modes.AILegendary;
                                break;
                        }

                        HandleAIMove();
                    }
                }
                if (Player2Mode.SelectedIndex == 1)
                {
                    // Only one player is allowed to be AI.
                    if (Player1Mode.SelectedIndex == 1)
                    {
                        Player1Mode.SelectedIndex = 0;
                        player1.Mode = Player.Modes.Human;
                    }
                    switch (Player2Difficulty.SelectedIndex)
                    {
                        case 0:
                            player2.Mode = Player.Modes.AIEasy;
                            break;
                        case 1:
                            player2.Mode = Player.Modes.AINormal;
                            break;
                        case 2:
                            player2.Mode = Player.Modes.AIHard;
                            break;
                        case 3:
                            player2.Mode = Player.Modes.AILegendary;
                            break;
                    }
                    HandleAIMove();
                }
            }
            catch { }
        }
       
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            int lastIndex;
            if (boardHistories.Count > 2)
            {
                if (player1.Mode != Player.Modes.Human || player2.Mode != Player.Modes.Human)
                {
                    // if one of the players are human, we must undo 2 moves.
                    lastIndex = boardHistories.Count - 1;
                    board = boardHistories[lastIndex - 1].Clone() as byte[,];
                    boardHistories.RemoveAt(lastIndex);
                    turn--;
                    RenderBoard(board);
                }
                lastIndex = boardHistories.Count - 1;
                board = boardHistories[lastIndex - 1].Clone() as byte[,];
                boardHistories.RemoveAt(lastIndex);
                turn--;
                HandleAIMove();
                RenderBoard(board);
            }
            else if (boardHistories.Count > 1)
            {
                lastIndex = boardHistories.Count - 1;
                board = boardHistories[lastIndex - 1].Clone() as byte[,];
                boardHistories.RemoveAt(lastIndex);
                RenderBoard(board);
                turn--;
                HandleAIMove();
            }
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            Storyboard winnerBannerOut = this.FindResource("WinnerBannerOut") as Storyboard;
            winnerBannerOut.Begin();
            boardHistories.Clear();
            board = Game.NewBoard();
            turn = 0;
            animationPaused = false;
            animationPending = false;
            RenderBoard(board);
            HandleAIMove();
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            Frame mainFrame = Window.GetWindow(this).FindName("MainFrame") as Frame;
            mainFrame.Navigate(new StartPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame mainFrame = Window.GetWindow(this).FindName("MainFrame") as Frame;
            mainFrame.Navigate(new StartPage());
        }
    }
}
