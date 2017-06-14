using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reversi
{
    /// <summary>
    /// Interaction logic for HighscoresPage.xaml
    /// </summary>
    public partial class HighscoresPage : Page
    {
        public class Highscore
        {
            public string Player1 { get; set; }
            public string Player2 { get; set; }
            public string WinType { get; set; }
            public string Score { get; set; }
        }
        public HighscoresPage()
        {
            InitializeComponent();
            List<Highscore> highscoresList = ReadHighscores();
            Highscores.ItemsSource = highscoresList;
        }

        List<Highscore> ReadHighscores(string filePath= "highscores.txt")
        {
            List<Highscore> highscores = new List<Highscore>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] splitLine = line.Split(',');
                    highscores.Add(new Highscore() { Player1=splitLine[0], WinType= splitLine[1], Player2= splitLine[2], Score=splitLine[3] });
                }
            }
            return highscores;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame mainFrame = Window.GetWindow(this).FindName("MainFrame") as Frame;
            mainFrame.Navigate(new StartPage());
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("highscores.txt", string.Empty);
            List<Highscore> highscoresList = new List<Highscore>();
            Highscores.ItemsSource = highscoresList;
        }
    }
}
