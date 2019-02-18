using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SudokuLib.Model;
using Sudoku.ViewModel.Enums;

namespace Sudoku.ViewModel
{
    public class PuzzleViewModel : INotifyPropertyChanged
    {
        private const int m_MINIMUMPUZZLESIZE = 4;
        private const int m_MAXIMUMPUZZLESIZE = 25;
        private Puzzle m_StartState;
        private Puzzle m_CurrentPuzzle;
        private Solver m_Solver;
        private CompletionState m_CompletionState = CompletionState.Incomplete;
        private GameState m_GameState = GameState.Stopped;
        private GameType m_GameType = GameType.Manual;
        private Dictionary<int, List<GridSize>> m_PresetSubGridSizes = new Dictionary<int, List<GridSize>>();
        private string m_StatusMessage;

        public void Initialise()
        {
            GenerateSubSizes();
            GeneratePuzzle(9, new GridSize(3, 3));
        }

        private void GenerateSubSizes()
        {
            //4, 6, 8, 9, 10, 12, 14, 15, 16, 18, 20, 21, 22, 24, 25
            //4 = 2x2
            //6 = 2x3
            //8 = 2x4
            //9 = 3x3
            //10 = 2x5
            //12 = 2x6, 3x4
            //14 = 2x7
            //15 = 3x5
            //16 = 2x8, 4x4
            //18 = 2x9, 3x6
            //20 = 2x10, 4x5
            //21 = 3x7
            //22 = 2x11
            //24 = 2x12, 3x8, 4x6
            //25 = 5x5

            for(int i = m_MINIMUMPUZZLESIZE; i <= m_MAXIMUMPUZZLESIZE; i++)
            {
                List<GridSize> subGridSizes = new List<GridSize>();

                for(int j = 2; j <= Math.Sqrt(i); j++)
                {
                    if(i % j == 0)
                    {
                        if(i / j == j)
                        {
                            subGridSizes.Add(new GridSize(j, j));
                        }
                        else
                        {
                            subGridSizes.Add(new GridSize(j, i / j));
                        }
                    }
                }

                if(subGridSizes.Count > 0)
                {
                    m_PresetSubGridSizes.Add(i, subGridSizes);
                }
            }
        }

        public void GeneratePuzzle(object pPuzzleSize, object pSubGridSize)
        {
            Generator generator = new Generator();
            m_StartState = generator.GeneratePuzzle((int)pPuzzleSize, pSubGridSize as GridSize);

            completionState = CompletionState.Incomplete;
            gameState = GameState.Started;
            puzzle = m_StartState.Clone();
            m_Solver = new Solver();
            m_Solver.timer.Restart();

            Thread thread = new Thread(() => Safe(() => CanBeCompleted(), Handle));
            thread.Start();

            Changed("puzzle");
            Changed("starterPuzzle");
            Changed("completePuzzle");
        }

        private void PuzzleGeneration(object pPuzzleSize, object pSubGridSize)
        {
            Generator generator = new Generator();
            m_StartState = generator.GeneratePuzzle((int)pPuzzleSize, pSubGridSize as GridSize);

            completionState = CompletionState.Incomplete;
            gameState = GameState.Started;
            puzzle = m_StartState.Clone();
            m_Solver = new Solver();
            m_Solver.timer.Restart();

            Thread thread = new Thread(() => Safe(() => CanBeCompleted(), Handle));
            thread.Start();

            Changed("puzzle");
            Changed("starterPuzzle");
            Changed("completePuzzle");
        }

        public void NewGame(object pPuzzleSize, object pSubGridSize)
        {
            solvingStatusMessage = "No puzzle started";
            completionState = CompletionState.Incomplete;
            gameState = GameState.Started;
            m_StartState = null;
            puzzle = new Puzzle((int)pPuzzleSize, pSubGridSize as GridSize);
            m_Solver = new Solver();

            Changed("puzzle");
            Changed("starterPuzzle");
            Changed("completePuzzle");
        }

        public void CanBeCompleted()
        {
            solvingStatusMessage = "Solver is attempting to solve the puzzle";
            Puzzle current = m_StartState.Clone();
            string outputMessage;
            Dictionary<string, string> completedAlgorithms;
            gameType = GameType.Loaded;

            if (Solver.SolvePuzzle(ref current, out completedAlgorithms, out outputMessage))
            {
                m_Solver.complete = current;
                m_Solver.completedAlgorithms = completedAlgorithms;
                m_Solver.completeMessage = outputMessage;

                solvingStatusMessage = "Puzzle is solvable";
                Changed("completePuzzle");
            }
            else
            {
                solvingStatusMessage = "Puzzle can not be solved";
                throw new Exception("Puzzle can not be solved");
            }
        }

        private static void Handle(Exception pException)
        {
            MessageBoxResult result = MessageBox.Show(pException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void Safe(Action pTask, Action<Exception> pException)
        {
            try
            {
                pTask.Invoke();
            }
            catch (Exception e)
            {
                pException(e);
            }
        }

        public void LoadPuzzle()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                m_StartState = new Puzzle(dlg.FileName);

                completionState = CompletionState.Incomplete;
                gameState = GameState.Started;
                puzzle = m_StartState.Clone();
                m_Solver = new Solver();
                m_Solver.timer.Restart();

                Thread thread = new Thread(() => Safe(() => CanBeCompleted(), Handle));
                thread.Start();

                Changed("puzzle");
                Changed("starterPuzzle");
                Changed("completePuzzle");
            }
        }

