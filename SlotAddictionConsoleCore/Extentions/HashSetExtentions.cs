using System;
using System.Collections.Generic;

namespace SlotAddictionCore.Extentions
{
    public static class HashSetExtentions
    {
        /// <summary>
        /// 指定したコレクションの要素をSystem.Collections.Generic.HashSet<T>の末尾に追加します
        /// </summary>
        /// <typeparam name="T">ハッシュセット内の要素の型</typeparam>
        /// <param name="self"></param>
        /// <param name="collection"></param>
        public static void AddRange<T>(this HashSet<T> self, IEnumerable<T> collection)
        {
            if (self == null)
            {
                throw new NullReferenceException($"nullに対して{nameof(AddRange)}を呼び出すことはできません。");
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "パラメーターがnullです。");
            }

            foreach (T item in collection)
            {
                self.Add(item);
            }
        }
    }
}
