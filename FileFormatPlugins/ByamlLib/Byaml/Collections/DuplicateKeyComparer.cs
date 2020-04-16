using System;
using System.Collections.Generic;

namespace ByamlExt.Byaml
{
    internal class DuplicateKeyComparer<TKey> : IComparer<TKey>
        where TKey : IComparable
    {
        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);
            return result == 0 ? 1 : result; // Handle equality as being greater.
        }
    }
}
