using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SudokuLib.Model.Algorithms;

namespace SudokuLib.Model
{
    public class Generator
    {
        private BruteForce m_BruteForce = new BruteForce();
        private int m_PuzzleSize;
        private GridSize m_SubGridSize;

        public Puzzle GeneratePuzzle(int pPuzzleSize, GridSize pSubGridSize)
        {
            m_PuzzleSize = pPuzzleSize;
            m_SubGridSize = pSubGridSize;

            int[,] puzzle = new int[m_PuzzleSize, m_PuzzleSize];

            m_BruteForce = new BruteForce();
            int numberOfCellsEntered = 0;
            bool puzzleFound = false;
            while (!puzzleFound)
            {
                int row = RNG.RANDOM.Next(0, m_PuzzleSize);
                int col = RNG.RANDOM.Next(0, m_PuzzleSize);

                while (puzzle[row, col] != 0)
                {
                    row = RNG.RANDOM.Next(0, m_PuzzleSize);
                    col = RNG.RANDOM.Next(0, m_PuzzleSize);
                }

                int[] possibles = PossibleValues(puzzle, row, col);

                if (possibles.Length > 0)
                {
                    puzzle[row, col] = possibles[RNG.RANDOM.Next(possibles.Length)];
                    numberOfCellsEntered++;
                }
                else
                {
                    numberOfCellsEntered = 0;
                    puzzle = new int[m_PuzzleSize, m_PuzzleSize];
                    continue;
                }

                if (numberOfCellsEntered > (m_PuzzleSize * m_PuzzleSize) / (m_PuzzleSize / 2))
                {
                    m_BruteForce = new BruteForce();
                    var task = Task.Run(() => m_BruteForce.Solve(puzzle, m_PuzzleSize, m_SubGridSize));
                    if (!task.Wait(TimeSpan.FromSeconds(1)))
                    {
                        m_BruteForce = new BruteForce();
                        numberOfCellsEntered = 0;
                        puzzle = new int[m_PuzzleSize, m_PuzzleSize];
                        continue;
                    }

                    if (m_BruteForce.numberOfSolutions > 0)
                    {
                        puzzleFound = true;
                        continue;
                    }
                    else
                    {
                        m_BruteForce = new BruteForce();
                        numberOfCellsEntered = 0;
                        puzzle = new int[m_PuzzleSize, m_PuzzleSize];
                    }
                }
            }

            m_BruteForce = new BruteForce();
            int numberOfHints = (m_PuzzleSize * m_PuzzleSize) / (m_PuzzleSize / 2) + ((m_PuzzleSize * m_PuzzleSize) / 4);
            int currentNumberOfHints = m_PuzzleSize * m_PuzzleSize;
            int tries = 0;
            int lowestNumber = int.MaxValue;
            int[,] currentPuzzle = Clone(puzzle);
            int[,] previousPuzzle = Clone(puzzle);
            while (true)
            {
                int row = RNG.RANDOM.Next(0, m_PuzzleSize);
                int col = RNG.RANDOM.Next(0, m_PuzzleSize);

                while (currentPuzzle[row, col] == 0)
                {
                    row = RNG.RANDOM.Next(0, m_PuzzleSize);
                    col = RNG.RANDOM.Next(0, m_PuzzleSize);
                }

                currentPuzzle[row, col] = 0;
                currentNumberOfHints--;

                m_BruteForce = new BruteForce();
                m_BruteForce.Solve(Clone(currentPuzzle), m_PuzzleSize, m_SubGridSize);
                    

                if (m_BruteForce.numberOfSolutions == 1)
                {
                    if (currentNumberOfHints < lowestNumber)
                    {
                        lowestNumber = currentNumberOfHints;
                        previousPuzzle = Clone(currentPuzzle);
                    }
                    continue;
                }
                else
                {
                    if (currentNumberOfHints <= numberOfHints || tries == 10)
                    {
                        puzzle = previousPuzzle;
                        break;
                    }
                    else
                    {
                        tries++;
                        currentPuzzle = Clone(puzzle);
                        m_BruteForce = new BruteForce();
                        currentNumberOfHints = m_PuzzleSize * m_PuzzleSize;
                    }
                }
            }

            return new Puzzle(puzzle, m_PuzzleSize, m_SubGridSize);
        }

        private int[,] Clone(int[,] pPuzzle)
        {
            int[,] puzzle = new int[m_PuzzleSize, m_PuzzleSize];

            for (int i = 0; i < m_PuzzleSize; i++)
            {
                for (int j = 0; j < m_PuzzleSize; j++)
                {
                    puzzle[i, j] = pPuzzle[i, j];
                }
            }

            return puzzle;
        }

        private int[] PossibleValues(int[,] pPuzzle, int pRow, int pCol)
        {
            List<int> possibleValues = new List<int>();
            for (int i = 1; i <= m_PuzzleSize; i++)
            {
                possibleValues.Add(i);
            }

            for (int i = 0; i < m_PuzzleSize; i++)
            {
                if (i != pRow && pPuzzle[i, pCol] != 0)
                {
                    possibleValues.Remove(pPuzzle[i, pCol]);
                }

                if (i != pCol && pPuzzle[pRow, i] != 0)
                {
                    possibleValues.Remove(pPuzzle[pRow, i]);
                }
            }

            int row = (pRow / m_SubGridSize.rowCount) * m_SubGridSize.rowCount;
            int col = (pCol / m_SubGridSize.columnCount) * m_SubGridSize.columnCount;

            for (int currentRow = row; currentRow < row + m_SubGridSize.rowCount; currentRow++)
            {
                for (int currentCol = col; currentCol < col + m_SubGridSize.columnCount; currentCol++)
                {
                    if ((currentRow != pRow && currentCol != pCol) && pPuzzle[currentRow, currentCol] != 0)
                    {
                        possibleValues.Remove(pPuzzle[currentRow, currentCol]);
                    }
                }
            }

            return possibleValues.ToArray();
        }
    }
}
