// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Internal
{
    public static partial class Console
    {
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static unsafe void Write(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            fixed (byte* pBytes = bytes)
            {
                Interop.Sys.Log(pBytes, bytes.Length);
            }
        }

        public static partial class Error
        {
            [MethodImplAttribute(MethodImplOptions.NoInlining)]
            public static unsafe void Write(string s)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                fixed (byte* pBytes = bytes)
                {
                    Interop.Sys.LogError(pBytes, bytes.Length);
                }
            }
        }
    }
}
