﻿// Copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Implements the well-known quick sort algorithm.
    /// </summary>
    public static class QuickSort
    {
        /// <summary>
        /// Sorts a list in-place, given the <paramref name="comparison"/>
        /// method.
        /// </summary>
        /// <typeparam name="T">Type of element in the list.</typeparam>
        /// <param name="list">The list to sort.</param>
        /// <param name="comparison">The method used to compare list elements.</param>
        public static void Sort<T>(IList<T> list, Comparison<T> comparison)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (comparison == null) throw new ArgumentNullException("comparison");

            if (list.Count < 2)
            {
                return;
            }

            int middle = (list.Count - 1) / 2;
            int partitionIndex = partition(list, comparison, 0, list.Count - 1, middle);
            sortRange(list, comparison, 0, partitionIndex);
            sortRange(list, comparison, partitionIndex + 1, list.Count - 1);
        }

        private static int partition<T>(IList<T> list, Comparison<T> comparison,
            int minIndex, int maxIndex, int pivotIndex)
        {
            T pivotItem = list[pivotIndex];
            swap(list, pivotIndex, maxIndex);

            int minCompareIndex = minIndex - 1;

            for (int i = minIndex; i <= maxIndex - 1; i++)
            {
                if (comparison(list[i], pivotItem) < 0)
                {
                    minCompareIndex++;
                    swap(list, minCompareIndex, i);
                }
            }

            minCompareIndex++;  
            swap(list, maxIndex, minCompareIndex);

            return minCompareIndex;
        }

        private static void sortRange<T>(IList<T> list, Comparison<T> comparison, int minIndex, int maxIndex)
        {
            if (minIndex >= maxIndex)
            {
                return;
            }

            int middle = (maxIndex - minIndex) / 2 + minIndex;
            int partitionIndex = partition(list, comparison, minIndex, maxIndex, middle);
            sortRange(list, comparison, minIndex, partitionIndex);
            sortRange(list, comparison, partitionIndex + 1, maxIndex);
        }

        private static void swap<T>(IList<T> list, int minIndex, int maxIndex)
        {
            T item = list[minIndex];
            list[minIndex] = list[maxIndex];
            list[maxIndex] = item;
        }
    }
}
