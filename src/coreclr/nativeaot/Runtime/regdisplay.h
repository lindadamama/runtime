// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#ifndef __regdisplay_h__
#define __regdisplay_h__

#if defined(TARGET_X86) || defined(TARGET_AMD64)

#include "PalLimitedContext.h" // Fp128

struct REGDISPLAY
{
    PTR_uintptr_t pRax;
    PTR_uintptr_t pRcx;
    PTR_uintptr_t pRdx;
    PTR_uintptr_t pRbx;
    //           pEsp;
    PTR_uintptr_t pRbp;
    PTR_uintptr_t pRsi;
    PTR_uintptr_t pRdi;
#ifdef TARGET_AMD64
    PTR_uintptr_t pR8;
    PTR_uintptr_t pR9;
    PTR_uintptr_t pR10;
    PTR_uintptr_t pR11;
    PTR_uintptr_t pR12;
    PTR_uintptr_t pR13;
    PTR_uintptr_t pR14;
    PTR_uintptr_t pR15;
#endif // TARGET_AMD64

    uintptr_t   SP;
    PCODE       IP;

#if defined(TARGET_AMD64) && defined(TARGET_WINDOWS)
    uintptr_t   SSP;          // keep track of SSP for EH unwind
                              // we do not adjust the original, so only need the value
#endif  // TARGET_AMD64 && TARGET_WINDOWS

#if defined(TARGET_AMD64) && !defined(UNIX_AMD64_ABI)
    Fp128          Xmm[16-6]; // preserved xmm6..xmm15 regs for EH stackwalk
                              // these need to be unwound during a stack walk
                              // for EH, but not adjusted, so we only need
                              // their values, not their addresses
#endif // TARGET_AMD64 && !UNIX_AMD64_ABI

    inline PCODE GetIP() { return IP; }
    inline uintptr_t GetSP() { return SP; }
    inline uintptr_t GetFP() { return *pRbp; }
    inline uintptr_t GetPP() { return *pRbx; }

    inline void SetIP(PCODE IP) { this->IP = IP; }
    inline void SetSP(uintptr_t SP) { this->SP = SP; }

#ifdef TARGET_X86
    TADDR PCTAddr;
    // SP for use by catch funclet when resuming execution
    uintptr_t ResumeSP;

    inline unsigned long *GetEaxLocation() { return (unsigned long *)pRax; }
    inline unsigned long *GetEcxLocation() { return (unsigned long *)pRcx; }
    inline unsigned long *GetEdxLocation() { return (unsigned long *)pRdx; }
    inline unsigned long *GetEbpLocation() { return (unsigned long *)pRbp; }
    inline unsigned long *GetEbxLocation() { return (unsigned long *)pRbx; }
    inline unsigned long *GetEsiLocation() { return (unsigned long *)pRsi; }
    inline unsigned long *GetEdiLocation() { return (unsigned long *)pRdi; }

    inline void SetEaxLocation(unsigned long *loc) { pRax = (PTR_uintptr_t)loc; }
    inline void SetEcxLocation(unsigned long *loc) { pRcx = (PTR_uintptr_t)loc; }
    inline void SetEdxLocation(unsigned long *loc) { pRdx = (PTR_uintptr_t)loc; }
    inline void SetEbxLocation(unsigned long *loc) { pRbx = (PTR_uintptr_t)loc; }
    inline void SetEsiLocation(unsigned long *loc) { pRsi = (PTR_uintptr_t)loc; }
    inline void SetEdiLocation(unsigned long *loc) { pRdi = (PTR_uintptr_t)loc; }
    inline void SetEbpLocation(unsigned long *loc) { pRbp = (PTR_uintptr_t)loc; }
#endif

};

#ifdef TARGET_X86
inline TADDR GetRegdisplayFP(REGDISPLAY *display)
{
    return (TADDR)*display->GetEbpLocation();
}

inline void SetRegdisplayPCTAddr(REGDISPLAY *display, TADDR addr)
{
    display->PCTAddr = addr;
    display->SetIP(*PTR_PCODE(addr));
}
#endif

