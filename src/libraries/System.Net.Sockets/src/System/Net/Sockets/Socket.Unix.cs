// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.Net.Sockets
{
    public partial class Socket
    {
        [SupportedOSPlatform("windows")]
        public Socket(SocketInformation socketInformation)
        {
            // This constructor works in conjunction with DuplicateAndClose, which is not supported on Unix.
            // See comments in DuplicateAndClose.
            throw new PlatformNotSupportedException(SR.net_sockets_duplicateandclose_notsupported);
        }

        [SupportedOSPlatform("windows")]
        public SocketInformation DuplicateAndClose(int targetProcessId)
        {
            // DuplicateAndClose is not supported on Unix, since passing file descriptors between processes
            // requires Unix Domain Sockets. The programming model is fundamentally different,
            // and incompatible with the design of SocketInformation-related methods.
            throw new PlatformNotSupportedException(SR.net_sockets_duplicateandclose_notsupported);
        }

        internal bool PreferInlineCompletions
        {
            get => _handle.PreferInlineCompletions;
            set => _handle.PreferInlineCompletions = value;
        }

        internal bool CanProceedWithMultiConnect => !_handle.ExposedHandleOrUntrackedConfiguration;

        partial void ValidateForMultiConnect()
        {
            // ValidateForMultiConnect is called before any {Begin}Connect{Async} call,
            // regardless of whether it's targeting an endpoint with multiple addresses.
            // If it is targeting such an endpoint, then any exposure of the socket's handle
            // or configuration of the socket we haven't tracked would prevent us from
            // replicating the socket's file descriptor appropriately.  Similarly, if it's
            // only targeting a single address, but it already experienced a failure in a
            // previous connect call, then this is logically part of a multi endpoint connect,
            // and the same logic applies.  Either way, in such a situation we throw.
            if (_handle.ExposedHandleOrUntrackedConfiguration && _handle.LastConnectFailed)
            {
                ThrowMultiConnectNotSupported();
            }

            // If the socket was already used for a failed connect attempt, replace it
            // with a fresh one, copying over all of the state we've tracked.
            ReplaceHandleIfNecessaryAfterFailedConnect();
            Debug.Assert(!_handle.LastConnectFailed);
        }

        private static void LoadSocketTypeFromHandle(
            SafeSocketHandle handle, out AddressFamily addressFamily, out SocketType socketType, out ProtocolType protocolType, out bool blocking, out bool isListening, out bool isSocket)
        {
            if (OperatingSystem.IsWasi())
            {
                // FIXME: Unify with unix after https://github.com/WebAssembly/wasi-libc/issues/537
                blocking = false;
                Interop.Error e = Interop.Sys.GetSocketType(handle, out addressFamily, out socketType, out protocolType, out isListening);
                if (e == Interop.Error.ENOTSOCK)
                {
                    throw new SocketException((int)SocketError.NotSocket);
                }
                handle.IsSocket = isSocket = true;
                return;
            }

            if (Interop.Sys.FStat(handle, out Interop.Sys.FileStatus stat) == -1)
            {
                throw new SocketException((int)SocketError.NotSocket);
            }
            isSocket = (stat.Mode & Interop.Sys.FileTypes.S_IFSOCK) == Interop.Sys.FileTypes.S_IFSOCK;

            handle.IsSocket = isSocket;

            if (isSocket)
            {
                // On Linux, GetSocketType will be able to query SO_DOMAIN, SO_TYPE, and SO_PROTOCOL to get the
                // address family, socket type, and protocol type, respectively.  On macOS, this will only succeed
                // in getting the socket type, and the others will be unknown.  Subsequently the Socket ctor
                // can use getsockname to retrieve the address family as part of trying to get the local end point.
                Interop.Error e = Interop.Sys.GetSocketType(handle, out addressFamily, out socketType, out protocolType, out isListening);
                Debug.Assert(e == Interop.Error.SUCCESS, e.ToString());
            }
            else
            {
                addressFamily = AddressFamily.Unknown;
                socketType = SocketType.Unknown;
                protocolType = ProtocolType.Unknown;
                isListening = false;
            }

            // Get whether the socket is in non-blocking mode.  On Unix, we automatically put the underlying
            // Socket into non-blocking mode whenever an async method is first invoked on the instance, but we
            // maintain a shadow bool that maintains the Socket.Blocking value set by the developer.  Because
            // we're querying the underlying socket here, and don't have access to the original Socket instance
            // (if there even was one... the Socket(SafeSocketHandle) ctor is likely being used because there
            // wasn't one, Socket.Blocking will end up reflecting the actual state of the socket even if the
            // developer didn't set Blocking = false.
            bool nonBlocking;
            int rv = Interop.Sys.Fcntl.GetIsNonBlocking(handle, out nonBlocking);
            blocking = !nonBlocking;
            Debug.Assert(rv == 0 || blocking); // ignore failures
        }

        internal void ReplaceHandleIfNecessaryAfterFailedConnect()
        {
            if (!_handle.LastConnectFailed)
            {
                return;
            }

            SocketError errorCode = ReplaceHandle();
            if (errorCode != SocketError.Success)
            {
                throw new SocketException((int)errorCode);
            }

            _handle.LastConnectFailed = false;
        }

        internal SocketError ReplaceHandle()
        {
            // Collect values of trackable socket options marked by SafeSocketHandle.TrackSocketOption().
            // The content of optionValues is uninitialized after creation but GetTrackedSocketOptions should fill the tracked options
            // and SetTrackedSocketOptions should ignore the ones which are untracked.
            Span<int> optionValues = stackalloc int[SafeSocketHandle.TrackableOptionCount];
            _handle.GetTrackedSocketOptions(optionValues, out LingerOption? lingerOption);

            // Replace the handle with a new one.
            SafeSocketHandle oldHandle = _handle;
            SocketError errorCode = SocketPal.CreateSocket(_addressFamily, _socketType, _protocolType, out SafeSocketHandle newHandle);
            Volatile.Write(ref _handle, newHandle);
            oldHandle.TransferTrackedState(_handle);
            oldHandle.Dispose();

            if (errorCode != SocketError.Success)
            {
                return errorCode;
            }

            if (Volatile.Read(ref _disposed))
            {
                _handle.Dispose();
                throw new ObjectDisposedException(GetType().FullName);
            }

            // Copy the tracked socket options to the new handle.
            _handle.SetTrackedSocketOptions(optionValues, lingerOption);

            return SocketError.Success;
        }

        private static void ThrowMultiConnectNotSupported()
        {
            throw new PlatformNotSupportedException(SR.net_sockets_connect_multiconnect_notsupported);
        }

#pragma warning disable IDE0060, CA1822
        private Socket? GetOrCreateAcceptSocket(Socket? acceptSocket, bool checkDisconnected, string propertyName, out SafeSocketHandle? handle)
        {
            if (acceptSocket != null)
            {
                if (acceptSocket._handle.HasShutdownSend)
                {
                    throw new SocketException((int)SocketError.InvalidArgument);
                }

                if (acceptSocket._rightEndPoint != null && (!checkDisconnected || !acceptSocket._isDisconnected))
                {
                    throw new InvalidOperationException(SR.Format(SR.net_sockets_namedmustnotbebound, propertyName));
                }
            }

            handle = null;
            return acceptSocket;
        }
#pragma warning restore IDE0060, CA1822

        private static void CheckTransmitFileOptions(TransmitFileOptions flags)
        {
            // Note, UseDefaultWorkerThread is the default and is == 0.
            // Unfortunately there is no TransmitFileOptions.None.
            if (flags != TransmitFileOptions.UseDefaultWorkerThread)
            {
                throw new PlatformNotSupportedException(SR.net_sockets_transmitfileoptions_notsupported);
            }
        }

        private void SendFileInternal(string? fileName, ReadOnlySpan<byte> preBuffer, ReadOnlySpan<byte> postBuffer, TransmitFileOptions flags)
        {
            CheckTransmitFileOptions(flags);

            SocketError errorCode = SocketError.Success;

            // Open the file, if any
            // Open it before we send the preBuffer so that any exception happens first
            using (SafeFileHandle? fileHandle = OpenFileHandle(fileName))
            {
                // Send the preBuffer, if any
                // This will throw on error
                if (!preBuffer.IsEmpty)
                {
                    Send(preBuffer);
                }

                // Send the file, if any
                if (fileHandle != null)
                {
                    // This can throw ObjectDisposedException.
                    errorCode = SocketPal.SendFile(_handle, fileHandle);
                }
            }

            if (errorCode != SocketError.Success)
            {
                UpdateSendSocketErrorForDisposed(ref errorCode);

                UpdateStatusAfterSocketErrorAndThrowException(errorCode);
            }

            // Send the postBuffer, if any
            // This will throw on error
            if (!postBuffer.IsEmpty)
            {
                Send(postBuffer);
            }
        }

        internal void DisposeHandle()
        {
            _handle.Dispose();
        }

        internal void ClearHandle()
        {
            _handle = null!;
        }

        internal Socket CopyStateFromSource(Socket source)
        {
            _addressFamily = source._addressFamily;
            _closeTimeout = source._closeTimeout;
            _disposed = source._disposed;
            _handle = source._handle;
            _isConnected = source._isConnected;
            _isDisconnected = source._isDisconnected;
            _isListening = source._isListening;
            _nonBlockingConnectInProgress = source._nonBlockingConnectInProgress;
            _protocolType = source._protocolType;
            _receivingPacketInformation = source._receivingPacketInformation;
            _remoteEndPoint = source._remoteEndPoint;
            _rightEndPoint = source._rightEndPoint;
            _socketType = source._socketType;
            _willBlock = source._willBlock;
            _willBlockInternal = source._willBlockInternal;
            _localEndPoint = source._localEndPoint;
            _multiBufferReceiveEventArgs = source._multiBufferReceiveEventArgs;
            _multiBufferSendEventArgs = source._multiBufferSendEventArgs;
            _pendingConnectRightEndPoint = source._pendingConnectRightEndPoint;
            _singleBufferReceiveEventArgs = source._singleBufferReceiveEventArgs;
#if DEBUG
            // Try to detect if a property gets added that we're not copying correctly.
            foreach (PropertyInfo pi in typeof(Socket).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                try
                {
                    object? origValue = pi.GetValue(source);
                    object? cloneValue = pi.GetValue(this);

                    Debug.Assert(Equals(origValue, cloneValue), $"{pi.Name}. Expected: {origValue}, Actual: {cloneValue}");
                }
                catch (TargetInvocationException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.OperationNotSupported)
                {
                    // macOS fails to retrieve DontFragment and MulticastLoopback at the moment
                }
            }
#endif
            return this;
        }
    }
}
