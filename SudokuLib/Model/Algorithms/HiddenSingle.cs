using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    public class HiddenSingle : Algorithm
    {
        public HiddenSingle() : base("Hidden Single") { }

        public override bool? Search(Cell pCell)
        {
            m_Cell = pCell;
            List<int> possibleValues = new List<int>(m_Cell.possibleValuesWithout0);

            if(SearchLine(m_Cell.cellColumnWithoutStartAndAlgo, ref possibleValues) || SearchLine(m_Cell.cellRowWithoutStartAndAlgo, ref possibleValues) || SearchLine(m_Cell.cellSubGridWithoutStartAndAlgo, ref possibleValues))
            {
                m_AlgorithmMessage = String.Format("value found to be {0}", Symbol.CURRENTSYMBOLS[possibleValues[0]].ToString());
                m_Row = m_Cell.rowNumber;
                m_Column = m_Cell.columnNumber;

                m_Cell.updatedPossiblesValues = new List<int>();
                m_Cell.updatedPossiblesValues.Add(0);
                m_Cell.updatedPossiblesValues.Add(possibleValues[0]);

                m_Cell.UpdateAllPossibles(possibleValues[0]);

                return null;
            }

            return false;
        }

        private bool SearchLine(List<Cell> pLineToSearch, ref List<int> pPossibles)
        {
            List<int> foundPossibles = new List<int>(pPossibles);

            foreach(Cell cell in pLineToSearch)
            {
                if (!cell.Equals(m_Cell) && foundPossibles.Count > 0)
                {
                    foundPossibles = foundPossibles.Except(cell.possibleValuesWithout0).ToList();
                }
            }

            if (foundPossibles.Count == 1)
            {
                pPossibles = foundPossibles;
                return true;
            }

            return false;
        }
    }
}
