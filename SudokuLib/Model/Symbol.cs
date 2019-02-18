using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model
{
    public static class Symbol
    {
        private static char[] m_CURRENTSYMBOLS;
        private static char[] m_FULLCHARARRAY = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        #region Symbol setters
        /// <summary>
        /// Sets the current symbol set - called when using a manual entry puzzle
        /// </summary>
        /// <param name="pSize">The size of the puzzle</param>
        public static void SetSymbols(int pPuzzleSize)
        {
            //Creates new array of the same size as the puzzle + 1 to include empty value
            m_CURRENTSYMBOLS = new char[(pPuzzleSize + 1)];
            m_CURRENTSYMBOLS[0] = ' ';

            //Adds a number of symbols the same length as the puzzle size from the full character set
            for (int i = 0; i < pPuzzleSize; i++)
            {
                m_CURRENTSYMBOLS[i + 1] = m_FULLCHARARRAY[i];
            }
        }

        /// <summary>
        /// Sets the current symbol set - called when loading a symbol set from a file
        /// </summary>
        /// <param name="pSymbols">The symbols in the symbol set</param>
        public static void SetSymbols(char[] pSymbols)
        {
            //Symbol set is not allowed to contain 0, 0 is reserved for empty symbol
            if (pSymbols.Contains('0') || pSymbols.Contains(' '))
            {
                throw new Exception("Symbol set error - Symbol set cannot contain 0 or a space");
            }

            //Creates new array of the same size as the puzzle + 1 to include empty value
            m_CURRENTSYMBOLS = new char[pSymbols.Length + 1];
            m_CURRENTSYMBOLS[0] = ' ';

            for (int i = 0; i < pSymbols.Length; i++)
            {
                //Checks to make sure the symbol doesnt exist in the symbol set, if it does it is a duplicate and throws exception
                if (!m_CURRENTSYMBOLS.Contains(pSymbols[i]))
                {
                    m_CURRENTSYMBOLS[i + 1] = pSymbols[i];
                }
                else
                {
                    throw new Exception("Symbol set error - All symbols in the symbol set must be unique");
                }
            }
        }
        #endregion

        #region Convertors
        /// <summary>
        /// Converts a list of ints to a list of chars based on their position in the current symbols array
        /// </summary>
        /// <param name="pIntList">the list of ints to be mapped</param>
        /// <returns>list of chars from inputted ints</returns>
        public static ObservableCollection<char> IntToCharList(List<int> pIntList)
        {
            ObservableCollection<char> possibles = new ObservableCollection<char>();

            //Gets the char based on the position specified by the inputter int
            for (int i = 0; i < pIntList.Count; i++)
            {
                possibles.Add(m_CURRENTSYMBOLS[pIntList[i]]);
            }
            return possibles;
        }

        /// <summary>
        /// Converts a list of symbols to a list of ints
        /// </summary>
        /// <param name="pCharList">List of chars to be mapped to ints</param>
        /// <returns>List of ints from the inputted chars</returns>
        public static List<int> CharToIntList(ObservableCollection<char> pCharList)
        {
            List<int> possibles = new List<int>();

            //Gets an int based on the chard position in the array
            for (int i = 0; i < pCharList.Count; i++)
            {
                possibles.Add(ConvertCharToInt(pCharList[i]));
            }
            return possibles;
        }

        /// <summary>
        /// Converts a char 
        /// </summary>
        /// <param name="pChar"></param>
        /// <returns></returns>
        public static int ConvertCharToInt(char pChar)
        {
            char charToCheck = pChar == '0' ? ' ' : pChar;
            for (int i = 0; i < m_CURRENTSYMBOLS.Length; i++)
            {
                //if the char exists in the array return its index
                if (charToCheck == m_CURRENTSYMBOLS[i])
                {
                    return i;
                }
            }

            //Else the char does not exist in the array
            throw new Exception("Symbol set error - Symbol does not exist in the symbol set");
        }
        #endregion

        #region Accessors
        public static List<int> currentSymbolsIntList
        {
            get
            {
                List<int> values = new List<int>();

                for (int i = 0; i < m_CURRENTSYMBOLS.Length; i++)
                {
                    values.Add(i);
                }

                return values;
            }
        }

        public static char[] CURRENTSYMBOLS
        {
            get { return m_CURRENTSYMBOLS; }
        }
        #endregion
    }
}
