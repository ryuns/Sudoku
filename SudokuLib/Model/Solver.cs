using SudokuLib.Model.Algorithms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model
{
    public class Solver : INotifyPropertyChanged
    {
        private static Type[] m_Algorithms = { typeof(NakedSingle), typeof(HiddenSingle), typeof(NakedCandidates), typeof(HiddenCandidates), typeof(Pointers), typeof(BoxLineReduction) };
        private Puzzle m_SolvedPuzzle = null;
        private ObservableCollection<string> m_AlgorithmsUsed = new ObservableCollection<string>();
        private Dictionary<string, string> m_CompletedPuzzleAlgorithms = new Dictionary<string, string>();
        private Stopwatch m_Timer = new Stopwatch();
        private string m_SolvedMessage;
        private string m_OutputMessage;

        #region Hints method
        /// <summary>
        /// Provides hints for the puzzle by filling in empy puzzle squares.
        /// </summary>
        /// <param name="pCurrentPuzzle">The current puzzle</param>
        /// <param name="pNumberOfHints">The number of hints required</param>
        /// <returns>A bool for whether the puzzle is complete or not, null means puzzle is incomplete but hints could be provided</returns>
        public bool? ProvideHints(ref Puzzle pCurrentPuzzle, int pNumberOfHints)
        {
            int numberOfHintsGiven = 0;

            List<Cell> possibleCells = new List<Cell>();

            //Gets a list of cells that are empty
            for (int i = 0; i < pCurrentPuzzle.puzzleSize; i++)
            {
                for (int j = 0; j < pCurrentPuzzle.puzzleSize; j++)
                {
                    if (!pCurrentPuzzle.grid[i][j].startPoint && !pCurrentPuzzle.grid[i][j].algorithmEntered && pCurrentPuzzle.grid[i][j].content == 0)
                    {
                        possibleCells.Add(pCurrentPuzzle.grid[i][j]);
                    }
                }
            }

            while (!pCurrentPuzzle.IsComplete() && numberOfHintsGiven < pNumberOfHints && possibleCells.Count > 0)
            {
                //Takes a random number for selection of a random cell
                int random = RNG.RANDOM.Next(possibleCells.Count);
                Cell currentCell = possibleCells[random];
                //Changes the randomly selected cells value
                currentCell.content = m_SolvedPuzzle.grid[currentCell.rowNumber][currentCell.columnNumber].content;
                currentCell.algorithmEntered = true;
                string algorithmToBeAdded;
                //Adds the string for the algorithms used list box
                m_CompletedPuzzleAlgorithms.TryGetValue(String.Format("{0},{1}", currentCell.rowNumber, currentCell.columnNumber), out algorithmToBeAdded);
                m_AlgorithmsUsed.Add(algorithmToBeAdded);
                possibleCells.RemoveAt(random);
                numberOfHintsGiven++;
            }

            if (pCurrentPuzzle.IsComplete())
            {
                //If puzzle is complete stop the timer and create a message
                m_Timer.Stop();
                m_OutputMessage = String.Format("Puzzle is complete, it took {0} seconds", decimal.Round((decimal)m_Timer.ElapsedMilliseconds / 1000, 2));
                return true;
            }

            if (numberOfHintsGiven != pNumberOfHints)
            {
                //Number of hints given didnt match the number requested
                m_OutputMessage = String.Format("{0} hints could be given out of your requested {1}", numberOfHintsGiven, pNumberOfHints);
                return false;
            }

            return null;
        }
        #endregion

        #region Solving method
        /// <summary>
        /// Solves a puzzle using a number of different Sudoku solving algorithms
        /// </summary>
        /// <param name="pPuzzleToComplete">The puzzle which needs completing</param>
        /// <param name="pCompletedPuzzleDictionary">The dictionary of algorithms used to solve the puzzle</param>
        /// <param name="pOutputMessage">The output message of the solver (if its completeable or not)</param>
        /// <returns>True or False if the puzzle can be completed or not</returns>
        public static bool SolvePuzzle(ref Puzzle pPuzzleToComplete, out Dictionary<string, string> pCompletedPuzzleDictionary, out string pOutputMessage)
        {
            Stopwatch timer = new Stopwatch();
            pCompletedPuzzleDictionary = new Dictionary<string, string>();
            pOutputMessage = null;

            timer.Start();

            List<Cell> possibleCells = new List<Cell>();

            //Gets a list of possible cells which can have their values changed
            for (int i = 0; i < pPuzzleToComplete.puzzleSize; i++)
            {
                for (int j = 0; j < pPuzzleToComplete.puzzleSize; j++)
                {
                    Cell cellToAdd = pPuzzleToComplete.grid[i][j];

                    if (!cellToAdd.startPoint && !cellToAdd.algorithmEntered)
                    {
                        if (cellToAdd.content != 0)
                        {
                            cellToAdd.content = 0;
                            cellToAdd.UpdateAllPossibles();
                        }

                        cellToAdd.updatedPossiblesValues = new List<int>(cellToAdd.possibleValues);
                        possibleCells.Add(pPuzzleToComplete.grid[i][j]);
                    }
                }
            }

            int counter = 0;

            while (possibleCells.Count != 0 && counter < 100)
            {
                List<Cell> cellsToRemove = new List<Cell>();
                bool found = false;

                for (int i = 0; i < m_Algorithms.Length; i++)
                {
                    found = false;

                    foreach (Cell cell in possibleCells)
                    {
                        //Creates an instance of the current algorithm
                        Algorithm algorithm = Activator.CreateInstance(m_Algorithms[i]) as Algorithm;

                        //Checks to see if the algorithm can find something at that cell either a value or narrow down the possible values
                        //True if value found and entered
                        //False if nothing was entered and possibles couldnt be found
                        //Null if nothing was entered but possibles could be narrowed down
                        bool? searchResult = algorithm.Search(cell);

                        if (searchResult == true)
                        {
                            cellsToRemove.Add(cell);

                            //Add the algorithm message to the list or update already existing one
                            if (pCompletedPuzzleDictionary.ContainsKey(String.Format("{0},{1}", cell.rowNumber, cell.columnNumber)))
                            {
                                pCompletedPuzzleDictionary[String.Format("{0},{1}", cell.rowNumber, cell.columnNumber)] = String.Format("{0}\n{1}", pCompletedPuzzleDictionary[String.Format("{0},{1}", cell.rowNumber, cell.columnNumber)], algorithm.ToString());
                            }
                            else
                            {
                                pCompletedPuzzleDictionary.Add(String.Format("{0},{1}", cell.rowNumber, cell.columnNumber), algorithm.ToString());
                            }

                            found = true;
                        }
                        else if (searchResult == null)
                        {
                            //Add algorithm message to the dictionary
                            if (!pCompletedPuzzleDictionary.ContainsKey(String.Format("{0},{1}", cell.rowNumber, cell.columnNumber)))
                            {
                                pCompletedPuzzleDictionary.Add(String.Format("{0},{1}", cell.rowNumber, cell.columnNumber), algorithm.ToString());
                            }

                            found = true;
                        }
                    }

                    //Break out of the algorthm loop if something was found in the cells using the algorithm
                    if (found)
                    {
                        break;
                    }
                }

                //If not found breaks out of while loop because nothng could be found at any cells using any algorithm
                if (!found)
                {
                    break;
                }
                else
                {
                    //Updates the possible values of all cells with their updated values
                    foreach (Cell cell in possibleCells)
                    {
                        cell.possibleValues = new List<int>(cell.updatedPossiblesValues);
                    }

                    //Removes the cells at which a value was entered from the possible cells ie. cells to find a value for list
                    foreach (Cell cell in cellsToRemove)
                    {
                        possibleCells.Remove(cell);
                    }
                }

                counter++;
            }

            //If there are still possible values left in the list after running all algorithms then fallback to the brute force algorithm to solve the remainder of the puzzle
            if (possibleCells.Count > 0)
            {
                BruteForce bruteForce = new BruteForce();
                if (bruteForce.Solve(ref pPuzzleToComplete))
                {
                    foreach (Cell cell in possibleCells)
                    {
                        if (pCompletedPuzzleDictionary.ContainsKey(String.Format("{0},{1}", cell.rowNumber, cell.columnNumber)))
                        {
                            pCompletedPuzzleDictionary[String.Format("{0},{1}", cell.rowNumber, cell.columnNumber)] = String.Format("{0}\n{1}", pCompletedPuzzleDictionary[String.Format("{0},{1}", cell.rowNumber, cell.columnNumber)], String.Format("Cell at position {0}, {1} was changed to value {2} using Brute Force algorithm", cell.rowNumber + 1, cell.columnNumber + 1, Symbol.CURRENTSYMBOLS[bruteForce.getSolution(0)[cell.rowNumber, cell.columnNumber]]));
                        }
                        else
                        {
                            pCompletedPuzzleDictionary.Add(String.Format("{0},{1}", cell.rowNumber, cell.columnNumber), String.Format("Cell at position {0}, {1} was changed to value {2} using Brute Force algorithm", cell.rowNumber + 1, cell.columnNumber + 1, Symbol.CURRENTSYMBOLS[bruteForce.getSolution(0)[cell.rowNumber, cell.columnNumber]]));
                        }
                    }
                }
            }

            timer.Stop();

            //If puzzle is complete write output message
            if (pPuzzleToComplete.IsComplete())
            {
                pOutputMessage = String.Format("Puzzle has been completed in {0} ticks, {1} milliseconds or {2} seconds using algorithms!", timer.ElapsedTicks, timer.ElapsedMilliseconds, decimal.Round((decimal)timer.ElapsedMilliseconds / 1000, 2));
                return true;
            }

            return false;
        }
        #endregion

        #region Accessors
        public string outputMessage
        {
            get { return m_OutputMessage; }
            set { m_OutputMessage = value; }
        }

        public string completeMessage
        {
            get { return m_SolvedMessage; }
            set { m_SolvedMessage = value; }
        }

        public Puzzle complete
        {
            get { return m_SolvedPuzzle; }
            set { m_SolvedPuzzle = value; Changed("complete"); }
        }

        public ObservableCollection<string> completedAlgorithmsUsed
        {
            get
            {
                ObservableCollection<string> temp = new ObservableCollection<string>();

                foreach (string value in m_CompletedPuzzleAlgorithms.Values)
                {
                    temp.Add(value);
                }
                return temp;
            }
        }

        public Dictionary<string, string> completedAlgorithms
        {
            set { m_CompletedPuzzleAlgorithms = value; }
        }

        public Stopwatch timer
        {
            get { return m_Timer; }
        }

        public ObservableCollection<string> algorithmsUsed
        {
            get { return m_AlgorithmsUsed; }
            set { m_AlgorithmsUsed = value; Changed("algorithmsUsed"); }
        }
        #endregion

        #region Misc
        public event PropertyChangedEventHandler PropertyChanged;

        private void Changed(string pPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
        }
        #endregion
    }
}
