using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    public class BruteForce
    {
        private const int m_MaxNoSolutions = 1;
        private int m_PuzzleSize;
        private GridSize m_SubGridSize;
        private int m_NumberOfSolutions = 0;
        private List<int[,]> m_Solutions = new List<int[,]>();

        public bool Solve(ref Puzzle pPuzzle)
        {
            m_PuzzleSize = pPuzzle.puzzleSize;
            m_SubGridSize = pPuzzle.subGridSize;

            SolvePuzzle(pPuzzle.puzzleAsArray);

            if(m_NumberOfSolutions == m_MaxNoSolutions)
            {
                for(int i = 0; i < m_PuzzleSize; i++)
                {
                    for(int j = 0; j < m_PuzzleSize; j++)
                    {
                        pPuzzle.grid[i][j].content = m_Solutions[0][i, j];
                        pPuzzle.grid[i][j].algorithmEntered = true;
                    }
                }

                return true;
            }

            return false;
        }

        public bool Solve(int[,] pPuzzle, int pPuzzleSize, GridSize pSubGridSize)
        {
            m_PuzzleSize = pPuzzleSize;
            m_SubGridSize = pSubGridSize;

            SolvePuzzle(pPuzzle);

            if(m_NumberOfSolutions == m_MaxNoSolutions)
            {
                return true;
            }

            return false;
        }

        private bool SolvePuzzle(int[,] pPuzzle)
        {
            for(int row = 0; row < m_PuzzleSize; row++)
            {
                for(int col = 0; col < m_PuzzleSize; col++)
                {
                    if(pPuzzle[row, col] == 0)
                    {
                        return TryCell(pPuzzle, row, col);
                    }
                }
            }

            return true;
        }

        private bool TryCell(int[,] pPuzzle, int pRow, int pCol)
        {
            int[] possibles = PossibleValues(pPuzzle, pRow, pCol);
            for(int i = 0; i < possibles.Length; i++)
            {
                pPuzzle[pRow, pCol] = possibles[i];
                if(SolvePuzzle(pPuzzle))
                {
                    if (!Contains(m_Solutions, pPuzzle))
                    {
                        m_Solutions.Add(CopyPuzzle(pPuzzle));
                        m_NumberOfSolutions++;
                    }

                    if (m_NumberOfSolutions > m_MaxNoSolutions)
                    {
                        return true;
                    }
                }
            }

            /*for (int num = 1; num <= m_PuzzleSize; num++)
            {
                if (DirectionallyUnique(pPuzzle, num, pRow, pCol))
                {
                    pPuzzle[pRow, pCol] = num;
                    if (SolvePuzzle(pPuzzle))
                    {
                        if (!Contains(m_Solutions, pPuzzle))
                        {
                            m_Solutions.Add(CopyPuzzle(pPuzzle));
                            m_NumberOfSolutions++;
                        }

                        if (m_NumberOfSolutions > m_MaxNoSolutions)
                        {
                            return true;
                        }
                    }
                }
            }*/

            pPuzzle[pRow, pCol] = 0;
            return false;
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

        private bool DirectionallyUnique(int[,] pPuzzle, int pNum, int pRow, int pCol)
        {
            return UniqueInRow(pPuzzle, pNum, pRow, pCol) && UniqueInColumn(pPuzzle, pNum, pRow, pCol) && UniqueInSubGrid(pPuzzle, pNum, pRow, pCol);
        }

        private bool UniqueInRow(int[,] pPuzzle, int pNum, int pRow, int pCol)
        {
            for(int i = 0; i < m_PuzzleSize; i++)
            {
                if(i != pRow && pPuzzle[i, pCol] == pNum)
                {
                    return false;
                }
            }

            return true;
        }

        private bool UniqueInColumn(int[,] pPuzzle, int pNum, int pRow, int pCol)
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                if (i != pCol && pPuzzle[pRow, i] == pNum)
                {
                    return false;
                }
            }

            return true;
        }

        private bool UniqueInSubGrid(int[,] pPuzzle, int pNum, int pRow, int pCol)
        {
            int row = (pRow / m_SubGridSize.rowCount) * m_SubGridSize.rowCount;
            int col = (pCol / m_SubGridSize.columnCount) * m_SubGridSize.columnCount;

            for (int currentRow = row; currentRow < row + m_SubGridSize.rowCount; currentRow++)
            {
                for(int currentCol = col; currentCol < col + m_SubGridSize.columnCount; currentCol++)
                {
                    if((currentRow != pRow && currentCol != pCol) && pPuzzle[currentRow, currentCol] == pNum)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool Contains(List<int[,]> pList, int[,] pPuzzle)
        {
            foreach (int[,] puzzle in pList)
            {
                if (Equals(puzzle, pPuzzle))
                {
                    return true;
                }
            }

            return false;
        }

        private bool Equals(int[,] pPuzzleToTest, int[,] pPuzzleAgainst)
        {
            for (int row = 0; row < pPuzzleToTest.GetLength(0); row++)
            {
                for (int col = 0; col < pPuzzleToTest.GetLength(1); col++)
                {
                    if (pPuzzleToTest[row, col] != pPuzzleAgainst[row, col])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private int[,] CopyPuzzle(int[,] pPuzzle)
        {
            int[,] puzzle = new int[m_PuzzleSize, m_PuzzleSize];

            for(int row = 0; row < m_PuzzleSize; row++)
            {
                for(int col = 0; col < m_PuzzleSize; col++)
                {
                    puzzle[row, col] = pPuzzle[row, col];
                }
            }

            return puzzle;
        }

        #region Accessors
        public int[,] getSolution(int pSolutionNumber)
        {
            if(pSolutionNumber > m_Solutions.Count || pSolutionNumber < 0)
            {
                throw new Exception(String.Format("Solution number invalid, must be between 0 and {0}", m_Solutions.Count));
            }

            return m_Solutions[pSolutionNumber];
        }

        public List<int[,]> getAllSolutions
        {
            get { return this.m_Solutions; }
        }

        public int numberOfSolutions
        {
            get { return this.m_NumberOfSolutions; }
        }
        #endregion
    }
}
