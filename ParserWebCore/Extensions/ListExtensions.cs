﻿#region

using System.Collections.Generic;

#endregion

namespace ParserWebCore.Extensions
{
    public static class ListExtensions
    {
        public static List<T> AddRangeAndReturnList<T>(this List<T> l, List<T> addList)
        {
            l.AddRange(addList);
            return l;
        }
    }
}