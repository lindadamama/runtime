// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Xunit;

// Generated by Fuzzlyn v1.7 on 2024-07-25 11:29:20
// Run on X64 Windows
// Seed: 17281006834984098297-vectort,vector128,vector256,x86aes,x86avx,x86avx2,x86bmi1,x86bmi1x64,x86bmi2,x86bmi2x64,x86fma,x86lzcnt,x86lzcntx64,x86pclmulqdq,x86popcnt,x86popcntx64,x86sse,x86ssex64,x86sse2,x86sse2x64,x86sse3,x86sse41,x86sse41x64,x86sse42,x86sse42x64,x86ssse3,x86x86base
// Reduced from 159.4 KiB to 0.4 KiB in 00:00:57
// Hits JIT assert in Release:
// Assertion failed 'isContainable || supportsRegOptional' in 'Program:Main(Fuzzlyn.ExecutionServer.IRuntime)' during 'Generate code' (IL size 47; hash 0xade6b36b; FullOpts)
//
//     File: C:\dev\dotnet\runtime4\src\coreclr\jit\hwintrinsiccodegenxarch.cpp Line: 61
//

public class Runtime_105467
{
    public static Vector128<float>[] s_6 = new []{ Vector128<float>.Zero };

    [Fact]
    public static void TestEntryPoint()
    {
        if (Sse41.IsSupported)
        {
            var vr3 = s_6[0];
            var vr4 = Vector128.Create<float>(0);
            s_6[0] = Sse41.Insert(vr3, vr4, 254);
        }
    }
}
