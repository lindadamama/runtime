// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


#ifndef PAL_LIMITED_CONTEXT_INCLUDED
#define PAL_LIMITED_CONTEXT_INCLUDED

#include "rhassert.h"

#ifndef DECLSPEC_ALIGN
#ifdef _MSC_VER
#define DECLSPEC_ALIGN(x)   __declspec(align(x))
#else
#define DECLSPEC_ALIGN(x)   __attribute__((aligned(x)))
#endif
#endif // DECLSPEC_ALIGN

#ifdef HOST_AMD64
#define AMD64_ALIGN_16 DECLSPEC_ALIGN(16)
#else // HOST_AMD64
#define AMD64_ALIGN_16
#endif // HOST_AMD64

struct AMD64_ALIGN_16 Fp128 {
    uint64_t Low;
    int64_t High;
};


struct PAL_LIMITED_CONTEXT
{
    // Includes special registers, callee saved registers and general purpose registers used to return values from functions (not floating point return registers)
#ifdef TARGET_ARM
    uintptr_t  R0;
    uintptr_t  R4;
    uintptr_t  R5;
    uintptr_t  R6;
    uintptr_t  R7;
    uintptr_t  R8;
    uintptr_t  R9;
    uintptr_t  R10;
    uintptr_t  R11;

    uintptr_t  IP;
    uintptr_t  SP;
    uintptr_t  LR;

    uint64_t      D[16-8]; // D8 .. D15 registers (D16 .. D31 are volatile according to the ABI spec)

    uintptr_t GetIp() const { return IP; }
    uintptr_t GetSp() const { return SP; }
    uintptr_t GetFp() const { return R7; }
    uintptr_t GetLr() const { return LR; }
    void SetIp(uintptr_t ip) { IP = ip; }
    void SetSp(uintptr_t sp) { SP = sp; }

#elif defined(TARGET_ARM64)
    uintptr_t  FP;
    uintptr_t  LR;

    uintptr_t  X0;
    uintptr_t  X1;
    uintptr_t  X19;
    uintptr_t  X20;
    uintptr_t  X21;
    uintptr_t  X22;
    uintptr_t  X23;
    uintptr_t  X24;
    uintptr_t  X25;
    uintptr_t  X26;
    uintptr_t  X27;
    uintptr_t  X28;

    uintptr_t  SP;
    uintptr_t  IP;

    uint64_t      D[16 - 8];  // Only the bottom 64-bit value of the V registers V8..V15 needs to be preserved
                            // (V0-V7 and V16-V31 are not preserved according to the ABI spec).


    uintptr_t GetIp() const { return IP; }
    uintptr_t GetSp() const { return SP; }
    uintptr_t GetFp() const { return FP; }
    uintptr_t GetLr() const { return LR; }
    void SetIp(uintptr_t ip) { IP = ip; }
    void SetSp(uintptr_t sp) { SP = sp; }

#elif defined(TARGET_LOONGARCH64)
    uintptr_t  FP;
    uintptr_t  RA;

    uintptr_t  R4;
    uintptr_t  R5;
    uintptr_t  R23;
    uintptr_t  R24;
    uintptr_t  R25;
    uintptr_t  R26;
    uintptr_t  R27;
    uintptr_t  R28;
    uintptr_t  R29;
    uintptr_t  R30;
    uintptr_t  R31;

    uintptr_t  SP;
    uintptr_t  IP;

    uint64_t      F[32 - 24]; // Only the F registers F24..F31 need to be preserved
                              // (F0-F23 are not preserved according to the ABI spec).

    uintptr_t GetIp() const { return IP; }
    uintptr_t GetSp() const { return SP; }
    uintptr_t GetFp() const { return FP; }
    uintptr_t GetRa() const { return RA; }
    void SetIp(uintptr_t ip) { IP = ip; }
    void SetSp(uintptr_t sp) { SP = sp; }

#elif defined(TARGET_RISCV64)

    uintptr_t  FP;
    uintptr_t  RA;

    uintptr_t  A0;
    uintptr_t  A1;
    uintptr_t  S1;
    uintptr_t  S2;
    uintptr_t  S3;
    uintptr_t  S4;
    uintptr_t  S5;
    uintptr_t  S6;
    uintptr_t  S7;
    uintptr_t  S8;
    uintptr_t  S9;
    uintptr_t  S10;
    uintptr_t  S11;

    uintptr_t  SP;
    uintptr_t  IP;

    uint64_t  F[12];

    uintptr_t GetIp() const { return IP; }
    uintptr_t GetSp() const { return SP; }
    uintptr_t GetFp() const { return FP; }
    uintptr_t GetRa() const { return RA; }
    void SetIp(uintptr_t ip) { IP = ip; }
    void SetSp(uintptr_t sp) { SP = sp; }

#elif defined(UNIX_AMD64_ABI)
    // Param regs: rdi, rsi, rdx, rcx, r8, r9, scratch: rax, rdx (both return val), preserved: rbp, rbx, r12-r15
    uintptr_t  IP;
    uintptr_t  Rsp;
    uintptr_t  Rbp;
    uintptr_t  Rax;
    uintptr_t  Rbx;
    uintptr_t  Rdx;
    uintptr_t  R12;
    uintptr_t  R13;
    uintptr_t  R14;
    uintptr_t  R15;

    uintptr_t GetIp() const { return IP; }
    uintptr_t GetSp() const { return Rsp; }
    void SetIp(uintptr_t ip) { IP = ip; }
    void SetSp(uintptr_t sp) { Rsp = sp; }
    uintptr_t GetFp() const { return Rbp; }
#elif defined(TARGET_X86) || defined(TARGET_AMD64)
    uintptr_t  IP;
    uintptr_t  Rsp;
    uintptr_t  Rbp;
    uintptr_t  Rdi;
    uintptr_t  Rsi;
    uintptr_t  Rax;
    uintptr_t  Rbx;
#ifdef TARGET_AMD64
    uintptr_t  R12;
    uintptr_t  R13;
    uintptr_t  R14;
    uintptr_t  R15;
#if defined(TARGET_WINDOWS)
    uintptr_t  SSP;
#else
    uintptr_t  __explicit_padding__;
#endif // TARGET_WINDOWS
    Fp128       Xmm6;
    Fp128       Xmm7;
    Fp128       Xmm8;
    Fp128       Xmm9;
    Fp128       Xmm10;
    Fp128       Xmm11;
    Fp128       Xmm12;
    Fp128       Xmm13;
    Fp128       Xmm14;
    Fp128       Xmm15;
#endif // TARGET_AMD64

    uintptr_t GetIp() const { return IP; }
    uintptr_t GetSp() const { return Rsp; }
    uintptr_t GetFp() const { return Rbp; }
    void SetIp(uintptr_t ip) { IP = ip; }
    void SetSp(uintptr_t sp) { Rsp = sp; }
#else // TARGET_ARM
    uintptr_t  IP;

    uintptr_t GetIp() const { PORTABILITY_ASSERT("GetIp");  return 0; }
    uintptr_t GetSp() const { PORTABILITY_ASSERT("GetSp"); return 0; }
    uintptr_t GetFp() const { PORTABILITY_ASSERT("GetFp"); return 0; }
    void SetIp(uintptr_t ip) { PORTABILITY_ASSERT("SetIp"); }
    void SetSp(uintptr_t sp) { PORTABILITY_ASSERT("GetSp"); }
#endif // TARGET_ARM
};

#endif // PAL_LIMITED_CONTEXT_INCLUDED
