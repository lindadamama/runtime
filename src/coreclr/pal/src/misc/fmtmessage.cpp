// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*++



Module Name:

    fmtmessage.c

Abstract:

    Implementation of FormatMessage function.

Revision History:



--*/

#include "pal/palinternal.h"
#include "pal/dbgmsg.h"
#include "pal/module.h"

#include "errorstrings.h"

#include <stdarg.h>
#include <dlfcn.h>
#include <errno.h>
#include <wctype.h>

SET_DEFAULT_DEBUG_CHANNEL(MISC);

/* Defines */

#define MAX_ERROR_STRING_LENGTH 32

/*++
Function:

    FMTMSG_GetMessageString

Returns the message as a wide string.
--*/
static LPWSTR FMTMSG_GetMessageString( DWORD dwErrCode )
{
    TRACE("Entered FMTMSG_GetMessageString\n");

    LPCWSTR lpErrorString = GetPalErrorString(dwErrCode);
    int allocChars;

    if (lpErrorString != NULL)
    {
        allocChars = PAL_wcslen(lpErrorString) + 1;
    }
    else
    {
        allocChars = MAX_ERROR_STRING_LENGTH + 1;
    }

    LPWSTR lpRetVal = (LPWSTR)malloc(allocChars * sizeof(WCHAR));

    if (lpRetVal)
    {
        if (lpErrorString != NULL)
        {
            PAL_wcscpy(lpRetVal, lpErrorString);
        }
        else
        {
            char errorString[sizeof("Error 4294967295")];
            int cnt = sprintf_s(errorString, sizeof(errorString), "Error %u", dwErrCode);
            cnt++; // +1 for null terminator

            // Widening characters is okay here because they are the
            // same in both char and WCHAR.
            for (int i = 0; i < cnt; ++i)
                lpRetVal[i] = (WCHAR)errorString[i];
        }
    }
    else
    {
        ERROR("Unable to allocate memory.\n");
    }

    return lpRetVal;
}

/*++

Function :

    FMTMSG__watoi

    Converts a wide string repersentation of an integer number
    into a integer number.

    Returns a integer number, or 0 on failure. 0 is not a valid number
    for FormatMessage inserts.

--*/
static INT FMTMSG__watoi( LPWSTR str )
{
    CONST UINT MAX_NUMBER_LENGTH = 3;
    CHAR buf[ MAX_NUMBER_LENGTH ];
    INT nRetVal = 0;

    nRetVal = WideCharToMultiByte( CP_ACP, 0, str, -1, buf,
                                   MAX_NUMBER_LENGTH, NULL, 0 );

    if ( nRetVal != 0 )
    {
        return atoi( buf );
    }
    else
    {
        ERROR( "Unable to convert the string to a number.\n" );
        return 0;
    }
}

/* Adds the character to the working string. */
#define _ADD_TO_STRING( c ) \
{\
   TRACE( "Adding %c to the string.\n", (CHAR)c );\
   *lpWorkingString = c;\
    lpWorkingString++;\
    nCount++;\
}

/* Grows the buffer. */
#define _GROW_BUFFER() \
{\
    if ( bIsLocalAlloced ) \
    { \
        LPWSTR lpTemp = NULL; \
        UINT NumOfBytes = 0; \
        nSize *= 2; \
        NumOfBytes = nSize * sizeof( WCHAR ); \
        lpTemp = static_cast<WCHAR *>( malloc( NumOfBytes ) ); \
        TRACE( "Growing the buffer.\n" );\
        \
        if ( !lpTemp ) \
        { \
            ERROR( "Out of buffer\n" ); \
            SetLastError( ERROR_NOT_ENOUGH_MEMORY ); \
            nCount = 0; \
            lpWorkingString = NULL; \
            goto exit; \
        } \
        \
        *lpWorkingString = '\0';\
        PAL_wcscpy( lpTemp, lpReturnString );\
        free( lpReturnString ); \
        lpWorkingString = lpReturnString = lpTemp; \
        lpWorkingString += nCount; \
    } \
    else \
    { \
        WARN( "Out of buffer.\n" ); \
        SetLastError( ERROR_INSUFFICIENT_BUFFER ); \
        nCount = 0; \
        lpWorkingString = NULL; \
        goto exit; \
    } \
}
/* Adds a character to the working string.  This is a safer version
of _ADD_TO_STRING, as we will resize the buffer if necessary. */
#define _CHECKED_ADD_TO_STRING( c ) \
{\
    if ( nCount+1 == nSize ) \
    {\
        _GROW_BUFFER();\
    } \
    _ADD_TO_STRING( c );\
}

