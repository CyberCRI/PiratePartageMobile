using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility {

		// Based on http://stackoverflow.com/a/36634935/209505

        /// Heap's algorithm to find all pmermutations. Non recursive, more efficient.
        /// <param name="items">Items to permute in each possible ways</param>
        /// <param name="funcExecuteAndTellIfShouldStop"></param>
        /// <returns>Return true if cancelled</returns> 
        public static IEnumerable<List<T>> Permutate<T>(List<T> items)
        {
            int countOfItem = items.Count;

            if (countOfItem <= 1)
            {
                yield return items; 
				yield break;
            }

            var indexes = new int[countOfItem];
            for (int i = 0; i < countOfItem; i++)
            {
                indexes[i] = 0;
            }

			yield return items;

            for (int i = 1; i < countOfItem;)
            {
                if (indexes[i] < i)
                { // On the web there is an implementation with a multiplication which should be less efficient.
                    if ((i & 1) == 1) // if (i % 2 == 1)  ... more efficient ??? At least the same.
                    {
                        Swap(items, i, indexes[i]);
                    }
                    else
                    {
                        Swap(items, i, 0);
                    }

					yield return items;

                    indexes[i]++;
                    i = 1;
                }
                else
                {
                    indexes[i++] = 0;
                }
            }
        }

		public static void Swap<T>(List<T> list, int a, int b)
        {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        // The Fisher–Yates shuffle
        public static void Shuffle<T>(List<T> list)
        {
            for(int i = 0; i < list.Count - 2; i++)
            {
                int j = Random.Range(i, list.Count);
                Swap(list, i, j);
            }
        }
}
