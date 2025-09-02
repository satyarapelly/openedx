// <copyright file="LinqExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class LinqExtensions
    {
        public static IEnumerable<TSource>[] Split<TSource>(this IEnumerable<TSource> source, int batchCount)
        {
            if (batchCount <= 0)
            {
                throw new ArgumentException("Value should be greater than zero", "batchCount");
            }

            if (source == null)
            {
                return new IEnumerable<TSource>[0];
            }

            List<IEnumerable<TSource>> result = new List<IEnumerable<TSource>>();

            int sourceCnt = source.Count();
            int batchSize = sourceCnt / batchCount;
            int additionalItemsCnt = sourceCnt % batchCount;

            int currentIdx = 0;
            int batchIdx = 0;
            while (currentIdx < sourceCnt)
            {
                int itemsToTake = batchIdx < additionalItemsCnt ? batchSize + 1 : batchSize;
                result.Add(source.Skip(currentIdx).Take(itemsToTake));

                currentIdx += itemsToTake;
                batchIdx++;
            }

            return result.ToArray();
        }
    }
}