        public void StartGame()
        {
            Puzzle currentPuzzle = m_CurrentPuzzle.Clone();
            currentPuzzle.SetStartPoints();
            m_StartState = currentPuzzle;
            puzzle = m_StartState.Clone();
            m_Solver.timer.Restart();

            Thread thread = new Thread(() => Safe(() => CanBeCompleted(), Handle));
            thread.Start();

            Changed("puzzle");
            Changed("starterPuzzle");
            Changed("completePuzzle");
        }

        public bool UpdatePossibleValues(bool pPossiblesSelected, object pSender)
        {
            if (m_CurrentPuzzle.IsComplete())
            {
                if (m_CompletionState == CompletionState.Incomplete)
                {
                    completionState = CompletionState.UserSolved;
                    m_Solver.timer.Stop();
                    m_Solver.outputMessage = String.Format("Puzzle is complete, it took {0} seconds", decimal.Round((decimal)m_Solver.timer.ElapsedMilliseconds / 1000, 2));
                    return true;
                }
            }

            if (pPossiblesSelected)
            {
                Cell cell = pSender as Cell;
                if (cell != null)
                {
                    cell.UpdateAllPossibles();
                }
            }

            return false;
        }

        public void PossibleValuesChecked()
        {
                for (int i = 0; i < m_CurrentPuzzle.puzzleSize; i++)
                {
                    for (int j = 0; j < m_CurrentPuzzle.puzzleSize; j++)
                    {
                        puzzle.grid[i][j].possibleValues = puzzle.grid[i][j].SetPossibleValues();
                    }
                }
        }

        public void PossibleValuesUnchecked()
        {
                for (int i = 0; i < m_CurrentPuzzle.puzzleSize; i++)
                {
                    for (int j = 0; j < m_CurrentPuzzle.puzzleSize; j++)
                    {
                        puzzle.grid[i][j].possibleValues = Symbol.currentSymbolsIntList;
                    }
                }
        }

        public void SolvePuzzle()
        {
            completionState = CompletionState.AlgorithmSolved;
            puzzle.grid = m_Solver.complete.grid;
            m_Solver.outputMessage = m_Solver.completeMessage;
            m_Solver.algorithmsUsed = m_Solver.completedAlgorithmsUsed;
        }

        public void ResetPuzzle()
        {
            if (m_StartState != null)
            {
                puzzle = m_StartState.Clone();
                m_Solver.algorithmsUsed = new ObservableCollection<string>();
                gameType = GameType.Loaded;
                gameState = GameState.Started;
                completionState = CompletionState.Incomplete;
                m_Solver.timer.Restart();

                Changed("puzzle");
                Changed("starterPuzzle");
                Changed("completePuzzle");
            }
            else
            {
                throw new Exception("Puzzle reset error - There is no puzzle to reset");
            }
        }

        public void SaveGame()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string savePath = dlg.FileName;

                m_CurrentPuzzle.SaveFile(savePath);
            }
        }

        public bool? ProvideHints(int pNumberOfHints, bool pPossiblesSelected)
        {
            completionState = CompletionState.AlgorithmSolved;

            PossibleValuesChecked();

            bool? solverOutput = m_Solver.ProvideHints(ref m_CurrentPuzzle, pNumberOfHints);

            if (!pPossiblesSelected)
            {
                PossibleValuesUnchecked();
            }

            if (solverOutput == null || solverOutput == false)
            {
                completionState = CompletionState.Incomplete;
            }

            return solverOutput;
        }

        public List<GridSize> ValuesForKey(object pKey)
        {
            return m_PresetSubGridSizes[(int)pKey];
        }

        public string outputMessage
        {
            get { return m_Solver.outputMessage; }
        }

        public Puzzle starterPuzzle
        {
            get { if (m_StartState != null) { return m_StartState; } return null; }
        }

        public Puzzle puzzle
        {
            get { if (m_CurrentPuzzle != null) { return m_CurrentPuzzle; } return null; }
            set { m_CurrentPuzzle = value; Changed("puzzle"); }
        }

        public Puzzle completePuzzle
        {
            get { if (m_Solver != null) { if (m_Solver.complete != null) { return m_Solver.complete; } } return null; }
        }

        public Solver solver
        {
            get { return m_Solver; }
        }

        public string solvingStatusMessage
        {
            get { return m_StatusMessage; }
            private set { m_StatusMessage = value; Changed("solvingStatusMessage"); }
        }

        public GameType gameType
        {
            get { return m_GameType; }
            private set { m_GameType = value; Changed("gameType"); }
        }

        public GameState gameState
        {
            get { return m_GameState; }
            private set { m_GameState = value; Changed("gameState"); }
        }

        public CompletionState completionState
        {
            get { return m_CompletionState; }
            private set { m_CompletionState = value; Changed("completionState"); }
        }

        public List<int> dictionaryKeys
        {
            get
            {
                List<int> keys = new List<int>();

                foreach (int key in m_PresetSubGridSizes.Keys)
                {
                    keys.Add(key);
                }

                return keys;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void Changed(string pPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
        }
    }
}
