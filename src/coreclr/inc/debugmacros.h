// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//*****************************************************************************
// DebugMacros.h
//
// Wrappers for Debugging purposes.
//
//*****************************************************************************

#ifndef __DebugMacros_h__
#define __DebugMacros_h__

#include "stacktrace.h"
#include "debugmacrosext.h"
#include "palclr.h"
#include <minipal/utils.h>

#undef VERIFY

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

#if defined(_DEBUG)

class SString;
bool GetStackTraceAtContext(SString & s, struct _CONTEXT * pContext);

bool _DbgBreakCheck(LPCSTR szFile, int iLine, LPCSTR szExpr, BOOL fConstrained = FALSE);

extern VOID ANALYZER_NORETURN DbgAssertDialog(const char *szFile, int iLine, const char *szExpr);

#define PRE_ASSERTE         /* if you need to change modes before doing asserts override */
#define POST_ASSERTE        /* put it back */

#if !defined(_ASSERTE_MSG)
  #define _ASSERTE_MSG(expr, msg)                                           \
        do {                                                                \
             if (!(expr)) {                                                 \
                PRE_ASSERTE                                                 \
                DbgAssertDialog(__FILE__, __LINE__, msg);                   \
                POST_ASSERTE                                                \
             }                                                              \
        } while (0)
#endif // _ASSERTE_MSG

#if !defined(_ASSERTE)
  #define _ASSERTE(expr) _ASSERTE_MSG(expr, #expr)
#endif  // !_ASSERTE


#define VERIFY(stmt) _ASSERTE((stmt))

#define _ASSERTE_ALL_BUILDS(expr) _ASSERTE((expr))

#else // !_DEBUG

#if !defined(_ASSERTE)
    #define _ASSERTE(expr) ((void)0)
#endif
#if !defined(_ASSERTE_MSG)
    #define _ASSERTE_MSG(expr, msg) ((void)0)
#endif
#define VERIFY(stmt) (void)(stmt)

// At this point, EEPOLICY_HANDLE_FATAL_ERROR may or may not be defined. It will be defined
// if we are building the VM folder, but outside VM, its not necessarily defined.
//
// Thus, if EEPOLICY_HANDLE_FATAL_ERROR is not defined, we will call into __FreeBuildAssertFail,
// but if it is defined, we will use it.
//
// Failing here implies an error in the runtime - hence we use COR_E_EXECUTIONENGINE.
#ifdef EEPOLICY_HANDLE_FATAL_ERROR
#define _ASSERTE_ALL_BUILDS(expr) if (!(expr)) EEPOLICY_HANDLE_FATAL_ERROR(COR_E_EXECUTIONENGINE);
#else // !EEPOLICY_HANDLE_FATAL_ERROR
void DECLSPEC_NORETURN __FreeBuildAssertFail(const char *szFile, int iLine, const char *szExpr);
#define _ASSERTE_ALL_BUILDS(expr) if (!(expr)) __FreeBuildAssertFail(__FILE__, __LINE__, #expr);
#endif // EEPOLICY_HANDLE_FATAL_ERROR

#endif


#define ASSERT_AND_CHECK(x) {       \
    BOOL bResult = x;               \
    if (!bResult)                   \
    {                               \
        _ASSERTE(x);                \
        return FALSE;               \
    }                               \
}


#ifdef _DEBUG_IMPL

#define _ASSERTE_IMPL(expr) _ASSERTE((expr))

#if     defined(_M_IX86)
#if defined(_MSC_VER)
#define _DbgBreak() __asm { int 3 }
#elif defined(__GNUC__)
#define _DbgBreak() __asm__ ("int $3");
#else
#error Unknown compiler
#endif
#else
#define _DbgBreak() DebugBreak()
#endif

extern VOID DebBreakHr(HRESULT hr);

#ifndef IfFailGoto
#define IfFailGoto(EXPR, LABEL) \
do { hr = (EXPR); if(FAILED(hr)) { DebBreakHr(hr); goto LABEL; } } while (0)
#endif

#ifndef IfFailRet
#define IfFailRet(EXPR) \
do { hr = (EXPR); if(FAILED(hr)) { DebBreakHr(hr); return (hr); } } while (0)
#endif

#ifndef IfFailWin32Ret
#define IfFailWin32Ret(EXPR) \
do { hr = (EXPR); if(hr != ERROR_SUCCESS) { hr = HRESULT_FROM_WIN32(hr); DebBreakHr(hr); return hr;} } while (0)
#endif

#ifndef IfFailWin32Goto
#define IfFailWin32Goto(EXPR, LABEL) \
do { hr = (EXPR); if(hr != ERROR_SUCCESS) { hr = HRESULT_FROM_WIN32(hr); DebBreakHr(hr); goto LABEL; } } while (0)
#endif

#ifndef IfFailGo
#define IfFailGo(EXPR) IfFailGoto(EXPR, ErrExit)
#endif

#ifndef IfFailWin32Go
#define IfFailWin32Go(EXPR) IfFailWin32Goto(EXPR, ErrExit)
#endif

#else // _DEBUG_IMPL

#define _DbgBreak() {}

#define _ASSERTE_IMPL(expr)

#define IfFailGoto(EXPR, LABEL) \
do { hr = (EXPR); if(FAILED(hr)) { goto LABEL; } } while (0)

#define IfFailRet(EXPR) \
do { hr = (EXPR); if(FAILED(hr)) { return (hr); } } while (0)

#define IfFailWin32Ret(EXPR) \
do { hr = (EXPR); if(hr != ERROR_SUCCESS) { hr = HRESULT_FROM_WIN32(hr); return hr;} } while (0)

#define IfFailWin32Goto(EXPR, LABEL) \
do { hr = (EXPR); if(hr != ERROR_SUCCESS) { hr = HRESULT_FROM_WIN32(hr); goto LABEL; } } while (0)

#define IfFailGo(EXPR) IfFailGoto(EXPR, ErrExit)

#define IfFailWin32Go(EXPR) IfFailWin32Goto(EXPR, ErrExit)

#endif // _DEBUG_IMPL


#define IfNullGoto(EXPR, LABEL) \
    do { if ((EXPR) == NULL) { OutOfMemory(); IfFailGoto(E_OUTOFMEMORY, LABEL); } } while (false)

#ifndef IfNullRet
#define IfNullRet(EXPR) \
    do { if ((EXPR) == NULL) { OutOfMemory(); return E_OUTOFMEMORY; } } while (false)
#endif //!IfNullRet

#define IfNullGo(EXPR) IfNullGoto(EXPR, ErrExit)

#ifdef __cplusplus
}

#endif // __cplusplus


#undef assert
#define assert _ASSERTE
#undef _ASSERT
#define _ASSERT _ASSERTE

#endif // __DebugMacros_h__
