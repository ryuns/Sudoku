using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    class Pointers : Algorithm
    {
        private bool m_PossiblesChanged = false;

        #region Constructors
        public Pointers() : base("Pointing Pairs/Triples") { }
        #endregion

        #region Search Method
        public override bool? Search(Cell pCell)
        {
            m_Cell = pCell;
            List<Cell> cellsColumn = new List<Cell>();
            List<Cell> cellsRow = new List<Cell>();

            foreach (Cell cell in m_Cell.cellSubGridWithoutStartAndAlgo)
            {
                if (cell != m_Cell)
                {
                    if (cell.rowNumber == m_Cell.rowNumber && cell.possibleValuesWithout0.Intersect(m_Cell.possibleValuesWithout0).Count() > 0)
                    {
                        cellsRow.Add(cell);
                    }
                    else if (cell.columnNumber == m_Cell.columnNumber && cell.possibleValuesWithout0.Intersect(m_Cell.possibleValuesWithout0).Count() > 0)
                    {
                        cellsColumn.Add(cell);
                    }
                }
            }

            if (cellsColumn.Count > 0)
            {
                List<int> possibleValues = GetPossibleValuesForLine(cellsColumn);

                if (cellsColumn.Count > 0 && possibleValues.Count > 0)
                {
                    ValueExistsInLine(m_Cell.cellColumnWithoutStartAndAlgo, cellsColumn, possibleValues);

                    char[] symbols = new char[possibleValues.Count];
                    for (int i = 0; i < possibleValues.Count; i++)
                    {
                        symbols[i] = Symbol.CURRENTSYMBOLS[possibleValues[i]];
                    }

                    m_Row = m_Cell.rowNumber;
                    m_Column = m_Cell.columnNumber;

                    m_AlgorithmMessage = String.Format("Used to eliminate possible values {0} from Column", String.Join(", ", symbols));
                }
            }

            if (cellsRow.Count > 0)
            {
                List<int> possibleValues = GetPossibleValuesForLine(cellsRow);

                if(cellsRow.Count > 0 && possibleValues.Count > 0)
                {
                    ValueExistsInLine(m_Cell.cellRowWithoutStartAndAlgo, cellsRow, possibleValues);

                    char[] symbols = new char[possibleValues.Count];
                    for (int i = 0; i < possibleValues.Count; i++)
                    {
                        symbols[i] = Symbol.CURRENTSYMBOLS[possibleValues[i]];
                    }

                    m_Row = m_Cell.rowNumber;
                    m_Column = m_Cell.columnNumber;

                    m_AlgorithmMessage = String.Format("Used to eliminate possible values {0} from Row", String.Join(", ", symbols));
                }
            }

            if(m_PossiblesChanged)
            {
                return null;
            }

            return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCellDirection"></param>
        /// <param name="pCellLine"></param>
        /// <param name="pPossibleValue"></param>
        /// <param name="pPossiblesChanged"></param>
        private void ValueExistsInLine(List<Cell> pCellDirection, List<Cell> pCellLine, List<int> pPossibleValue)
        {
            foreach (Cell cell in pCellDirection)
            {
                if (cell != m_Cell && !pCellLine.Contains(cell))
                {
                    foreach (int i in pPossibleValue)
                    {
                        cell.updatedPossiblesValues = new List<int>(cell.updatedPossiblesValues);

                        if (cell.updatedPossiblesValues.Contains(i))
                        {
                            cell.updatedPossiblesValues.Remove(i);
                            m_PossiblesChanged = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of integers, which is the list of values whch only exist in the pCellLine variable
        /// </summary>
        /// <param name="pCellLine">The List of potential cells</param>
        /// <returns>List of integers which could possibly make up the pointing pair/triple</returns>
        private List<int> GetPossibleValuesForLine(List<Cell> pCellLine)
        {
            List<int> possibleValues = new List<int>(m_Cell.possibleValuesWithout0);

            foreach (Cell cell in m_Cell.cellSubGridWithoutStartAndAlgo)
            {
                //Checks to ensure the clel being checked isnt the start cell or a cell in the potential cell list
                if (cell != m_Cell && !pCellLine.Contains(cell))
                {
                    //Removes the possible values of the other cells from the possible values list
                    foreach (int i in cell.possibleValuesWithout0)
                    {
                        possibleValues.Remove(i);
                    }
                }
            }

            return possibleValues;
        }
        #endregion
    }
}