#elif defined(TARGET_ARM)

struct REGDISPLAY
{
    PTR_uintptr_t pR0;
    PTR_uintptr_t pR1;
    PTR_uintptr_t pR2;
    PTR_uintptr_t pR3;
    PTR_uintptr_t pR4;
    PTR_uintptr_t pR5;
    PTR_uintptr_t pR6;
    PTR_uintptr_t pR7;
    PTR_uintptr_t pR8;
    PTR_uintptr_t pR9;
    PTR_uintptr_t pR10;
    PTR_uintptr_t pR11;
    PTR_uintptr_t pR12;
    PTR_uintptr_t pLR;

    uintptr_t   SP;
    PCODE        IP;

    uint64_t       D[16-8]; // preserved D registers D8..D15 (note that D16-D31 are not preserved according to the ABI spec)
                          // these need to be unwound during a stack walk
                          // for EH, but not adjusted, so we only need
                          // their values, not their addresses

    inline PCODE GetIP() { return IP; }
    inline uintptr_t GetSP() { return SP; }
    inline uintptr_t GetFP() { return *pR11; }
    inline PTR_uintptr_t GetReturnAddressRegisterLocation() { return pLR; }

    inline void SetIP(PCODE IP) { this->IP = IP; }
    inline void SetSP(uintptr_t SP) { this->SP = SP; }
};

#elif defined(TARGET_ARM64)

struct REGDISPLAY
{
    PTR_uintptr_t pX0;
    PTR_uintptr_t pX1;
    PTR_uintptr_t pX2;
    PTR_uintptr_t pX3;
    PTR_uintptr_t pX4;
    PTR_uintptr_t pX5;
    PTR_uintptr_t pX6;
    PTR_uintptr_t pX7;
    PTR_uintptr_t pX8;
    PTR_uintptr_t pX9;
    PTR_uintptr_t pX10;
    PTR_uintptr_t pX11;
    PTR_uintptr_t pX12;
    PTR_uintptr_t pX13;
    PTR_uintptr_t pX14;
    PTR_uintptr_t pX15;
    PTR_uintptr_t pX16;
    PTR_uintptr_t pX17;
    PTR_uintptr_t pX18;
    PTR_uintptr_t pX19;
    PTR_uintptr_t pX20;
    PTR_uintptr_t pX21;
    PTR_uintptr_t pX22;
    PTR_uintptr_t pX23;
    PTR_uintptr_t pX24;
    PTR_uintptr_t pX25;
    PTR_uintptr_t pX26;
    PTR_uintptr_t pX27;
    PTR_uintptr_t pX28;
    PTR_uintptr_t pFP; // X29
    PTR_uintptr_t pLR; // X30

    uintptr_t   SP;
    PCODE        IP;

    uint64_t       D[16-8]; // Only the bottom 64-bit value of the V registers V8..V15 needs to be preserved
                          // (V0-V7 and V16-V31 are not preserved according to the ABI spec).
                          // These need to be unwound during a stack walk
                          // for EH, but not adjusted, so we only need
                          // their values, not their addresses

    inline PCODE GetIP() { return IP; }
    inline uintptr_t GetSP() { return SP; }
    inline uintptr_t GetFP() { return *pFP; }
    inline PTR_uintptr_t GetReturnAddressRegisterLocation() { return pLR; }

    inline void SetIP(PCODE IP) { this->IP = IP; }
    inline void SetSP(uintptr_t SP) { this->SP = SP; }
};

#elif defined(TARGET_LOONGARCH64)

struct REGDISPLAY
{
    PTR_uintptr_t pR0;
    PTR_uintptr_t pRA;
    PTR_uintptr_t pR2;

    uintptr_t   SP;

