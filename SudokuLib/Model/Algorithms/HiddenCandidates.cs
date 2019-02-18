using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    public class HiddenCandidates : Algorithm
    {
        private bool m_PossiblesChanged = false;

        public HiddenCandidates() : base("") { }

        public override bool? Search(Cell pCell)
        {
            m_Cell = pCell;

            SearchLine(m_Cell.cellRowWithoutStartAndAlgo);
            SearchLine(m_Cell.cellColumnWithoutStartAndAlgo);
            SearchLine(m_Cell.cellSubGridWithoutStartAndAlgo);

            if (m_PossiblesChanged)
            {
                return null;
            }

            return false;
        }

        private void SearchLine(List<Cell> pLineToSearch)
        {
            List<Cell> foundCells = new List<Cell>();
            List<int> foundCellsValues = new List<int>();

            List<List<Cell>> cellCombinations = Combinations.GenerateCombinations(pLineToSearch, m_Cell, 2, pLineToSearch.Count <= m_Cell.puzzleSize / 2 ? pLineToSearch.Count : m_Cell.puzzleSize / 2);

            foreach (List<Cell> currentFoundCells in cellCombinations)
            {
                List<int> foundCellsUnion = new List<int>();
                foreach (Cell cell in currentFoundCells)
                {
                    foundCellsUnion = foundCellsUnion.Union(cell.possibleValuesWithout0).ToList();
                }

                foreach (Cell cell in pLineToSearch)
                {
                    if (!currentFoundCells.Contains(cell))
                    {
                        foreach (int i in cell.possibleValuesWithout0)
                        {
                            foundCellsUnion.Remove(i);
                        }
                    }
                }

                if (foundCellsUnion.Count == currentFoundCells.Count && currentFoundCells.Count < pLineToSearch.Count)
                {
                    foundCells = currentFoundCells;
                    foundCellsValues = foundCellsUnion;
                    break;
                }
            }

            if (foundCells.Count > 1 && m_Cell.possibleValuesWithout0.Count > foundCells.Count)
            {
                char[] symbols = new char[foundCellsValues.Count];
                for (int i = 0; i < foundCellsValues.Count; i++)
                {
                    symbols[i] = Symbol.CURRENTSYMBOLS[foundCellsValues[i]];
                }

                m_Row = m_Cell.rowNumber;
                m_Column = m_Cell.columnNumber;

                m_AlgorithmMessage = String.Format("Used to eliminate possible values {0} from nearby cells", String.Join(", ", symbols));

                m_AlgorithmName = String.Format("Hidden {0}'s", foundCells.Count);

                foreach (Cell cell in foundCells)
                {
                    List<int> temp = new List<int>(cell.updatedPossiblesValues);

                    foreach (int i in cell.updatedPossiblesValues)
                    {
                        if (!foundCellsValues.Contains(i) && i != 0)
                        {
                            temp.Remove(i);
                        }
                    }

                    cell.updatedPossiblesValues = new List<int>(temp);
                }

                m_PossiblesChanged = true;
            }
        }
    }
}
