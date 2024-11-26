// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Collections
{
    internal class KeyedStack<T, R>
        where T : notnull
    {
        private readonly Dictionary<T, Stack<R>> _dict = new Dictionary<T, Stack<R>>();

        public void Push(T key, R value)
        {
            Stack<R>? store;
            if (!_dict.TryGetValue(key, out store))
            {
                store = new Stack<R>();
                _dict.Add(key, store);
            }

            store.Push(value);
        }

        public bool TryPop(T key, [MaybeNullWhen(returnValue: false)] out R value)
        {
            Stack<R>? store;
            if (_dict.TryGetValue(key, out store) && store.Count > 0)
            {
                value = store.Pop();
                return true;
            }

            value = default(R)!;
            return false;
        }
    }
}