    PTR_uintptr_t pR4;
    PTR_uintptr_t pR5;
    PTR_uintptr_t pR6;
    PTR_uintptr_t pR7;
    PTR_uintptr_t pR8;
    PTR_uintptr_t pR9;
    PTR_uintptr_t pR10;
    PTR_uintptr_t pR11;
    PTR_uintptr_t pR12;
    PTR_uintptr_t pR13;
    PTR_uintptr_t pR14;
    PTR_uintptr_t pR15;
    PTR_uintptr_t pR16;
    PTR_uintptr_t pR17;
    PTR_uintptr_t pR18;
    PTR_uintptr_t pR19;
    PTR_uintptr_t pR20;
    PTR_uintptr_t pR21;
    PTR_uintptr_t pFP;
    PTR_uintptr_t pR23;
    PTR_uintptr_t pR24;
    PTR_uintptr_t pR25;
    PTR_uintptr_t pR26;
    PTR_uintptr_t pR27;
    PTR_uintptr_t pR28;
    PTR_uintptr_t pR29;
    PTR_uintptr_t pR30;
    PTR_uintptr_t pR31;

    PCODE        IP;

    uint64_t       F[32-24]; // Only the F registers F24..F31 needs to be preserved
                             // (F0-F23 are not preserved according to the ABI spec).
                             // These need to be unwound during a stack walk
                             // for EH, but not adjusted, so we only need
                             // their values, not their addresses

    inline PCODE GetIP() { return IP; }
    inline uintptr_t GetSP() { return SP; }
    inline uintptr_t GetFP() { return *pFP; }
    inline PTR_uintptr_t GetReturnAddressRegisterLocation() { return pRA; }

    inline void SetIP(PCODE IP) { this->IP = IP; }
    inline void SetSP(uintptr_t SP) { this->SP = SP; }
};

#elif defined(TARGET_RISCV64)

struct REGDISPLAY
{
    PTR_uintptr_t pR0;
    PTR_uintptr_t pRA;

    uintptr_t   SP;

    PTR_uintptr_t pGP;
    PTR_uintptr_t pTP;
    PTR_uintptr_t pT0;
    PTR_uintptr_t pT1;
    PTR_uintptr_t pT2;
    PTR_uintptr_t pFP;
    PTR_uintptr_t pS1;
    PTR_uintptr_t pA0;
    PTR_uintptr_t pA1;
    PTR_uintptr_t pA2;
    PTR_uintptr_t pA3;
    PTR_uintptr_t pA4;
    PTR_uintptr_t pA5;
    PTR_uintptr_t pA6;
    PTR_uintptr_t pA7;
    PTR_uintptr_t pS2;
    PTR_uintptr_t pS3;
    PTR_uintptr_t pS4;
    PTR_uintptr_t pS5;
    PTR_uintptr_t pS6;
    PTR_uintptr_t pS7;
    PTR_uintptr_t pS8;
    PTR_uintptr_t pS9;
    PTR_uintptr_t pS10;
    PTR_uintptr_t pS11;
    PTR_uintptr_t pT3;
    PTR_uintptr_t pT4;
    PTR_uintptr_t pT5;
    PTR_uintptr_t pT6;

    PCODE        IP;

    uint64_t F[32];  // Expanded to cover all F registers

    inline PCODE GetIP() { return IP; }
    inline uintptr_t GetSP() { return SP; }
    inline uintptr_t GetFP() { return *pFP; }
    inline PTR_uintptr_t GetReturnAddressRegisterLocation() { return pRA; }

    inline void SetIP(PCODE IP) { this->IP = IP; }
    inline void SetSP(uintptr_t SP) { this->SP = SP; }
};

#elif defined(TARGET_WASM)

struct REGDISPLAY
{
    // TODO: WebAssembly doesn't really have registers. What exactly do we need here?

    uintptr_t   SP;
    PCODE        IP;

    inline PCODE GetIP() { return NULL; }
    inline uintptr_t GetSP() { return 0; }
    inline uintptr_t GetFP() { return 0; }

    inline void SetIP(PCODE IP) { }
    inline void SetSP(uintptr_t SP) { }
};
#endif

typedef REGDISPLAY * PREGDISPLAY;

#endif //__regdisplay_h__
