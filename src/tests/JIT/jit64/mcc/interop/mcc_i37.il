// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


.assembly extern System.Runtime.Extensions { auto }
.assembly extern xunit.core {}
.assembly extern mscorlib { auto }
.assembly extern TestLibrary {}

.assembly 'mcc_i37' {}


.namespace MCCTest
{
  .class public MyClass
  {
    .method assembly static pinvokeimpl("native_i3s" as "#1" stdcall)
        valuetype MCCTest.VType3 Sum(unsigned int64, valuetype MCCTest.VType3, float64,	valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3,
                                    int32, valuetype MCCTest.VType3, unsigned int16, valuetype MCCTest.VType3, unsigned int32, valuetype MCCTest.VType3,
                                    float32, valuetype MCCTest.VType3, int64, valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3,
                                    float64, valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3, int16, valuetype MCCTest.VType3) cil managed preservesig {
    }

    .method private valuetype MCCTest.VType3 GetSum()
    {
      .maxstack  64
      .locals init	(
        [0] valuetype MCCTest.VType3 v1,
        [1] valuetype MCCTest.VType3 v2,
        [2] valuetype MCCTest.VType3 v3,
        [3] valuetype MCCTest.VType3 v4,
        [4] valuetype MCCTest.VType3 v5,
        [5] valuetype MCCTest.VType3 v6,
        [6] valuetype MCCTest.VType3 v7,
        [7] valuetype MCCTest.VType3 v8,
        [8] valuetype MCCTest.VType3 v9,
        [9] valuetype MCCTest.VType3 v10,
        [10] valuetype MCCTest.VType3 v11,
        [11] valuetype MCCTest.VType3 v12
      )

      // Initialize v1 thru v12
      ldloca.s   v1
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v2
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v3
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v4
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v5
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v6
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v7
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v8
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v9
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v10
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v11
      call       instance void MCCTest.VType3::Init()
      ldloca.s   v12
      call       instance void MCCTest.VType3::Init()

      ldc.i8     1
      ldloc.s    v1
      ldc.r8     2
      ldloc.s    v2
      ldc.r4     3
      ldloc.s    v3
      ldc.i4     4
      ldloc.s    v4
      ldc.i8     5
      conv.u2
      ldloc.s    v5
      ldc.i4     6
      ldloc.s    v6
      ldc.r4     7
      ldloc.s    v7
      ldc.i8     8
      ldloc.s    v8
      ldc.r4     9
      ldloc.s    v9
      ldc.r8     10
      ldloc.s    v10
      ldc.r4     11
      ldloc.s    v11
      ldc.i8     12
      conv.i2
      ldloc.s    v12
      call       valuetype MCCTest.VType3 MCCTest.MyClass::GetSum2(	unsigned int64,	valuetype MCCTest.VType3,
                                                                float64,		valuetype MCCTest.VType3,
                                                                float32,		valuetype MCCTest.VType3,
                                                                int32,			valuetype MCCTest.VType3,
                                                                unsigned int16,	valuetype MCCTest.VType3,
                                                                unsigned int32,	valuetype MCCTest.VType3,
                                                                float32,		valuetype MCCTest.VType3,
                                                                int64,			valuetype MCCTest.VType3,
                                                                float32,		valuetype MCCTest.VType3,
                                                                float64,		valuetype MCCTest.VType3,
                                                                float32,		valuetype MCCTest.VType3,
                                                                int16,			valuetype MCCTest.VType3)
        ret
    }

    .method private static valuetype MCCTest.VType3 GetSum2(unsigned int64, valuetype MCCTest.VType3, float64,	valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3,
                                    int32, valuetype MCCTest.VType3, unsigned int16, valuetype MCCTest.VType3, unsigned int32, valuetype MCCTest.VType3,
                                    float32, valuetype MCCTest.VType3, int64, valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3,
                                    float64, valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3, int16, valuetype MCCTest.VType3)
    {
      jmp   valuetype MCCTest.VType3 MCCTest.MyClass::Sum(unsigned int64, valuetype MCCTest.VType3, float64,	valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3,
                                    int32, valuetype MCCTest.VType3, unsigned int16, valuetype MCCTest.VType3, unsigned int32, valuetype MCCTest.VType3,
                                    float32, valuetype MCCTest.VType3, int64, valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3,
                                    float64, valuetype MCCTest.VType3, float32, valuetype MCCTest.VType3, int16, valuetype MCCTest.VType3)
    }

    .method public specialname rtspecialname instance void  .ctor()
    {
      .maxstack  1
      ldarg.0
      call       instance void [mscorlib]System.Object::.ctor()
      ret
    } // end of method MyClass::.ctor

    .method public static int32 Main()
    {
      .custom instance void [xunit.core]Xunit.FactAttribute::.ctor() = (
          01 00 00 00
      )
      .entrypoint
      .maxstack  1
      call       bool [TestLibrary]TestLibrary.PlatformDetection::get_IsWindows()
      brtrue.s   DoTest
      ldc.i4     100
      ret

    DoTest:
      call       int32 MCCTest.MyClass::Test()
      ret
    }

    .method public static int32 Test()
    {
      .maxstack  64
      .locals init	(
        [0] class MCCTest.MyClass me,
        [1] valuetype MCCTest.VType3 res,
        [2] int32 rc
      )

      newobj     instance void MCCTest.MyClass::.ctor()
      stloc.s    me

      ldloc.s    me
      call   instance valuetype MCCTest.VType3 MCCTest.MyClass::GetSum()
      stloc.s    res

      // Check Result
      ldloc.s    res
      ldc.i4     12
      call       int32 MCCTest.Common::CheckResult(valuetype MCCTest.VType3, int32)
      stloc.s    rc

      ldloc.s    rc
      ret
    } // end of method MyClass::Test

  } // end of class MyClass

} // end of namespace MCCTest
