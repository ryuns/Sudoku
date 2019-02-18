using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model
{
    public class Cell : INotifyPropertyChanged
    {
        private int m_PuzzleSize;
        private int m_Content;
        private bool m_StartPoint;
        private bool m_AlgorithmEntered;
        private bool m_Coloured;
        private int m_ColumnNumber;
        private int m_RowNumber;
        private List<Cell> m_CellRow = new List<Cell>();
        private List<Cell> m_CellColumn = new List<Cell>();
        private List<Cell> m_CellSubGrid = new List<Cell>();
        private List<int> m_PossibleValues = new List<int>();
        private List<int> m_UpdatedPossibleValues = new List<int>();

        #region Constructors
        /// <summary>
        /// Constructor to set the cell for a manual entry puzzle
        /// </summary>
        /// <param name="pGridSize">The grid size</param>
        /// <param name="pRowNumber">The row number of the cell</param>
        /// <param name="pColumnNumber">The column number of the cell</param>
        public Cell(int pPuzzleSize, bool pColoured, int pRowNumber, int pColumnNumber)
        {
            m_PuzzleSize = pPuzzleSize;
            m_Content = 0;
            m_StartPoint = false;
            m_AlgorithmEntered = false;
            m_Coloured = pColoured;
            m_RowNumber = pRowNumber;
            m_ColumnNumber = pColumnNumber;
        }

        /// <summary>
        /// Constructor used when reading a file
        /// </summary>
        /// <param name="pGridSize">The grid size</param>
        /// <param name="pContent">The content of the cell</param>
        /// <param name="pStartPoint">If the cell is a start point or not</param>
        /// <param name="pRowNumber">The row number of the cell</param>
        /// <param name="pColumnNumber">The column number fothe cell</param>
        public Cell(int pPuzzleSize, int pContent, bool pStartPoint, bool pAlgorithmEntered, bool pColoured, int pRowNumber, int pColumnNumber)
        {
            m_PuzzleSize = pPuzzleSize;
            m_Content = pContent;
            m_StartPoint = pStartPoint;
            m_AlgorithmEntered = pAlgorithmEntered;
            m_Coloured = pColoured;
            m_RowNumber = pRowNumber;
            m_ColumnNumber = pColumnNumber;
        }

        /// <summary>
        /// Constructor to clone a Cell
        /// </summary>
        /// <param name="pCell">The Cell to be cloned</param>
        private Cell(Cell pCell)
        {
            m_PuzzleSize = pCell.m_PuzzleSize;
            m_Content = pCell.m_Content;
            m_StartPoint = pCell.m_StartPoint;
            m_AlgorithmEntered = pCell.m_AlgorithmEntered;
            m_Coloured = pCell.m_Coloured;
            m_ColumnNumber = pCell.m_ColumnNumber;
            m_RowNumber = pCell.m_RowNumber;
        }
        #endregion

        #region Possible values methods
        /// <summary>
        /// Sets the possible vales for each cell related to this cell
        /// </summary>
        /// <returns>Possible values as a list of ints</returns>
        public List<int> SetPossibleValues()
        {
            List<int> possibles = new List<int>();

            if (!m_StartPoint)
            {
                //Adds all possible values to a list
                possibles = Symbol.currentSymbolsIntList;

                //Checks all of the cells related to this cell and removes possible values based on related cells content
                for (int i = 0; i < m_PuzzleSize; i++)
                {
                    RemovePossibles(m_CellRow[i], possibles);
                    RemovePossibles(m_CellColumn[i], possibles);
                    RemovePossibles(m_CellSubGrid[i], possibles);
                }
            }
            else
            {
                possibles.Add(m_Content);
            }

            return possibles;
        }

        private void RemovePossibles(Cell pCellToCheck, List<int> pPossibles)
        {
            if (pCellToCheck.m_Content != 0)
            {
                if (pPossibles.Contains(pCellToCheck.m_Content))
                {
                    if (pCellToCheck.m_Content != this.m_Content)
                    {
                        pPossibles.Remove(pCellToCheck.m_Content);
                    }
                }
            }
        }

        /// <summary>
        /// Updates all possible values for the cell
        /// </summary>
        public void UpdateAllPossibles()
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                m_CellColumn[i].possibleValues = m_CellColumn[i].SetPossibleValues();
                m_CellRow[i].possibleValues = m_CellRow[i].SetPossibleValues();
                m_CellSubGrid[i].possibleValues = m_CellSubGrid[i].SetPossibleValues();
            }
        }

        /// <summary>
        /// Removes a specific possible value from all cells related to this cell
        /// </summary>
        /// <param name="pPossibleToRemove">The value to remove</param>
        public void UpdateAllPossibles(int pPossibleToRemove)
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                if (!m_CellColumn[i].Equals(this) && !m_CellColumn[i].m_StartPoint && !m_CellColumn[i].m_AlgorithmEntered)
                {
                    m_CellColumn[i].m_UpdatedPossibleValues.Remove(pPossibleToRemove);
                }

                if (!m_CellRow[i].Equals(this) && !m_CellRow[i].m_StartPoint && !m_CellRow[i].m_AlgorithmEntered)
                {
                    m_CellRow[i].m_UpdatedPossibleValues.Remove(pPossibleToRemove);
                }

                if (!m_CellSubGrid[i].Equals(this) && !m_CellSubGrid[i].m_StartPoint && !m_CellSubGrid[i].m_AlgorithmEntered)
                {
                    m_CellSubGrid[i].m_UpdatedPossibleValues.Remove(pPossibleToRemove);
                }
            }
        }
        #endregion

        #region Accessors
        public int content
        {
            get { return m_Content; }
            set { m_Content = value; Changed("content"); }
        }

        public bool coloured
        {
            get { return m_Coloured; }
        }

        public bool startPoint
        {
            get { return m_StartPoint; }
            set { m_StartPoint = value; Changed("startPoint"); }
        }

        public bool algorithmEntered
        {
            get { return m_AlgorithmEntered; }
            set { m_AlgorithmEntered = value; Changed("algorithmEntered"); }
        }

        public List<int> possibleValues
        {
            get { return m_PossibleValues; }
            set { m_PossibleValues = value; Changed("possibleValues"); }
        }

        public List<int> updatedPossiblesValues
        {
            get { return m_UpdatedPossibleValues; }
            set { m_UpdatedPossibleValues = value; }
        }

        public List<int> possibleValuesWithout0
        {
            get
            {
                return new List<int>(m_PossibleValues.GetRange(1, m_PossibleValues.Count - 1));
            }
        }

        public List<Cell> cellColumnWithoutStartAndAlgo
        {
            get
            {
                List<Cell> temp = new List<Cell>();

                foreach (Cell cell in m_CellColumn)
                {
                    if (!cell.startPoint && !cell.algorithmEntered)
                    {
                        temp.Add(cell);
                    }
                }

                return temp;
            }
        }

        public List<Cell> cellRowWithoutStartAndAlgo
        {
            get
            {
                List<Cell> temp = new List<Cell>();

                foreach (Cell cell in m_CellRow)
                {
                    if (!cell.startPoint && !cell.algorithmEntered)
                    {
                        temp.Add(cell);
                    }
                }

                return temp;
            }
        }

        public List<Cell> cellSubGridWithoutStartAndAlgo
        {
            get
            {
                List<Cell> temp = new List<Cell>();

                foreach (Cell cell in m_CellSubGrid)
                {
                    if (!cell.startPoint && !cell.algorithmEntered)
                    {
                        temp.Add(cell);
                    }
                }

                return temp;
            }
        }

        public List<Cell> cellColumn
        {
            get { return m_CellColumn; }
            set { m_CellColumn = value; }
        }

        public List<Cell> cellRow
        {
            get { return m_CellRow; }
            set { m_CellRow = value; }
        }

        public List<Cell> cellSubGrid
        {
            get { return m_CellSubGrid; }
            set { m_CellSubGrid = value; }
        }

        public int puzzleSize
        {
            get { return m_PuzzleSize; }
        }

        public int columnNumber
        {
            get { return m_ColumnNumber; }
        }

        public int rowNumber
        {
            get { return m_RowNumber; }
        }
        #endregion

        #region Misc
        /// <summary>
        /// Checks if a Cell is equal to another Cell
        /// </summary>
        /// <param name="obj">The Cell to be compared</param>
        /// <returns>True if cells are qual false if not</returns>
        public override bool Equals(object obj)
        {
            Cell compare = obj as Cell;

            if (compare == null)
            {
                return false;
            }
            else
            {
                return this.m_ColumnNumber == compare.m_ColumnNumber && this.m_RowNumber == compare.m_RowNumber;
            }
        }

        /// <summary>
        /// Returns the hashcode of the object
        /// </summary>
        /// <returns>The hashcode int</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Calls the copy constructor
        /// </summary>
        /// <returns>A new copied cell</returns>
        public Cell Clone()
        {
            return new Cell(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Changed(string pPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pPropertyName));
        }
        #endregion
    }
}
