﻿using System;
using System.Collections.Generic;

namespace Solitaire.Game.Extensions
{

    public static class IListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int count = list.Count;
            int last = count - 1;

            for (int i = 0; i < last; i++)
            {
                int r = UnityEngine.Random.Range(i, count);
                T tmp = list[i];
                list[i] = list[r];
                list[r] = tmp;
            }
        }
        public static T PopAt<T>(this IList<T> list, int index)
        {
            if (index >= list.Count)
                return default(T);

            T value = list[index];
            list.RemoveAt(index);

            return value;
        }
        public static T Pop<T>(this IList<T> list)
        {
            return list.PopAt(list.Count - 1);
        }
        public static List<T> Copy<T>(this IList<T> list)
        {
            var other = new List<T>();

            foreach (T item in list)
                other.Add(item);

            return other;
        }
        public static T Last<T>(this IList<T> list)
        {
            if (list.Count == 0)
                return default(T);

            return list[list.Count - 1];
        }
    }
}