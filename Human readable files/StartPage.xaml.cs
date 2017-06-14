using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
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
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            Frame mainFrame = Window.GetWindow(this).FindName("MainFrame") as Frame;
            mainFrame.Navigate(new GameBoardPage());
        }

        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            Frame mainFrame = Window.GetWindow(this).FindName("MainFrame") as Frame;
            mainFrame.Navigate(new HighscoresPage());
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Frame mainFrame = Window.GetWindow(this).FindName("MainFrame") as Frame;
            mainFrame.Navigate(new HelpPage());
        }
    }
}