/*++
Function:
  FormatMessageW

See MSDN doc.
--*/
DWORD
PALAPI
FormatMessageW(
           IN DWORD dwFlags,
           IN LPCVOID lpSource,
           IN DWORD dwMessageId,
           IN DWORD dwLanguageId,
           OUT LPWSTR lpBuffer,
           IN DWORD nSize,
           IN va_list *Arguments)
{
    BOOL bIgnoreInserts = FALSE;
    BOOL bIsVaList = TRUE;
    BOOL bIsLocalAlloced = FALSE;
    LPWSTR lpSourceString = NULL;
    UINT nCount = 0;
    LPWSTR lpReturnString = NULL;
    LPWSTR lpWorkingString = NULL;

    PERF_ENTRY(FormatMessageW);
    ENTRY( "FormatMessageW(dwFlags=%#x, lpSource=%p, dwMessageId=%#x, "
           "dwLanguageId=%#x, lpBuffer=%p, nSize=%u, va_list=%p)\n",
           dwFlags, lpSource, dwMessageId, dwLanguageId, lpBuffer, nSize,
           Arguments);

    /* Sanity checks. */
    if ( dwFlags & FORMAT_MESSAGE_FROM_STRING && !lpSource )
    {
        /* This behavior is different then in Windows.
           Windows would just crash.*/
        ERROR( "lpSource cannot be NULL.\n" );
        SetLastError( ERROR_INVALID_PARAMETER );
        goto exit;
    }

    if ( !(dwFlags & FORMAT_MESSAGE_ALLOCATE_BUFFER ) && !lpBuffer )
    {
        /* This behavior is different then in Windows.
           Windows would just crash.*/
        ERROR( "lpBuffer cannot be NULL, if "
               " FORMAT_MESSAGE_ALLOCATE_BUFFER is not specified.\n" );
        SetLastError( ERROR_INVALID_PARAMETER );
        goto exit;
    }

    if ( ( dwFlags & FORMAT_MESSAGE_FROM_STRING ) &&
         ( dwFlags & FORMAT_MESSAGE_FROM_SYSTEM ) )
    {
        ERROR( "These flags cannot co-exist. You can either "
               "specify FORMAT_MESSAGE_FROM_STRING, or "
               "FORMAT_MESSAGE_FROM_SYSTEM.\n" );
        SetLastError( ERROR_INVALID_PARAMETER );
        goto exit;
    }

    if ( !( dwFlags & FORMAT_MESSAGE_FROM_STRING ) &&
         ( dwLanguageId != 0) )
    {
        ERROR( "Invalid language identifier.\n" );
        SetLastError( ERROR_RESOURCE_LANG_NOT_FOUND );
        goto exit;
    }

    /* Parameter processing. */
    if ( dwFlags & FORMAT_MESSAGE_ALLOCATE_BUFFER )
    {
        TRACE( "Allocated %d TCHARs. Don't forget to call free to "
               "free the memory when done.\n", nSize );
        bIsLocalAlloced = TRUE;
    }

    if ( dwFlags & FORMAT_MESSAGE_IGNORE_INSERTS )
    {
        bIgnoreInserts = TRUE;
    }

    if ( dwFlags & FORMAT_MESSAGE_ARGUMENT_ARRAY )
    {
        if ( !Arguments && !bIgnoreInserts )
        {
            ERROR( "The va_list cannot be NULL.\n" );
            SetLastError( ERROR_INVALID_PARAMETER );
            goto exit;
        }
        else
        {
            bIsVaList = FALSE;
        }
    }

    if ( dwFlags & FORMAT_MESSAGE_FROM_STRING )
    {
        lpSourceString = (LPWSTR)lpSource;
    }
    else if ( dwFlags & FORMAT_MESSAGE_FROM_SYSTEM )
    {
        if ((dwMessageId & 0xFFFF0000) == 0x80070000)
        {
            // This message has been produced by HRESULT_FROM_WIN32.  Undo its work.
            dwMessageId &= 0xFFFF;
        }

        lpWorkingString = lpReturnString =
            FMTMSG_GetMessageString( dwMessageId );

        if ( !lpWorkingString )
        {
            ERROR( "Unable to find the message %d.\n", dwMessageId );
            SetLastError( ERROR_INTERNAL_ERROR );
            nCount = 0;
            goto exit;
        }

        nCount = PAL_wcslen( lpWorkingString );

        if ( !bIsLocalAlloced && nCount > nSize )
        {
            ERROR( "Insufficient buffer.\n" );
            SetLastError( ERROR_INSUFFICIENT_BUFFER );
            lpWorkingString = NULL;
            nCount = 0;
            goto exit;
        }
        if ( !lpWorkingString )
        {
            ERROR( "Invalid error identifier.\n" );
            SetLastError( ERROR_INVALID_ADDRESS );
        }
        goto exit;
    }
    else
    {
        ERROR( "Unknown flag.\n" );
        SetLastError( ERROR_INVALID_PARAMETER );
        goto exit;
    }

    if ( nSize == 0 && bIsLocalAlloced )
    {
        nSize = 1;
    }

    lpWorkingString = static_cast<WCHAR *>(
        malloc( nSize * sizeof( WCHAR ) ) );
    if ( !lpWorkingString )
    {
        ERROR( "Unable to allocate memory for the working string.\n" );
        SetLastError( ERROR_INSUFFICIENT_BUFFER );
        goto exit;
    }


    /* Process the string. */
    lpReturnString = lpWorkingString;
    while ( *lpSourceString )
    {
        if ( *lpSourceString == '%' && !bIgnoreInserts )
        {
            lpSourceString++;
            /* Escape sequences. */
            if ( *lpSourceString == '0' )
            {
                /* Terminates a message without a newline character. */
                *lpWorkingString = '\0';
                goto exit;
            }
            else if ( iswdigit( *lpSourceString ) )
            {
                /* Get the insert number. */
                WCHAR Number[] = { '\0', '\0', '\0' };
                SIZE_T Index = 0;

                Number[ 0 ] = *lpSourceString;
                lpSourceString++;

                if ( iswdigit( *lpSourceString ) )
                {
                    Number[ 1 ] = *lpSourceString;
                    lpSourceString++;
                    if ( iswdigit( *lpSourceString ) )
                    {
                        ERROR( "Invalid insert identifier.\n" );
                        SetLastError( ERROR_INVALID_PARAMETER );
                        lpWorkingString = NULL;
                        nCount = 0;
                        goto exit;
                    }
                }
                Index = FMTMSG__watoi( Number );
                if ( Index == 0 )
                {
                    ERROR( "Invalid insert identifier.\n" );
                    SetLastError( ERROR_INVALID_PARAMETER );
                    lpWorkingString = NULL;
                    nCount = 0;
                    goto exit;
                }
                if ( *lpSourceString == '!' )
                {
                    ERROR( "Embedded printf formatting ('!<printf format>!') is unsupported\n" );
                    SetLastError( ERROR_INVALID_PARAMETER );
                    lpWorkingString = NULL;
                    nCount = 0;
                    goto exit;
                }

                LPWSTR lpInsert = NULL;
                if ( !bIsVaList )
                {
                    lpInsert = ((LPWSTR*)Arguments)[Index - 1];
                }
                else
                {
                    va_list TheArgs;
                    va_copy(TheArgs, *Arguments);
                    UINT i = 0;
                    for ( ; i < Index; i++ )
                    {
                        lpInsert = va_arg( TheArgs, LPWSTR );
                    }
                }

                while ( *lpInsert )
                {
                    _CHECKED_ADD_TO_STRING( *lpInsert );
                    lpInsert++;
                }
            }
            /* Format specifiers. */
            else if ( *lpSourceString == '%' )
            {
                _CHECKED_ADD_TO_STRING( '%' );
                lpSourceString++;
            }
            else if ( *lpSourceString == 'n' )
            {
                /* Hard line break. */
                _CHECKED_ADD_TO_STRING( '\n' );
                lpSourceString++;
            }
            else if ( *lpSourceString == '.' )
            {
                _CHECKED_ADD_TO_STRING( '.' );
                lpSourceString++;
            }
            else if ( *lpSourceString == '!' )
            {
                _CHECKED_ADD_TO_STRING( '!' );
                lpSourceString++;
            }
            else if ( !*lpSourceString )
            {
                ERROR( "Invalid parameter.\n" );
                SetLastError( ERROR_INVALID_PARAMETER );
                lpWorkingString = NULL;
                nCount = 0;
                goto exit;
            }
            else /* Append the character. */
            {
                _CHECKED_ADD_TO_STRING( *lpSourceString );
                lpSourceString++;
            }
        }/* END if ( *lpSourceString == '%' ) */
        else
        {
            /* In Windows if FormatMessage is called with ignore inserts,
            then FormatMessage strips %1!s! down to %1, since string is the
            default. */
            if ( bIgnoreInserts && *lpSourceString == '!' &&
                 *( lpSourceString + 1 ) == 's' )
            {
                LPWSTR lpLastBang = PAL_wcschr( lpSourceString + 1, '!' );

                if ( lpLastBang && ( 2 == lpLastBang - lpSourceString ) )
                {
                    lpSourceString = lpLastBang + 1;
                }
                else
                {
                    ERROR( "Mal-formed string\n" );
                    SetLastError( ERROR_INVALID_PARAMETER );
                    lpWorkingString = NULL;
                    nCount = 0;
                    goto exit;
                }
            }
            else
            {
                /* Append to the string. */
                _CHECKED_ADD_TO_STRING( *lpSourceString );
                lpSourceString++;
            }
        }
    }

    /* Terminate the message. */
    _CHECKED_ADD_TO_STRING( '\0' );
    /* NULL does not count. */
    nCount--;

exit: /* Function clean-up and exit. */
    if ( lpWorkingString )
    {
        if ( bIsLocalAlloced )
        {
            TRACE( "Assigning the buffer to the pointer.\n" );
            // when FORMAT_MESSAGE_ALLOCATE_BUFFER is specified, nSize
            // does not specify the size of lpBuffer, rather it specifies
            // the minimum size of the string
            // as such we have to blindly assume that lpBuffer has enough space to
            // store PVOID
            // might cause a prefast warning, but there is no good way to suppress it yet
            _ASSERTE(dwFlags & FORMAT_MESSAGE_ALLOCATE_BUFFER);
            *((LPVOID*)lpBuffer) = (LPVOID)lpReturnString;
        }
        else /* Only delete lpReturnString if the caller has their own buffer.*/
        {
            TRACE( "Copying the string into the buffer.\n" );
            PAL_wcsncpy( lpBuffer, lpReturnString, nCount + 1 );
            free( lpReturnString );
        }
    }
    else /* Error, something occurred. */
    {
        if ( lpReturnString )
        {
            free( lpReturnString );
        }
    }
    LOGEXIT( "FormatMessageW returns %d.\n", nCount );
    PERF_EXIT(FormatMessageW);
    return nCount;
}
