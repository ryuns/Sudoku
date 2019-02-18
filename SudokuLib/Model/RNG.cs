using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model
{
    class RNG
    {
        private static Random m_Random = new Random();

        public static Random RANDOM
        {
            get { return m_Random; }
        }
    }
}
