using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    public abstract class Algorithm
    {
        protected Cell m_Cell;
        protected string m_AlgorithmName;
        protected string m_AlgorithmMessage;
        protected int m_Row;
        protected int m_Column;

        protected Algorithm(string pAlgorithmName)
        {
            m_AlgorithmName = pAlgorithmName;
        }

        public abstract bool? Search(Cell pCell);

        public override string ToString()
        {
            return String.Format("Cell at position {0}, {1} {2} using {3} algorithm", m_Row + 1, m_Column + 1, m_AlgorithmMessage, m_AlgorithmName);
        }
    }
}
