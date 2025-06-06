// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (source is IReadOnlyCollection<TSource> gc)
            {
                return gc.Count != 0;
            }

            if (!IsSizeOptimized && source is Iterator<TSource> iterator)
            {
                int count = iterator.GetCount(onlyIfCheap: true);
                if (count >= 0)
                {
                    return count != 0;
                }

                iterator.TryGetFirst(out bool found);
                return found;
            }

            if (source is ICollection ngc)
            {
                return ngc.Count != 0;
            }

            using IEnumerator<TSource> e = source.GetEnumerator();
            return e.MoveNext();
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (predicate is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
            }

            if (source.TryGetSpan(out ReadOnlySpan<TSource> span))
            {
                foreach (TSource element in span)
                {
                    if (predicate(element))
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (TSource element in source)
                {
                    if (predicate(element))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (predicate is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
            }

            if (source.TryGetSpan(out ReadOnlySpan<TSource> span))
            {
                foreach (TSource element in span)
                {
                    if (!predicate(element))
                    {
                        return false;
                    }
                }
            }
            else
            {
                foreach (TSource element in source)
                {
                    if (!predicate(element))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
