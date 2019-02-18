using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLib.Model
{
    public static class Combinations
    {
        #region Private methods
        /// <summary>
        /// Generates a mapping for the current combination
        /// </summary>
        /// <param name="pCombinationsListLength">The length of the combination required</param>
        /// <param name="pCombinationValuesLength">The number of different values to be used in the combination</param>
        /// <returns>A mapping for the combination</returns>
        private static IEnumerable<int[]> CurrentCombinationMapping(int pCombinationsListLength, int pCombinationValuesLength)
        {
            int[] currentCombinationMapping = new int[pCombinationsListLength];

            Stack<int> stack = new Stack<int>(pCombinationsListLength);
            stack.Push(0);

            while (stack.Count > 0)
            {
                int index = stack.Count - 1;

                //Pops the last used value off the stack to indicate the positon in the combinations
                int currentCombinationIndex = stack.Pop();

                //last used value is less than the total number of values to combine
                while (currentCombinationIndex < pCombinationValuesLength)
                {
                    currentCombinationMapping[index++] = currentCombinationIndex++;
                    stack.Push(currentCombinationIndex);

                    if (index != pCombinationsListLength)
                    {
                        continue;
                    }

                    yield return (int[])currentCombinationMapping.Clone();                                                        
                    break;
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets a list of lists of combinations by their length
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCombinationValues">The values which the combinations must be made from</param>
        /// <param name="pObjectToContain">The object that must be contained in every combination</param>
        /// <param name="pMinCombinationListLength">The minimum length of the combination</param>
        /// <param name="pMaxCombinationListLength">The maximum length of the combination</param>
        /// <returns>All of the combinations which could be made form the given objects</returns>
        public static List<List<T>> GenerateCombinations<T>(List<T> pCombinationValues, T pObjectToContain, int pMinCombinationListLength, int pMaxCombinationListLength)
        {
            List<List<T>> combinations = new List<List<T>>();

            for (int i = pMinCombinationListLength; i <= pMaxCombinationListLength; i++)
            {
                //Sets the current combination length
                T[] currentCombination = new T[i];

                //Forach of the combinations for x number of values with a combination length of i
                foreach (int[] j in CurrentCombinationMapping(i, pCombinationValues.Count))
                {
                    //Add the combinations at the mapping to the current array
                    for (int k = 0; k < i; k++)
                    {
                        currentCombination[k] = pCombinationValues[j[k]];
                    }

                    //Check if the combination contains the required object and add the combination to the return list
                    if (currentCombination.Contains(pObjectToContain))
                    {
                        combinations.Add(((T[]) currentCombination.Clone()).ToList());
                    }
                }
            }

            return combinations;
        }
        #endregion
    }
}
