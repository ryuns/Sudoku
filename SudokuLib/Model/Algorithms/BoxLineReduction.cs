using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    class BoxLineReduction : Algorithm
    {
        private bool m_PossiblesChanged = false;

        public BoxLineReduction() : base("Box Line Reduction") { }

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

            if(cellsColumn.Count > 0)
            {
                List<int> possibleValues = GetPossibleValuesForLine(cellsColumn, m_Cell.cellColumnWithoutStartAndAlgo);

                if (cellsColumn.Count > 0 && possibleValues.Count > 0)
                {
                    ValueExistsInLine(cellsColumn, possibleValues);

                    char[] symbols = new char[possibleValues.Count];
                    for (int i = 0; i < possibleValues.Count; i++)
                    {
                        symbols[i] = Symbol.CURRENTSYMBOLS[possibleValues[i]];
                    }

                    m_Row = m_Cell.rowNumber;
                    m_Column = m_Cell.columnNumber;

                    m_AlgorithmMessage = String.Format("Used to eliminate possible values {0} from SubGrid", String.Join(", ", symbols));
                }
            }

            if(cellsRow.Count > 0)
            {
                List<int> possibleValues = GetPossibleValuesForLine(cellsRow, m_Cell.cellRowWithoutStartAndAlgo);

                if (cellsRow.Count > 0 && possibleValues.Count > 0)
                {
                    ValueExistsInLine(cellsRow, possibleValues);

                    char[] symbols = new char[possibleValues.Count];
                    for (int i = 0; i < possibleValues.Count; i++)
                    {
                        symbols[i] = Symbol.CURRENTSYMBOLS[possibleValues[i]];
                    }

                    m_Row = m_Cell.rowNumber;
                    m_Column = m_Cell.columnNumber;

                    m_AlgorithmMessage = String.Format("Used to eliminate possible values {0} from SubGrid", String.Join(", ", symbols));
                }
            }

            if (m_PossiblesChanged)
            {
                return null;
            }

            return false;
        }

        private void ValueExistsInLine(List<Cell> pCellLine, List<int> pPossibleValue)
        {
            foreach (Cell cell in m_Cell.cellSubGridWithoutStartAndAlgo)
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

        private List<int> GetPossibleValuesForLine(List<Cell> pCellLine, List<Cell> pCellRowColumn)
        {
            List<int> possibleValues = new List<int>(m_Cell.possibleValuesWithout0);

            foreach (Cell cell in pCellRowColumn)
            {
                if (cell != m_Cell && !pCellLine.Contains(cell))
                {
                    foreach (int i in cell.possibleValuesWithout0)
                    {
                        possibleValues.Remove(i);
                    }
                }
            }

            return possibleValues;
        }
    }
}
