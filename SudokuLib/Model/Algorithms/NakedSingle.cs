using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    public class NakedSingle : Algorithm
    {
        public NakedSingle() : base("Naked Single") { }

        public override bool? Search(Cell pCell)
        {
            m_Cell = pCell;
            List<int> possibleValues = new List<int>(m_Cell.possibleValuesWithout0);

            if (possibleValues.Count == 1)
            {
                m_AlgorithmMessage = String.Format("changed to value {0}", Symbol.CURRENTSYMBOLS[possibleValues[0]].ToString());
                m_Row = m_Cell.rowNumber;
                m_Column = m_Cell.columnNumber;

                m_Cell.content = possibleValues[0];

                m_Cell.updatedPossiblesValues = new List<int>(m_Cell.updatedPossiblesValues);
                m_Cell.UpdateAllPossibles(possibleValues[0]);
                m_Cell.algorithmEntered = true;

                return true;
            }

            return false;
        }
    }
}
