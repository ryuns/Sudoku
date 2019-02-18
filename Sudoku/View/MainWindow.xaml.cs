using Sudoku.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected PuzzleViewModel ViewModel
        {
            get { return (PuzzleViewModel)Resources["PuzzleViewModel"]; }
        }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel.Initialise();
            SizesComboBox.DataContext = ViewModel;
            SubSizesComboBox.DataContext = ViewModel;
            SetDataContexts();
        }

        private void PossibleValuesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.UpdatePossibleValues((bool)PossibleValueHints.IsChecked, ((ListBox)sender).DataContext))
            {
                MessageBox.Show(ViewModel.outputMessage, "Message", MessageBoxButton.OK);
            }
        }

        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.LoadPuzzle();
                SetDataContexts();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetDataContexts()
        {
            PossibleValueHints.IsChecked = false;
            SetPossibles();
            Grid.DataContext = ViewModel.puzzle;
            AlgorithmsList.DataContext = ViewModel.solver;
        }

        private void SetPossibles()
        {
            if ((bool)PossibleValueHints.IsChecked)
            {
                ViewModel.PossibleValuesChecked();
            }
            else
            {
                ViewModel.PossibleValuesUnchecked();
            }
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NewGame(SizesComboBox.SelectedValue, SubSizesComboBox.SelectedValue);
            PossibleValueHints.IsChecked = true;
            SetPossibles();
            Grid.DataContext = ViewModel.puzzle;
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SolvePuzzle();

            MessageBox.Show(ViewModel.outputMessage, "Message", MessageBoxButton.OK);
        }

        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            bool? solverOutput = ViewModel.ProvideHints((int)HintSlider.Value, (bool)PossibleValueHints.IsChecked);

            if (solverOutput == true)
            {
                MessageBox.Show(ViewModel.outputMessage, "Message", MessageBoxButton.OK);
            }
            else if (solverOutput == false)
            {
                MessageBox.Show(ViewModel.outputMessage, "Message", MessageBoxButton.OK);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ResetPuzzle();
                SetDataContexts();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.SaveGame();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.StartGame();
                SetDataContexts();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SizesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SubSizesComboBox.ItemsSource = ViewModel.ValuesForKey(SizesComboBox.SelectedValue);
            SubSizesComboBox.SelectedIndex = 0;
        }

        private void PossibleValueHints_Click(object sender, RoutedEventArgs e)
        {
            if(ViewModel.puzzle != null)
            {
                if((bool)PossibleValueHints.IsChecked)
                {
                    ViewModel.PossibleValuesChecked();
                }
                else
                {
                    ViewModel.PossibleValuesUnchecked();
                }
            }
        }

        private void GeneratePuzzleButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GeneratePuzzle(SizesComboBox.SelectedValue, SubSizesComboBox.SelectedValue);
            SetDataContexts();
        }
    }
}
