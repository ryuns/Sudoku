using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model
{
    public class GridSize
    {
        private int m_Rows;
        private int m_Columns;

        #region Constructors
        /// <summary>
        /// Constructs a grid size with the row count and column count
        /// </summary>
        /// <param name="pRows">The number of rows</param>
        /// <param name="pColumns">The number fo columns</param>
        public GridSize(int pRows, int pColumns)
        {
            m_Rows = pRows;
            m_Columns = pColumns;
        }
        #endregion

        #region Accessors
        public int rowCount
        {
            get { return m_Rows; }
            set
            {
                if (value > 1)
                {
                    m_Rows = value;
                }
                else
                {
                    throw new Exception("Grid size error - size must be greater than 1");
                }
            }
        }

        public int columnCount
        {
            get { return m_Columns; }
            set
            {
                if(value > 1)
                {
                    m_Columns = value;
                }
                else
                {
                    throw new Exception("Grid size error - size must be greater than 1");
                }
            }
        }
        #endregion

        #region Misc
        public override string ToString()
        {
            return string.Format("{0} x {1}", m_Rows, m_Columns);
        }

        private static bool TryParse(string pGridColumnRow, out int pSize)
        {
            if(int.TryParse(pGridColumnRow, out pSize) && pSize > 1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses a grid size string to a GridSize object
        /// </summary>
        /// <param name="pGridSize">The grid size</param>
        /// <returns>A GridSize object</returns>
        public static GridSize Parse(string pGridSize)
        {
            //Splits the string on the x
            string[] gridSize = pGridSize.ToLower().Split('x');
            int rowNumber;
            int columnNumber;

            //Tries to parse both numbers and returns the GridSize else throws an exception
            if (TryParse(gridSize[0], out rowNumber) && TryParse(gridSize[1], out columnNumber))
            {
                return new GridSize(rowNumber, columnNumber);
            }
            else
            {
                throw new Exception("Grid size error - Gridsize must be greater than 1x1");
            }
        }
        #endregion
    }
}
