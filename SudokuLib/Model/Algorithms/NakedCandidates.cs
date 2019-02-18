using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model.Algorithms
{
    class NakedCandidates : Algorithm
    {
        private bool m_PossiblesChanged = false;

        public NakedCandidates() : base("") { }

        public override bool? Search(Cell pCell)
        {
            m_Cell = pCell;
            List<int> cellsPossibles = m_Cell.possibleValuesWithout0;

            if (cellsPossibles.Count <= m_Cell.puzzleSize / 2)
            {
                SearchLine(m_Cell.cellRowWithoutStartAndAlgo, cellsPossibles);
                SearchLine(m_Cell.cellColumnWithoutStartAndAlgo, cellsPossibles);
                SearchLine(m_Cell.cellSubGridWithoutStartAndAlgo, cellsPossibles);

                if (m_PossiblesChanged)
                {
                    return null;
                }
            }

            return false;
        }

        private void SearchLine(List<Cell> pLineToSearch, List<int> pPossibles)
        {
            List<Cell> foundCells = new List<Cell>();

            for (int i = pPossibles.Count; i <= m_Cell.puzzleSize / 2; i++)
            {
                foundCells.Clear();
                foundCells.Add(m_Cell);

                List<int> union = new List<int>(pPossibles);

                for (int j = 0; j < pLineToSearch.Count; j++)
                {
                    Cell currentCell = pLineToSearch[j];

                    if (!currentCell.Equals(m_Cell))
                    {
                        List<int> cellPossibles = currentCell.possibleValuesWithout0;

                        if (cellPossibles.Count <= i)
                        {
                            if (union.Union(cellPossibles).Count() == i)
                            {
                                union = union.Union(cellPossibles).ToList();
                                foundCells.Add(currentCell);
                            }
                        }
                    }
                }

                if (foundCells.Count == i && foundCells.Count < pLineToSearch.Count)
                {
                    pPossibles = union;
                    break;
                }
                else
                {
                    foundCells.Clear();
                }
            }

            if (foundCells.Count > 1)
            {
                char[] symbols = new char[pPossibles.Count];
                for (int i = 0; i < pPossibles.Count; i++)
                {
                    symbols[i] = Symbol.CURRENTSYMBOLS[pPossibles[i]];
                }

                m_Row = m_Cell.rowNumber;
                m_Column = m_Cell.columnNumber;

                m_AlgorithmMessage = String.Format("Used to eliminate possible values {0} from nearby cells", String.Join(", ", symbols));

                m_AlgorithmName = String.Format("Naked {0}'s", foundCells.Count);

                foreach (Cell cell in pLineToSearch)
                {
                    if (!foundCells.Contains(cell))
                    {
                        cell.updatedPossiblesValues = new List<int>(cell.updatedPossiblesValues);

                        foreach (int j in pPossibles)
                        {
                            if (cell.updatedPossiblesValues.Contains(j))
                            {
                                m_PossiblesChanged = true;
                                cell.updatedPossiblesValues.Remove(j);
                            }
                        }
                    }
                }
            }
        }
    }
}
