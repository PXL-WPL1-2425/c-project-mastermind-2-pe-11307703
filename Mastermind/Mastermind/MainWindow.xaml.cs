using Microsoft.VisualBasic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private string[] colors = { "Red", "Yellow", "Orange", "White", "Green", "Blue" };
        private string[] secretCode;
        private List<Brush> ellipseColor = new List<Brush> { Brushes.Red, Brushes.Yellow, Brushes.Orange, Brushes.White, Brushes.Green, Brushes.Blue };
        string[,] highScores = new string[15, 3];
        int attempts = 0;
        int countDown = 10;
        int totalScore = 100;

        private bool IsClosed = false;
        

        public MainWindow()
        {
            InitializeComponent();
            StartGame();
            InitializeGame();
            StartCountDown();
            //this.Closing += MainWindow_Closing;
        }

        private void StartCountDown()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void StopCountDown()
        {
            countDown = 10;
            timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            countDown--;
            timerCounter.Content = $"{countDown}";
            if (countDown == 0)
            {
                attempts++;
                timer.Stop();
                if (attempts >= 10)
                {
                    GameOver();
                    return;
                }              
                StopCountDown();
                UpdateTitle();
            }
        }

        private void InitializeGame()
        {
            ResetAllColors();
            totalScore = 100;
            scoreLabel.Content = $"Score: {totalScore}";
            attempts = 0;
            countDown = 10;
            UpdateTitle();
            historyPanel.Children.Clear();
            Random number = new Random();
            secretCode = Enumerable.Range(0, 4)
                             .Select(_ => colors[number.Next(colors.Length)])
                             .ToArray();
            cheatCode.Text = string.Join(" ", secretCode);
        }

        private void ControlButton_Click(object sender, RoutedEventArgs e)
        {

            
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };
            string[] selectedColors = ellipses.Select(e => GetColorName(e.Fill)).ToArray();

            if (selectedColors.Any(color => color == "Transparent"))
            {
                MessageBox.Show("Selecteer vier kleuren!", "Foutief", MessageBoxButton.OK);
                return;
            }

            attempts++;
            UpdateTitle();

            if (attempts >= 10)
            {
                GameOver();
                return;
            }

            CheckGuess(selectedColors);
            AddAttemptToHistory(selectedColors);
            UpdateScoreLabel(selectedColors);
            StopCountDown();
            StartCountDown();
        }

        private void CheckGuess(string[] selectedColors)
        {
            int correctPosition = 0;
            int correctColor = 0;

            List<string> tempSecretCode = new List<string>(secretCode);
            List<string> tempPlayerGuess = new List<string>(selectedColors);

            // Stap 1: Controleer correcte posities (kleur én positie)
            for (int i = 0; i < 4; i++)
            {
                if (tempPlayerGuess[i] == tempSecretCode[i])
                {
                    correctPosition++;
                    tempSecretCode[i] = null;
                    tempPlayerGuess[i] = null;
                }
            }

            // Stap 2: Controleer correcte kleuren (verkeerde positie)
            for (int i = 0; i < 4; i++)
            {
                if (tempPlayerGuess[i] != null)
                {
                    int index = tempSecretCode.IndexOf(tempPlayerGuess[i]);
                    if (index != -1)
                    {
                        correctColor++;
                        tempSecretCode[index] = null;
                    }
                }
            }

            
            
            if (correctPosition == 4)
            {
                if (MessageBox.Show($"Gefeliciteerd! Je hebt de code gekraakt in {attempts} pogingen! Wil je nog eens spelen?", "WINNER", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    
                    MainWindow newWindow = new MainWindow();
                    Application.Current.MainWindow = newWindow;
                    newWindow.Show();
                    this.Close();
                }
                else
                {
                   this.Close();
                }
                return;
            }
        }

        private void AddAttemptToHistory(string[] selectedColors)
        {
            StackPanel attemptPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            for (int i = 0; i < selectedColors.Length; i++)
            {
                Ellipse colorBox = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = GetBrushFromColorName(selectedColors[i]),
                    StrokeThickness = 5,
                    Stroke = GetFeedbackBorder(selectedColors[i], i)
                };
                attemptPanel.Children.Add(colorBox);
            }

            historyPanel.Children.Add(attemptPanel);
        }

        private void UpdateScoreLabel(string[] selectedColors)
        {
            int scorePenalty = 0;

            for (int i = 0; i < selectedColors.Length; i++)
            {
                if (selectedColors[i] == secretCode[i])
                {
                    continue;
                }
                else if (secretCode.Contains(selectedColors[i]))
                {
                    scorePenalty += 1;
                }
                else
                {                
                    scorePenalty += 2;
                }
            }

            totalScore -= scorePenalty;
            if (totalScore < 0) totalScore = 0; 

            scoreLabel.Content = $"Score: {totalScore}";
        }

        private Brush GetFeedbackBorder(string color, int index)
        {
            if (color == secretCode[index])
            {
                return Brushes.DarkRed; 
            }
            else if (secretCode.Contains(color))
            {
                return Brushes.Wheat; 
            }
            else
            {
                return Brushes.Transparent; 
            }
        }

        private void UpdateTitle()
        {
            this.Title = $"Poging {attempts}/10";
        }

        private void ResetGame()
        {
            totalScore = 100;
            attempts = 0;
            historyPanel.Children.Clear();
            ResetAllColors();
            UpdateTitle();
            InitializeGame();
        }

        private void ResetAllColors()
        {
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };

            foreach (Ellipse ellipse in ellipses)
            {
                ellipse.Fill = Brushes.Red;
                ellipse.Stroke = Brushes.Transparent;
            }
        }

        private string GetColorName(Brush brush)
        {
            if (brush == Brushes.Red) return "Red";
            if (brush == Brushes.Yellow) return "Yellow";
            if (brush == Brushes.Orange) return "Orange";
            if (brush == Brushes.White) return "White";
            if (brush == Brushes.Green) return "Green";
            if (brush == Brushes.Blue) return "Blue";
            return "Transparent";
        }

        private void Toggledebug()
        {
            if (cheatCode.Visibility == Visibility.Hidden)
            {
                cheatCode.Visibility = Visibility.Visible;
            }
            else if (cheatCode.Visibility == Visibility.Visible)
            {
                cheatCode.Visibility = Visibility.Hidden;
            }
        }

        private void cheatCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
            {
                Toggledebug();
            }
        }

        private void kleur_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse)
            {
                ChangeEllipseColor(ellipse);
            }
        }

        private void ChangeEllipseColor(Ellipse ellipse)
        {
            SolidColorBrush currentBrush = ellipse.Fill as SolidColorBrush;
            int currentIndex = ellipseColor.IndexOf(currentBrush);

            int nextIndex = (currentIndex + 1) % ellipseColor.Count;
            ellipse.Fill = ellipseColor[nextIndex];
        }

        private void GameOver()
        {
            timer.Stop();
            if (MessageBox.Show($"Je hebt de code niet gekraakt. De correcte code was: {string.Join(", ", secretCode)}. Nog eens proberen?", "FAILED", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                InitializeGame();
            }
            else
            {
                this.Close();
            }
        }

        private Brush GetBrushFromColorName(string colorName)
        {
            return colorName switch
            {
                "Red" => Brushes.Red,
                "Yellow" => Brushes.Yellow,
                "Orange" => Brushes.Orange,
                "White" => Brushes.White,
                "Green" => Brushes.Green,
                "Blue" => Brushes.Blue,
                _ => Brushes.Transparent
            };
        }
        //private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    MessageBoxResult result = MessageBox.Show("Wilt u het spel vroegtijdig beëindigen?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        //    if (result == MessageBoxResult.No)
        //    {
        //        e.Cancel = true;
        //    }
        //}

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {

            //if (MessageBox.Show("Applicatie afsluiten?", "Afsluiten", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
                this.Close();
            //}


        }


        private void StartGame()
        {
            try
            {
               
                string name = Interaction.InputBox("Naam speler: ");
                
                {
                    if (name.Contains(null))
                    {
                        MessageBox.Show("Geef aub uw naam in", "Foutief", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geef een naam in aub");
            }
        }

        private void NewGameMenu_Click(object sender, RoutedEventArgs e)
        {
           
            StartGame();
        }

        private void HighscoreMenu_Click(object sender, RoutedEventArgs e)
        {
            foreach (string input in highScores)
            {
                MessageBox.Show($"{input}");
            }
        }
    }
}
