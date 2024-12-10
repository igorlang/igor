using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor
{
    /// <summary>
    /// Utility extension methods for collections
    /// </summary>
    public static class CollectionHelper
    {
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var value in values)
            {
                action(value);
            }
        }

        public static IEnumerable<T> Yield<T>(this T value)
        {
            yield return value;
        }

        public static IEnumerable<T> YieldIfNotNull<T>(this T value) where T : class
        {
            if (value != null)
                yield return value;
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(item => item != null);
        }

        public static void AddIfNotNull<T>(this IList<T> list, T value) where T : class
        {
            if (value != null)
                list.Add(value);
        }

        public static T AssertNotNull<T>(this T value, string name) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(name);
            return value;
        }

        public static IDictionary<TKey, TValue> ZipDictionary<TKey, TValue>(this IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            return keys.Zip(values, Tuple.Create).ToDictionary(x => x.Item1, x => x.Item2);
        }

        public static IEnumerable<TResult> SelectWithIndex<T, TResult>(this IEnumerable<T> seq, Func<T, int, TResult> selector)
        {
            int i = 0;
            foreach (var item in seq)
            {
                yield return selector(item, i);
                i++;
            }
        }

        public static IEnumerable<List<T>> GroupAdjacentBy<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
        {
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    var list = new List<T> { e.Current };
                    var prev = e.Current;
                    while (e.MoveNext())
                    {
                        if (predicate(prev, e.Current))
                        {
                            list.Add(e.Current);
                        }
                        else
                        {
                            yield return list;
                            list = new List<T> { e.Current };
                        }
                        prev = e.Current;
                    }
                    yield return list;
                }
            }
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            return dict.TryGetValue(key, out var val) ? val : defaultValue;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> newValue)
        {
            if (!dict.TryGetValue(key, out var val))
            {
                val = newValue();
                dict[key] = val;
            }
            return val;
        }

        public static TItem GetOrAdd<TKey, TItem>(this IList<TItem> list, TKey key, Func<TItem, TKey> keyFun, Func<TItem> newItem) where TItem : class
        {
            var item = list.FirstOrDefault(it => Equals(keyFun(it), key));
            if (item == null)
            {
                item = newItem();
                list.Add(item);
            }
            return item;
        }
    }
}
