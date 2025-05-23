// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Caching.Hosting;
using System.Runtime.Caching.Resources;
using System.Security;
using System.Text;
using System.Threading;

namespace System.Runtime.Caching
{
    public sealed class HostFileChangeMonitor : FileChangeMonitor
    {
        private const int MAX_CHAR_COUNT_OF_LONG_CONVERTED_TO_HEXADECIMAL_STRING = 16;
        private static IFileChangeNotificationSystem s_fcn;
        private readonly ReadOnlyCollection<string> _filePaths;
        private string _uniqueId;
        private object _fcnState;
        private DateTimeOffset _lastModified;

        private HostFileChangeMonitor() { } // hide default .ctor

        private void InitDisposableMembers()
        {
            bool dispose = true;
            try
            {
                string uniqueId = null;
                if (_filePaths.Count == 1)
                {
                    string path = _filePaths[0];
                    DateTimeOffset lastWrite;
                    long fileSize;
                    s_fcn.StartMonitoring(path, new OnChangedCallback(OnChanged), out _fcnState, out lastWrite, out fileSize);
                    uniqueId = $"{path}{lastWrite.UtcDateTime.Ticks:X}{fileSize:X}";
                    _lastModified = lastWrite;
                }
                else
                {
                    int capacity = 0;
                    foreach (string path in _filePaths)
                    {
                        capacity += path.Length + (2 * MAX_CHAR_COUNT_OF_LONG_CONVERTED_TO_HEXADECIMAL_STRING);
                    }
                    Hashtable fcnState = new Hashtable(_filePaths.Count);
                    _fcnState = fcnState;
                    StringBuilder sb = new StringBuilder(capacity);
                    foreach (string path in _filePaths)
                    {
                        if (fcnState.Contains(path))
                        {
                            continue;
                        }
                        DateTimeOffset lastWrite;
                        long fileSize;
                        object state;
                        s_fcn.StartMonitoring(path, new OnChangedCallback(OnChanged), out state, out lastWrite, out fileSize);
                        fcnState[path] = state;
                        sb.Append(path);
                        sb.Append(lastWrite.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture));
                        sb.Append(fileSize.ToString("X", CultureInfo.InvariantCulture));
                        if (lastWrite > _lastModified)
                        {
                            _lastModified = lastWrite;
                        }
                    }
                    uniqueId = sb.ToString();
                }
                _uniqueId = uniqueId;
                dispose = false;
            }
            finally
            {
                InitializationComplete();
                if (dispose)
                {
                    Dispose();
                }
            }
        }

        private static void InitFCN()
        {
            if (s_fcn == null)
            {
                IFileChangeNotificationSystem fcn = null;
                IServiceProvider host = ObjectCache.Host;
                if (host != null)
                {
                    fcn = host.GetService(typeof(IFileChangeNotificationSystem)) as IFileChangeNotificationSystem;
                }
#pragma warning disable IDE0074 // Use compound assignment
                if (fcn == null)
                {
#if NET
                    if (OperatingSystem.IsBrowser() || OperatingSystem.IsWasi() || (OperatingSystem.IsIOS() && !OperatingSystem.IsMacCatalyst()) || OperatingSystem.IsTvOS())
                    {
                        throw new PlatformNotSupportedException();
                    }
#endif

                    fcn = new FileChangeNotificationSystem();
                }
#pragma warning restore IDE0074
                Interlocked.CompareExchange(ref s_fcn, fcn, null);
            }
        }

        //
        // protected members
        //

        protected override void Dispose(bool disposing)
        {
            if (disposing && s_fcn != null)
            {
                if (_filePaths != null && _fcnState != null)
                {
                    if (_filePaths.Count > 1)
                    {
                        Hashtable fcnState = _fcnState as Hashtable;
                        foreach (string path in _filePaths)
                        {
                            if (path != null)
                            {
                                object state = fcnState[path];
                                if (state != null)
                                {
                                    s_fcn.StopMonitoring(path, state);
                                }
                            }
                        }
                    }
                    else
                    {
                        string path = _filePaths[0];
                        if (path != null && _fcnState != null)
                        {
                            s_fcn.StopMonitoring(path, _fcnState);
                        }
                    }
                }
            }
        }

        //
        // public and internal members
        //

        public override ReadOnlyCollection<string> FilePaths { get { return _filePaths; } }
        public override string UniqueId { get { return _uniqueId; } }
        public override DateTimeOffset LastModified { get { return _lastModified; } }

        public HostFileChangeMonitor(IList<string> filePaths)
        {
            ArgumentNullException.ThrowIfNull(filePaths);

            if (filePaths.Count == 0)
            {
                throw new ArgumentException(RH.Format(SR.Empty_collection, nameof(filePaths)));
            }

            _filePaths = SanitizeFilePathsList(filePaths);

            InitFCN();
            InitDisposableMembers();
        }

        private static ReadOnlyCollection<string> SanitizeFilePathsList(IList<string> filePaths)
        {
            List<string> newList = new List<string>(filePaths.Count);

            foreach (string path in filePaths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException(RH.Format(SR.Collection_contains_null_or_empty_string, nameof(filePaths)));
                }
                else
                {
                    newList.Add(path);
                }
            }

            return newList.AsReadOnly();
        }
    }
}
