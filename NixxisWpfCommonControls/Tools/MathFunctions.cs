using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixxis.Client.Controls
{
    public static class MathFunction
    {
        /// <summary>
        /// This function return the best dimensions to give a matrix to display x number of items in
        /// </summary>
        /// <param name="numberOfItems"></param>
        /// <returns></returns>
        public static int[] GetBestDimensions(int numberOfItems)
        {
            return GetBestDimensions(numberOfItems, false);
        }
        /// <summary>
        /// This function return the best dimensions to give a matrix to display x number of items in
        /// </summary>
        /// <param name="numberOfItems"></param>
        /// <returns></returns>
        public static int[] GetBestDimensions(int numberOfItems, bool includeNumberOfItems)
        {
            int[][] list = GetAllDimensions(numberOfItems);

            int[] result = new int[] { 1, numberOfItems };
            int[] correctResult = new int[] { 0, 0 }, bestResult = new int[] { 0, 0 };

            int stop = includeNumberOfItems ? 0 : 1;

            if (list.Length > stop)
            {
                for (int i = list.Length - 1; i >= stop; i--)
                {
                    int sum = (list[i][0] * list[i][1]);

                    if (sum == numberOfItems)
                    {
                        correctResult = list[i];
                        break;
                    }
                    else if (sum > numberOfItems && (bestResult[0] == 0 || sum < (bestResult[0] * bestResult[1])))
                    {
                        bestResult = list[i];
                    }
                }
            }

            if (correctResult[0] != 0)
                result = correctResult;
            else if (bestResult[0] != 0)
                result = bestResult;

            return result;
        }
        /// <summary>
        /// This function returns all possible dimensions to give a matrix to display x number of items
        /// </summary>
        /// <param name="numberOfItems"></param>
        /// <returns></returns>
        public static int[][] GetAllDimensions(int numberOfItems)
        {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, numberOfItems });

            if (numberOfItems > 1)
            {
                for (int i = 2; i <= numberOfItems; i++)
                {
                    int result = 0;
                    int j = 0;
                    for (j = i; j < numberOfItems; j++)
                    {
                        result = i * j;

                        if (result >= numberOfItems)
                        {
                            list.Add(new int[] { i, j });
                            break;
                        }
                    }

                    if (i == j)
                        break;
                }
            }

            return list.ToArray();
        }
    }
}
