using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model
{
    public class Puzzle : INotifyPropertyChanged
    {
        private int m_PuzzleSize;
        private GridSize m_SubGridSize;
        private ObservableCollection<ObservableCollection<Cell>> m_Rows = new ObservableCollection<ObservableCollection<Cell>>();
        char[] m_CurrentSymbolSet;

        #region Constructors
        /// <summary>
        /// Contructor called when loading a puzzle from a file
        /// </summary>
        /// <param name="pFileName">The path of the puzzle file</param>
        public Puzzle(string pFileName)
        {
            ReadFile(pFileName);
        }

        public Puzzle(int[,] pPuzzle, int pPuzzleSize, GridSize pSubGridSize)
        {
            m_PuzzleSize = pPuzzleSize;
            m_SubGridSize = pSubGridSize;
            Symbol.SetSymbols(m_PuzzleSize);
            m_CurrentSymbolSet = new char[m_PuzzleSize];
            Array.Copy(Symbol.CURRENTSYMBOLS, 1, m_CurrentSymbolSet, 0, m_PuzzleSize);
            SetPuzzle(pPuzzle);
        }

        /// <summary>
        /// Constructor used when loading a blank puzzle
        /// </summary>
        /// <param name="pSize">The size of the puzzle</param>
        public Puzzle(int pPuzzleSize, GridSize pSubGridSize)
        {
            m_PuzzleSize = pPuzzleSize;
            m_SubGridSize = pSubGridSize;
            Symbol.SetSymbols(m_PuzzleSize);
            m_CurrentSymbolSet = new char[m_PuzzleSize];
            Array.Copy(Symbol.CURRENTSYMBOLS, 1, m_CurrentSymbolSet, 0, m_PuzzleSize);
            SetPuzzle();
        }

        /// <summary>
        /// Used to clone a puzzle
        /// </summary>
        /// <param name="pPuzzle">The puzzle to be cloned</param>
        private Puzzle(Puzzle pPuzzle)
        {
            m_PuzzleSize = pPuzzle.m_PuzzleSize;
            m_SubGridSize = pPuzzle.m_SubGridSize;
            m_Rows = new ObservableCollection<ObservableCollection<Cell>>();
            m_CurrentSymbolSet = pPuzzle.m_CurrentSymbolSet;
            Symbol.SetSymbols(m_CurrentSymbolSet);

            foreach (ObservableCollection<Cell> row in pPuzzle.m_Rows)
            {
                ObservableCollection<Cell> column = new ObservableCollection<Cell>();
                foreach (Cell cell in row)
                {
                    Cell currentCell = cell.Clone();
                    column.Add(currentCell);
                }
                m_Rows.Add(column);
            }

            SetCellRelations();
            SetCellPossibleValues();
        }
        #endregion

        #region Save game
        /// <summary>
        /// Saves the current state puzzle to a readable text file
        /// </summary>
        /// <param name="pFileName">The file path</param>
        public void SaveFile(string pFileName)
        {
            using (StreamWriter writer = new StreamWriter(pFileName))
            {
                string line = "";

                //Adds the current symbol set ot the line
                for(int i = 1; i < Symbol.CURRENTSYMBOLS.Length; i++)
                {
                    line += Symbol.CURRENTSYMBOLS[i];
                }

                //Writes the symbol set line to the file
                writer.WriteLine(line);

                //Sets the line ot the subgrid size string
                line = m_SubGridSize.ToString();

                //Writes the sub grid size to the file
                writer.WriteLine(line);

                for (int i = 0; i < m_PuzzleSize; i++)
                {
                    //If the cell was algorithm entered put an A before the content, if it was not a start point then it was user entered
                    line = "";
                    if(m_Rows[i][0].algorithmEntered)
                    {
                        line += "A";
                    }
                    else if(!m_Rows[i][0].startPoint)
                    {
                        line += "U";
                    }

                    //Adds the content after the A or U
                    line += m_Rows[i][0].content.ToString();

                    //Repeat for the rest of the values in the row
                    for(int j = 1; j < m_PuzzleSize; j++)
                    {
                        line += " ";

                        if (m_Rows[i][j].algorithmEntered)
                        {
                            line += "A";
                        }
                        else if(!m_Rows[i][j].startPoint)
                        {
                            line += "U";
                        }

                        line += m_Rows[i][j].content;
                    }

                    //Write the row to the text file
                    writer.WriteLine(line);
                }
            }
        }
        #endregion

        #region Parse File
        /// <summary>
        /// Reads the file into the puzzle
        /// </summary>
        /// <param name="pFileName">The path fo the puzzle file</param>
        private void ReadFile(string pFileName)
        {
            using (StreamReader reader = new StreamReader(pFileName))
            {
                string line = reader.ReadLine();

                //Symbol set is at the top of the input file
                char[] symbols = line.ToCharArray();
                m_CurrentSymbolSet = symbols;
                Symbol.SetSymbols(m_CurrentSymbolSet);

                m_PuzzleSize = m_CurrentSymbolSet.Length;

                line = reader.ReadLine();

                GetSubGridSize(line);

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    string[] puzzleRow = System.Text.RegularExpressions.Regex.Split(line, " +");

                    m_Rows.Add(SetRow(puzzleRow));
                }
            }

            //Checks if the loaded puzzle is valid, ie does not contain duplicate values in a row, column, subgrid
            //then sets relations for each cell and the possible values
            if (ValidateFile())
            {
                SetCellRelations();
                SetCellPossibleValues();
            }
        }

        /// <summary>
        /// Sets the size of the sub grid
        /// </summary>
        /// <param name="pSubGridSize">The raw entry from the puzzle file for the sub grid size</param>
        private void GetSubGridSize(string pSubGridSize)
        {
            //Checks to ensure it is in the correct format
            if (pSubGridSize.Length == 3)
            {
                //Parses the size string
                m_SubGridSize = GridSize.Parse(pSubGridSize);

                //Checks to ensure the sub grid rows and columns multiplies is the size of the full puzzle
                if (m_SubGridSize.rowCount * m_SubGridSize.columnCount != m_PuzzleSize)
                {
                    throw new Exception(String.Format("File read error - Sub grid values must multiply ({0}) to equal the length of the symbol set ({1}) (Puzzle size)", m_SubGridSize.rowCount * m_SubGridSize.columnCount, m_PuzzleSize));
                }
            }
            else
            {
                throw new Exception("File read error - There must only be 1 sub grid size of rows followed by collumns eg. 3x3");
            }
        }

        /// <summary>
        /// Check to ensure a puzzle is valid
        /// </summary>
        /// <returns>True if valid false if not</returns>
        private bool ValidateFile()
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                //Checks if there are duplicate values in the rows and columns
                if (!CheckLine(m_Rows[i].ToList()) || !CheckLine(GetColumn(m_Rows[0][i])))
                {
                    throw new Exception("File invalid - Values can not be duplicated");
                }

                //Checks to see if each row and column is the same length
                if (m_Rows[i].Count != m_PuzzleSize || GetColumn(m_Rows[0][i]).Count != m_PuzzleSize)
                {
                    throw new Exception("File invalid - The puzzle size must match the length of the symbol set");
                }
            }

            //Checks to ensure there arent duplicate values in the sub grids
            for (int i = 0; i < m_SubGridSize.rowCount; i++)
            {
                for (int j = 0; j < m_SubGridSize.columnCount; j++)
                {
                    if (!CheckLine(GetSubGrid(m_Rows[(i / m_SubGridSize.rowCount) * m_SubGridSize.rowCount][(j / m_SubGridSize.columnCount) * m_SubGridSize.columnCount])))
                    {
                        throw new Exception("File invalid - Values can not be duplicated");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the possible values for each cell in the puzzle
        /// </summary>
        private void SetCellPossibleValues()
        {
            //Gets each cell and sets the possible values
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                for (int j = 0; j < m_PuzzleSize; j++)
                {
                    m_Rows[i][j].possibleValues = m_Rows[i][j].SetPossibleValues();
                }
            }
        }

        /// <summary>
        /// Sets a row of the puzzle
        /// </summary>
        /// <param name="pPuzzleRow">The row taken from the puzzle file</param>
        /// <returns>A column of cells</returns>
        private ObservableCollection<Cell> SetRow(string[] pPuzzleRow)
        {
            ObservableCollection<Cell> column = new ObservableCollection<Cell>();
            for (int i = 0; i < pPuzzleRow.Length; i++)
            {
                char contentEnteredBy;
                string content;

                //Checks if the content was entered by the user (u) or algorithm (a), this is for when loading a saved puzzle
                if (Char.ToLower(pPuzzleRow[i][0]) == 'u' || Char.ToLower(pPuzzleRow[i][0]) == 'a')
                {
                    contentEnteredBy = pPuzzleRow[i][0];
                    content = pPuzzleRow[i].Substring(1);
                }
                //Else the content is a startign point
                else
                {
                    contentEnteredBy = ' ';
                    content = pPuzzleRow[i];
                }
                
                int cellContent = 0;
                if (!int.TryParse(content, out cellContent))
                {
                    throw new Exception("File read error - Puzzle content must contain values from 0 to puzzle size");
                }

                if(cellContent > m_PuzzleSize || cellContent < 0)
                {
                    throw new Exception("File read error - Cell value must be in the range of 0 to the size of the puzzle");
                }

                //Creates a new cell based on the input file
                Cell cell = new Cell(m_PuzzleSize, cellContent, cellContent != 0 && Char.ToLower(contentEnteredBy) != 'u' && Char.ToLower(contentEnteredBy) != 'a' ? true : false, Char.ToLower(contentEnteredBy) == 'a' ? true : false, CellColoursApplied(i, m_Rows.Count), m_Rows.Count, i);
                column.Add(cell);
            }

            return column;
        }

        /// <summary>
        /// Applies whether the cell should be colored or not
        /// </summary>
        /// <param name="pColumnCount">The current column count</param>
        /// <param name="pRowCount">The current row count</param>
        /// <returns>Returns True if cell should be coloured False if not</returns>
        private bool CellColoursApplied(int pColumnCount, int pRowCount)
        {
            //Checks if the puzzle row size or sub grid row size is even or not
            if (m_PuzzleSize % 2 != 0 || m_SubGridSize.rowCount % 2 != 0)
            {
                //Calculates which sub grid number the cell belongs in, if the sub grid number is odd then colour else dont
                if (((pColumnCount / m_SubGridSize.columnCount) + (pRowCount / m_SubGridSize.rowCount) * m_SubGridSize.rowCount) % 2 != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //Calculates if the sub grid number is even and the row count is odd or if the sub grid number is odd and the row count even, if either are true then coloured else not coloured
                if ((((pColumnCount / m_SubGridSize.columnCount) + (pRowCount / m_SubGridSize.rowCount) * m_SubGridSize.rowCount) % 2 == 0 && (pRowCount / m_SubGridSize.rowCount) % 2 != 0) || (((pColumnCount / m_SubGridSize.columnCount) + (pRowCount / m_SubGridSize.rowCount) * m_SubGridSize.rowCount) % 2 != 0 && (pRowCount / m_SubGridSize.rowCount) % 2 == 0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Sets a blank puzzle for use with manual entry
        /// </summary>
        private void SetPuzzle()
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                ObservableCollection<Cell> row = new ObservableCollection<Cell>();
                for (int j = 0; j < m_PuzzleSize; j++)
                {
                    //Creates new blank cells for the size of the puzzle grid
                    Cell cell = new Cell(m_PuzzleSize, CellColoursApplied(j, m_Rows.Count), i, j);
                    row.Add(cell);
                }
                m_Rows.Add(row);
            }

            //Sets each cells relations
            SetCellRelations();

            //Sets each cells possible values
            SetCellPossibleValues();
        }

        private void SetPuzzle(int[,] pPuzzle)
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                ObservableCollection<Cell> row = new ObservableCollection<Cell>();
                for (int j = 0; j < m_PuzzleSize; j++)
                {
                    //Creates new blank cells for the size of the puzzle grid
                    Cell cell = new Cell(m_PuzzleSize, pPuzzle[i, j], pPuzzle[i,j] == 0 ? false : true, false, CellColoursApplied(j, m_Rows.Count), i, j);
                    row.Add(cell);
                }
                m_Rows.Add(row);
            }

            //Sets each cells relations
            SetCellRelations();

            //Sets each cells possible values
            SetCellPossibleValues();
        }

        /// <summary>
        /// Sets each cells relation to other cells in the grid
        /// </summary>
        private void SetCellRelations()
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                for (int j = 0; j < m_PuzzleSize; j++)
                {
                    Cell currentCell = m_Rows[i][j];

                    //Sets the cells row, column and sub grid relation
                    currentCell.cellRow = m_Rows[i].ToList();
                    currentCell.cellColumn = GetColumn(currentCell);
                    currentCell.cellSubGrid = GetSubGrid(currentCell);
                }
            }
        }

        /// <summary>
        /// Gets a cells column from the grid
        /// </summary>
        /// <param name="pCell">The cell whose column is to be found</param>
        /// <returns>List of the cells column</returns>
        private List<Cell> GetColumn(Cell pCell)
        {
            List<Cell> column = new List<Cell>();
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                column.Add(m_Rows[i][pCell.columnNumber]);
            }
            return column;
        }

        /// <summary>
        /// Gets a cells sub grid from the grid
        /// </summary>
        /// <param name="pCell">The cell whose sub grid is to be found</param>
        /// <returns>List of the cells sub grid</returns>
        private List<Cell> GetSubGrid(Cell pCell)
        {
            List<Cell> subGrid = new List<Cell>();

            for (int i = 0; i < m_SubGridSize.rowCount; i++)
            {
                for (int j = 0; j < m_SubGridSize.columnCount; j++)
                {
                    Cell cell = m_Rows[((pCell.rowNumber / m_SubGridSize.rowCount) * m_SubGridSize.rowCount) + i][((pCell.columnNumber / m_SubGridSize.columnCount) * m_SubGridSize.columnCount) + j];

                    //Gets the sub grid by getting the row number of the cell dividing it by the sub grid size multiplying it by the
                    //sub grid size and adding the current for loop itteration
                    subGrid.Add(cell);
                }
            }
            return subGrid;
        }

        /// <summary>
        /// Checks to ensure every item in a row is unique
        /// </summary>
        /// <param name="pLineToCheck">The row to check</param>
        /// <returns></returns>
        private bool CheckLine(List<Cell> pLineToCheck)
        {
            List<int> usedValues = new List<int>();
            for (int i = 0; i < pLineToCheck.Count; i++)
            {
                if (pLineToCheck[i].startPoint == true || pLineToCheck[i].algorithmEntered == true)
                {
                    usedValues.Add(pLineToCheck[i].content);
                }
            }

            //Checks the content of the list against the number of distinct values in the list
            //if these values are different, there are duplicates
            return usedValues.Count == usedValues.Distinct().Count();
        }

        /// <summary>
        /// Checks to ensure that values in the line exist in the symbol set
        /// </summary>
        /// <param name="pLineToCheck">The current line that needs checking</param>
        /// <returns>True if line is okay False if line contains incorrect values</returns>
        private bool CheckCompleteLine(List<Cell> pLineToCheck)
        {
            List<int> usedValues = new List<int>();
            
            //Creates a list of integers that exist in the line
            for (int i = 0; i < pLineToCheck.Count; i++)
            {
                usedValues.Add(pLineToCheck[i].content);
            }

            //Checks if htose integers exist in the symbol set
            for (int i = 1; i < Symbol.currentSymbolsIntList.Count; i++)
            {
                if (!usedValues.Contains(Symbol.currentSymbolsIntList[i]))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Complete check
        /// <summary>
        /// Checks if the puzzle is complete or not
        /// </summary>
        /// <returns>Returns True if puzzle is complete False if not</returns>
        public bool IsComplete()
        {
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                //Checks if there are duplicate values in the rows and columns
                if (!CheckCompleteLine(m_Rows[i].ToList()) || !CheckCompleteLine(GetColumn(m_Rows[0][i])))
                {
                    return false;
                }
            }

            //Checks to ensure there arent duplicate values in the sub grids
            for (int i = 0; i < m_SubGridSize.rowCount; i++)
            {
                for (int j = 0; j < m_SubGridSize.columnCount; j++)
                {
                    if (!CheckCompleteLine(GetSubGrid(m_Rows[(i / m_SubGridSize.rowCount) * m_SubGridSize.rowCount][(j / m_SubGridSize.columnCount) * m_SubGridSize.columnCount])))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region Accessors
        public int[,] puzzleAsArray
        {
            get
            {
                int[,] puzzle = new int[m_PuzzleSize, m_PuzzleSize];

                for(int i = 0; i < m_Rows.Count; i++)
                {
                    for(int j = 0; j < m_Rows[i].Count; j++)
                    {
                        puzzle[i, j] = m_Rows[i][j].content;
                    }
                }

                return puzzle;
            }
        }

        public ObservableCollection<ObservableCollection<Cell>> grid
        {
            get { return m_Rows; }
            set { m_Rows = value; Changed("grid"); }
        }

        public int puzzleSize
        {
            get { return m_PuzzleSize; }
        }

        public GridSize subGridSize
        {
            get { return m_SubGridSize; }
        }

        public char[] currentSymbolSet
        {
            get { return m_CurrentSymbolSet; }
        }
        #endregion

        #region Misc
        /// <summary>
        /// Sets the start point on a new puzzle
        /// </summary>
        public void SetStartPoints()
        {
            //if the cells content is not zero set it as a startpoint
            for (int i = 0; i < m_PuzzleSize; i++)
            {
                for (int j = 0; j < m_PuzzleSize; j++)
                {
                    if (m_Rows[i][j].content != 0)
                    {
                        m_Rows[i][j].startPoint = true;
                    }
                }
            }
        }

        /// <summary>
        /// Calls the clone constructor
        /// </summary>
        /// <returns>A new clone of the puzzle</returns>
        public Puzzle Clone()
        {
            return new Puzzle(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Changed(string pParameterName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pParameterName));
        }
        #endregion
    }
}
