// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using Xunit;
using Microsoft.DotNet.RemoteExecutor;
using Microsoft.DotNet.XUnitExtensions;

namespace System.Diagnostics.Tests
{
    public partial class ProcessTests : ProcessTestBase
    {
        private static bool IsRemoteExecutorSupportedAndPrivilegedProcess => RemoteExecutor.IsSupported && PlatformDetection.IsPrivilegedProcess;

        [Fact]
        private void TestWindowApisUnix()
        {
            // This tests the hardcoded implementations of these APIs on Unix.
            using (Process p = Process.GetCurrentProcess())
            {
                Assert.True(p.Responding);
                Assert.Equal(string.Empty, p.MainWindowTitle);
                Assert.False(p.CloseMainWindow());
                Assert.Throws<InvalidOperationException>(()=>p.WaitForInputIdle());
            }
        }

        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        public void MainWindowHandle_GetUnix_ReturnsDefaultValue()
        {
            CreateDefaultProcess();

            Assert.Equal(IntPtr.Zero, _process.MainWindowHandle);
        }

        [Fact]
        public void TestProcessOnRemoteMachineUnix()
        {
            Process currentProcess = Process.GetCurrentProcess();

            Assert.Throws<PlatformNotSupportedException>(() => Process.GetProcessesByName(currentProcess.ProcessName, "127.0.0.1"));
            Assert.Throws<PlatformNotSupportedException>(() => Process.GetProcessById(currentProcess.Id, "127.0.0.1"));
        }

        [Theory]
        [MemberData(nameof(MachineName_Remote_TestData))]
        public void GetProcessesByName_RemoteMachineNameUnix_ThrowsPlatformNotSupportedException(string machineName)
        {
            Process currentProcess = Process.GetCurrentProcess();
            Assert.Throws<PlatformNotSupportedException>(() => Process.GetProcessesByName(currentProcess.ProcessName, machineName));
        }

        [Fact]
        public void TestRootGetProcessById()
        {
            Process p = Process.GetProcessById(1);
            Assert.Equal(1, p.Id);
        }

        [Fact]
        [PlatformSpecific(TestPlatforms.Linux)]
        public void ProcessStart_UseShellExecute_OnLinux_ThrowsIfNoProgramInstalled()
        {
            if (!s_allowedProgramsToRun.Any(program => IsProgramInstalled(program)))
            {
                Console.WriteLine($"None of the following programs were installed on this machine: {string.Join(",", s_allowedProgramsToRun)}.");
                Assert.Throws<Win32Exception>(() => Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = Environment.CurrentDirectory }));
            }
        }

        [Fact]
        [OuterLoop("Opens program")]
        [SkipOnPlatform(TestPlatforms.MacCatalyst, "In App Sandbox mode, the process doesn't have read access to the binary.")]
        [SkipOnPlatform(TestPlatforms.iOS | TestPlatforms.tvOS | TestPlatforms.Android | TestPlatforms.Browser, "Not supported on iOS/tvOS/Android/Browser.")]
        public void ProcessStart_DirectoryNameInCurDirectorySameAsFileNameInExecDirectory_Success()
        {
            string fileToOpen = "dotnet";
            string curDir = Environment.CurrentDirectory;
            string dotnetFolder = Path.Combine(Path.GetTempPath(),"dotnet");
            bool shouldDelete = !Directory.Exists(dotnetFolder);
            try
            {
                Directory.SetCurrentDirectory(Path.GetTempPath());
                Directory.CreateDirectory(dotnetFolder);

                using (var px = Process.Start(fileToOpen))
                {
                    Assert.NotNull(px);
                }
            }
            finally
            {
                if (shouldDelete)
                {
                    Directory.Delete(dotnetFolder);
                }

                Directory.SetCurrentDirectory(curDir);
            }
        }

        [Fact]
        [OuterLoop]
        [SkipOnPlatform(TestPlatforms.iOS | TestPlatforms.tvOS | TestPlatforms.Android | TestPlatforms.Browser, "Not supported on iOS/tvOS/Android/Browser.")]
        public void ProcessStart_UseShellExecute_OnUnix_OpenMissingFile_DoesNotThrow()
        {
            if (OperatingSystem.IsLinux() &&
                s_allowedProgramsToRun.FirstOrDefault(program => IsProgramInstalled(program)) == null)
            {
                return;
            }
            string fileToOpen = Path.Combine(Environment.CurrentDirectory, "_no_such_file.TXT");
            using (var px = Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = fileToOpen }))
            {
                Assert.NotNull(px);
                px.Kill();
                px.WaitForExit();
                Assert.True(px.HasExited);
            }
        }

        [Theory, InlineData(true), InlineData(false)]
        [OuterLoop("Opens program")]
        [SkipOnPlatform(TestPlatforms.iOS | TestPlatforms.tvOS | TestPlatforms.Android | TestPlatforms.Browser, "Not supported on iOS/tvOS/Android/Browser.")]
        public void ProcessStart_UseShellExecute_OnUnix_SuccessWhenProgramInstalled(bool isFolder)
        {
            string programToOpen = s_allowedProgramsToRun.FirstOrDefault(program => IsProgramInstalled(program));
            string fileToOpen;
            if (isFolder)
            {
                fileToOpen = Environment.CurrentDirectory;
            }
            else
            {
                fileToOpen = GetTestFilePath() + ".txt";
                File.WriteAllText(fileToOpen, $"{nameof(ProcessStart_UseShellExecute_OnUnix_SuccessWhenProgramInstalled)}");
            }

            if (OperatingSystem.IsMacOS() || programToOpen != null)
            {
                using (var px = Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = fileToOpen }))
                {
                    Assert.NotNull(px);
                    if (!OperatingSystem.IsMacOS()) // on OSX, process name is dotnet for some reason. Refer to https://github.com/dotnet/runtime/issues/23525
                    {
                        Assert.Equal(programToOpen, px.ProcessName);
                    }
                    px.Kill();
                    px.WaitForExit();
                    Assert.True(px.HasExited);
                }
            }
        }

        [Fact]
        [SkipOnPlatform(TestPlatforms.OSX | TestPlatforms.MacCatalyst, "On OSX, ProcessName returns the script interpreter.")]
        [SkipOnPlatform(TestPlatforms.iOS | TestPlatforms.tvOS, "Not supported on iOS or tvOS.")]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/13757")]
        public void ProcessNameMatchesScriptName()
        {
            string scriptName = GetTestFileName();
            string filename = Path.Combine(TestDirectory, scriptName);
            File.WriteAllText(filename, $"#!/bin/sh\nsleep 600\n"); // sleep 10 min.
            File.SetUnixFileMode(filename, ExecutablePermissions);

            using (var process = Process.Start(new ProcessStartInfo { FileName = filename }))
            {
                try
                {
                    string stat = File.ReadAllText($"/proc/{process.Id}/stat");
                    Assert.Contains($"({scriptName.Substring(0, 15)})", stat);
                    string cmdline = File.ReadAllText($"/proc/{process.Id}/cmdline");
                    Assert.Equal($"/bin/sh\0{filename}\0", cmdline);

                    Assert.Equal(scriptName, process.ProcessName);
                }
                finally
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
        }

        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        public void ProcessStart_SkipsNonExecutableFilesOnPATH()
        {
            const string ScriptName = "script";

            // Create a directory named ScriptName.
            string path1 = Path.Combine(TestDirectory, "Path1");
            Directory.CreateDirectory(Path.Combine(path1, ScriptName));

            // Create a non-executable file named ScriptName
            string path2 = Path.Combine(TestDirectory, "Path2");
            Directory.CreateDirectory(path2);
            File.WriteAllText(Path.Combine(path2, ScriptName), "Not executable");

            // Create an executable script named ScriptName
            string path3 = Path.Combine(TestDirectory, "Path3");
            Directory.CreateDirectory(path3);
            string filename = WriteScriptFile(path3, ScriptName, returnValue: 42);

            // Process.Start ScriptName with the above on PATH.
            RemoteInvokeOptions options = new RemoteInvokeOptions();
            options.StartInfo.EnvironmentVariables["PATH"] = $"{path1}:{path2}:{path3}";
            RemoteExecutor.Invoke(() =>
            {
                using (var px = Process.Start(new ProcessStartInfo { FileName = ScriptName }))
                {
                    Assert.NotNull(px);
                    px.WaitForExit();
                    Assert.True(px.HasExited);
                    Assert.Equal(42, px.ExitCode);
                }
            }, options).Dispose();
        }

        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        [PlatformSpecific(TestPlatforms.Linux)] // s_allowedProgramsToRun is Linux specific
        public void ProcessStart_UseShellExecute_OnUnix_FallsBackWhenNotRealExecutable()
        {
            // Create a script that we'll use to 'open' the file by putting it on PATH
            // with the appropriate name.
            string path = Path.Combine(TestDirectory, "Path");
            Directory.CreateDirectory(path);
            WriteScriptFile(path, s_allowedProgramsToRun[0], returnValue: 42);

            // Create a file that has the x-bit set, but which isn't a valid script.
            string filename = WriteScriptFile(TestDirectory, GetTestFileName(), returnValue: 0);
            File.WriteAllText(filename, $"not a script");
            File.SetUnixFileMode(filename, ExecutablePermissions);

            RemoteInvokeOptions options = new RemoteInvokeOptions();
            options.StartInfo.EnvironmentVariables["PATH"] = path;
            RemoteExecutor.Invoke(fileToOpen =>
            {
                using (var px = Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = fileToOpen }))
                {
                    Assert.NotNull(px);
                    px.WaitForExit();
                    Assert.True(px.HasExited);
                    Assert.Equal(42, px.ExitCode);
                }
            }, filename, options).Dispose();
        }

        [Fact]
        [PlatformSpecific(TestPlatforms.Linux)] // test relies on xdg-open
        public void ProcessStart_UseShellExecute_OnUnix_DocumentFile_IgnoresArguments()
        {
            Assert.Equal("xdg-open", s_allowedProgramsToRun[0]);

            if (!IsProgramInstalled("xdg-open"))
            {
                return;
            }

            // Open a file that doesn't exist with an argument that xdg-open considers invalid.
            var startInfo = new ProcessStartInfo { UseShellExecute = true, FileName = "/nosuchfile", Arguments = "invalid_arg" };
            startInfo.Environment.Remove("DISPLAY"); // Get rid of DISPLAY environment variable as this causes spurious test failures.
            using (var px = Process.Start(startInfo))
            {
                Assert.NotNull(px);
                px.WaitForExit();
                // xdg-open returns different failure exit codes, 1 indicates an error in command line syntax.
                Assert.NotEqual(0, px.ExitCode); // the command failed
                Assert.NotEqual(1, px.ExitCode); // the failure is not due to the invalid argument
            }
        }

        [Fact]
        [PlatformSpecific(TestPlatforms.Linux)]
        public void ProcessStart_UseShellExecute_OnUnix_Executable_PassesArguments()
        {
            string testFilePath = GetTestFilePath();
            Assert.False(File.Exists(testFilePath));

            // Start a process that will create a file pass the filename as Arguments.
            using (var px = Process.Start(new ProcessStartInfo { UseShellExecute = true,
                                                                 FileName = "touch",
                                                                 Arguments = testFilePath }))
            {
                Assert.NotNull(px);
                px.WaitForExit();
                Assert.Equal(0, px.ExitCode);
            }

            Assert.True(File.Exists(testFilePath));
        }

        [ConditionalTheory(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        [InlineData((string)null, true)]
        [InlineData("", true)]
        [InlineData("open", true)]
        [InlineData("Open", true)]
        [InlineData("invalid", false)]
        [PlatformSpecific(TestPlatforms.Linux)] // s_allowedProgramsToRun is Linux specific
        public void ProcessStart_UseShellExecute_OnUnix_ValidVerbs(string verb, bool isValid)
        {
            // Create a script that we'll use to 'open' the file by putting it on PATH
            // with the appropriate name.
            string path = Path.Combine(TestDirectory, "Path");
            Directory.CreateDirectory(path);
            WriteScriptFile(path, s_allowedProgramsToRun[0], returnValue: 42);

            RemoteInvokeOptions options = new RemoteInvokeOptions();
            options.StartInfo.EnvironmentVariables["PATH"] = path;
            RemoteExecutor.Invoke((argVerb, argValid) =>
            {
                if (argVerb == "<null>")
                {
                    argVerb = null;
                }

                var psi = new ProcessStartInfo { UseShellExecute = true, FileName = "/", Verb = argVerb };
                if (bool.Parse(argValid))
                {
                    using (var px = Process.Start(psi))
                    {
                        Assert.NotNull(px);
                        px.WaitForExit();
                        Assert.True(px.HasExited);
                        Assert.Equal(42, px.ExitCode);
                    }
                }
                else
                {
                    Assert.Throws<Win32Exception>(() => Process.Start(psi));
                }
            }, verb ?? "<null>", isValid.ToString(), options).Dispose();
        }

        [Fact]
        [PlatformSpecific(TestPlatforms.Linux)]
        public void ProcessStart_OnLinux_UsesSpecifiedProgram()
        {
            const string Program = "sleep";

            using (var px = Process.Start(Program, "60"))
            {
                try
                {
                    Assert.Equal(Program, px.ProcessName);
                }
                finally
                {
                    px.Kill();
                    px.WaitForExit();
                }
                Assert.True(px.HasExited);
            }
        }

        [Fact]
        [PlatformSpecific(TestPlatforms.Linux)]
        public void ProcessStart_OnLinux_UsesSpecifiedProgramUsingArgumentList()
        {
            const string Program = "sleep";

            ProcessStartInfo psi = new ProcessStartInfo(Program);
            psi.ArgumentList.Add("60");
            using (var px = Process.Start(psi))
            {
                try
                {
                    Assert.Equal(Program, px.ProcessName);
                }
                finally
                {
                    px.Kill();
                    px.WaitForExit();
                }
                Assert.True(px.HasExited);
            }
        }

        [Theory, InlineData("/usr/bin/open"), InlineData("/usr/bin/nano")]
        [PlatformSpecific(TestPlatforms.OSX | TestPlatforms.MacCatalyst)]
        [OuterLoop("Opens program")]
        public void ProcessStart_OpenFileOnOsx_UsesSpecifiedProgram(string programToOpenWith)
        {
            string fileToOpen = GetTestFilePath() + ".txt";
            File.WriteAllText(fileToOpen, $"{nameof(ProcessStart_OpenFileOnOsx_UsesSpecifiedProgram)}");
            using (var px = Process.Start(programToOpenWith, fileToOpen))
            {
                // Assert.Equal(programToOpenWith, px.ProcessName); // on OSX, process name is dotnet for some reason. Refer to https://github.com/dotnet/runtime/issues/23525
                Console.WriteLine($"in OSX, {nameof(programToOpenWith)} is {programToOpenWith}, while {nameof(px.ProcessName)} is {px.ProcessName}.");
                px.Kill();
                px.WaitForExit();
                Assert.True(px.HasExited);
            }
        }

        [Theory, InlineData("Safari"), InlineData("\"Google Chrome\"")]
        [PlatformSpecific(TestPlatforms.OSX | TestPlatforms.MacCatalyst)]
        [OuterLoop("Opens browser")]
        public void ProcessStart_OpenUrl_UsesSpecifiedApplication(string applicationToOpenWith)
        {
            using (var px = Process.Start("/usr/bin/open", "https://github.com/dotnet/corefx -a " + applicationToOpenWith))
            {
                Assert.NotNull(px);
                px.Kill();
                px.WaitForExit();
                Assert.True(px.HasExited);
            }
        }

        [Theory, InlineData("-a Safari"), InlineData("-a \"Google Chrome\"")]
        [PlatformSpecific(TestPlatforms.OSX | TestPlatforms.MacCatalyst)]
        [OuterLoop("Opens browser")]
        public void ProcessStart_UseShellExecuteTrue_OpenUrl_SuccessfullyReadsArgument(string arguments)
        {
            var startInfo = new ProcessStartInfo { UseShellExecute = true, FileName = "https://github.com/dotnet/corefx", Arguments = arguments };
            using (var px = Process.Start(startInfo))
            {
                Assert.NotNull(px);
                px.Kill();
                px.WaitForExit();
                Assert.True(px.HasExited);
            }
        }

        public static TheoryData<string[]> StartOSXProcessWithArgumentList => new TheoryData<string[]>
        {
            { new string[] { "-a", "Safari" } },
            { new string[] { "-a", "\"Google Chrome\"" } }
        };

        [Theory,
            MemberData(nameof(StartOSXProcessWithArgumentList))]
        [PlatformSpecific(TestPlatforms.OSX | TestPlatforms.MacCatalyst)]
        [OuterLoop("Opens browser")]
        public void ProcessStart_UseShellExecuteTrue_OpenUrl_SuccessfullyReadsArgumentArray(string[] argumentList)
        {
            var startInfo = new ProcessStartInfo { UseShellExecute = true, FileName = "https://github.com/dotnet/corefx"};

            foreach (string item in argumentList)
            {
                startInfo.ArgumentList.Add(item);
            }

            using (var px = Process.Start(startInfo))
            {
                Assert.NotNull(px);
                px.Kill();
                px.WaitForExit();
                Assert.True(px.HasExited);
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsPrivilegedProcess))]
        public void TestPriorityClassUnix()
        {
            CreateDefaultProcess();

            ProcessPriorityClass priorityClass = _process.PriorityClass;

            _process.PriorityClass = ProcessPriorityClass.Idle;
            Assert.Equal(ProcessPriorityClass.Idle, _process.PriorityClass);

            try
            {
                _process.PriorityClass = ProcessPriorityClass.High;
                Assert.Equal(ProcessPriorityClass.High, _process.PriorityClass);

                _process.PriorityClass = ProcessPriorityClass.Normal;
                Assert.Equal(ProcessPriorityClass.Normal, _process.PriorityClass);

                _process.PriorityClass = priorityClass;
            }
            catch (Win32Exception ex)
            {
                Assert.False(PlatformDetection.IsPrivilegedProcess, $"Failed even though superuser {ex.ToString()}");
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsPrivilegedProcess))]
        public void TestBasePriorityOnUnix()
        {
            CreateDefaultProcess();

            ProcessPriorityClass originalPriority = _process.PriorityClass;
            Assert.Equal(ProcessPriorityClass.Normal, originalPriority);
            SetAndCheckBasePriority(ProcessPriorityClass.Idle, 19);

            try
            {
                SetAndCheckBasePriority(ProcessPriorityClass.Normal, 0);
                SetAndCheckBasePriority(ProcessPriorityClass.High, -11);
                _process.PriorityClass = originalPriority;
            }
            catch (Win32Exception ex)
            {
                Assert.False(PlatformDetection.IsPrivilegedProcess, $"Failed even though superuser {ex.ToString()}");
            }
        }

        [Fact]
        [SkipOnPlatform(TestPlatforms.iOS | TestPlatforms.tvOS, "Not supported on iOS or tvOS.")]
        public void TestStartOnUnixWithBadPermissions()
        {
            string path = GetTestFilePath();
            File.Create(path).Dispose();
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);

            Win32Exception e = Assert.Throws<Win32Exception>(() => Process.Start(path));
            Assert.NotEqual(0, e.NativeErrorCode);
        }

        [Fact]
        [SkipOnPlatform(TestPlatforms.iOS | TestPlatforms.tvOS, "Not supported on iOS or tvOS.")]
        public void TestStartOnUnixWithBadFormat()
        {
            string path = GetTestFilePath();
            File.Create(path).Dispose();
            File.SetUnixFileMode(path, ExecutablePermissions);

            Win32Exception e = Assert.Throws<Win32Exception>(() => Process.Start(path));
            Assert.NotEqual(0, e.NativeErrorCode);
        }

        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        public void TestStartWithNonExistingUserThrows()
        {
            Process p = CreateProcessPortable(RemotelyInvokable.Dummy);
            p.StartInfo.UserName = "DoesNotExist";
            Assert.Throws<Win32Exception>(() => p.Start());
        }

        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        public void TestExitCodeKilledChild()
        {
            using (Process p = CreateProcessLong())
            {
                p.Start();
                p.Kill();
                p.WaitForExit();

                // SIGKILL may change per platform
                const int SIGKILL = 9; // Linux, macOS, FreeBSD, ...
                Assert.Equal(128 + SIGKILL, p.ExitCode);
            }
        }

        private static int CheckUserAndGroupIds(string userId, string groupId, string groupIdsJoined, string checkGroupsExact)
        {
            Assert.Equal(userId, getuid().ToString());
            Assert.Equal(userId, geteuid().ToString());
            Assert.Equal(groupId, getgid().ToString());
            Assert.Equal(groupId, getegid().ToString());

            var expectedGroups = new HashSet<uint>(groupIdsJoined.Split(',').Select(s => uint.Parse(s)));

            if (bool.Parse(checkGroupsExact))
            {
                AssertExtensions.Equal(expectedGroups, GetGroups());
            }
            else
            {
                Assert.Subset(expectedGroups, GetGroups());
            }

            return RemoteExecutor.SuccessExitCode;
        }

        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        [SkipOnPlatform(TestPlatforms.LinuxBionic, "Bionic is not normal Linux, has no normal users/groups")]
        public void TestCheckChildProcessUserAndGroupIds()
        {
            string userName = GetCurrentRealUserName();
            string userId = GetUserId(userName);
            string userGroupId = GetUserGroupId(userName);
            string userGroupIds = GetUserGroupIds(userName);
            // If this test runs as the user, we expect to be able to match the user groups exactly.
            // Except on OSX, where getgrouplist may return a list of groups truncated to NGROUPS_MAX.
            bool checkGroupsExact = userId == geteuid().ToString() &&
                                    !OperatingSystem.IsMacOS();

            // Start as username
            var invokeOptions = new RemoteInvokeOptions();
            invokeOptions.StartInfo.UserName = userName;
            using (RemoteInvokeHandle handle = RemoteExecutor.Invoke(CheckUserAndGroupIds, userId, userGroupId, userGroupIds, checkGroupsExact.ToString(),
                                                            invokeOptions))
            { }
        }

        /// <summary>
        /// Tests when running as root and starting a new process as a normal user,
        /// the new process doesn't have elevated privileges.
        /// </summary>
        [ConditionalTheory(nameof(IsRemoteExecutorSupportedAndPrivilegedProcess))]
        [InlineData(true)]
        [InlineData(false)]
        public unsafe void TestCheckChildProcessUserAndGroupIdsElevated(bool useRootGroups)
        {
            Func<string, string, int> runsAsRoot = (string username, string useRootGroupsArg) =>
            {
                // Verify we are root
                Assert.Equal(0U, getuid());
                Assert.Equal(0U, geteuid());
                Assert.Equal(0U, getgid());
                Assert.Equal(0U, getegid());

                string userId = GetUserId(username);
                string userGroupId = GetUserGroupId(username);
                string userGroupIds = GetUserGroupIds(username);

                if (bool.Parse(useRootGroupsArg))
                {
                    uint rootGroups = 0;
                    int setGroupsRv = setgroups(1, &rootGroups);
                    Assert.Equal(0, setGroupsRv);
                }

                // On systems with a low value of NGROUPS_MAX (e.g 16 on OSX), the groups may be truncated.
                // On Linux NGROUPS_MAX is 65536, so we expect to see every group.
                bool checkGroupsExact = OperatingSystem.IsLinux();

                // Start as username
                var invokeOptions = new RemoteInvokeOptions();
                invokeOptions.StartInfo.UserName = username;
                using (RemoteInvokeHandle handle = RemoteExecutor.Invoke(CheckUserAndGroupIds, userId, userGroupId, userGroupIds, checkGroupsExact.ToString(), invokeOptions))
                { }

                return RemoteExecutor.SuccessExitCode;
            };

            // Start as root
            string userName = GetCurrentRealUserName();
            using (RemoteInvokeHandle handle = RemoteExecutor.Invoke(runsAsRoot, userName, useRootGroups.ToString(),
                                                            new RemoteInvokeOptions { RunAsSudo = true }))
            { }
        }

        private static string GetUserId(string username)
            => StartAndReadToEnd("id", new[] { "-u", username }).Trim('\n');

        private static string GetUserGroupId(string username)
            => StartAndReadToEnd("id", new[] { "-g", username }).Trim('\n');

        private static string GetUserGroupIds(string username)
        {
            string[] groupIds = StartAndReadToEnd("id", new[] { "-G", username })
                                    .Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(",", groupIds.Select(s => uint.Parse(s)).OrderBy(id => id));
        }

        private static string GetCurrentRealUserName()
        {
            string realUserName = Environment.IsPrivilegedProcess ?
                Environment.GetEnvironmentVariable("SUDO_USER") :
                Environment.UserName;

            Assert.NotNull(realUserName);
            Assert.NotEqual("root", realUserName);

            return realUserName;
        }

        /// <summary>
        /// Tests whether child processes are reaped (cleaning up OS resources)
        /// when they terminate.
        /// </summary>
        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        [PlatformSpecific(TestPlatforms.Linux)] // Test uses Linux specific '/proc' filesystem
        public async Task TestChildProcessCleanup()
        {
            using (Process process = CreateShortProcess())
            {
                process.Start();
                bool processReaped = await TryWaitProcessReapedAsync(process.Id, timeoutMs: 30000);
                Assert.True(processReaped);
            }
        }

        /// <summary>
        /// Tests whether child processes are reaped (cleaning up OS resources)
        /// when they terminate after the Process was Disposed.
        /// </summary>
        [ConditionalTheory(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [PlatformSpecific(TestPlatforms.Linux)] // Test uses Linux specific '/proc' filesystem
        public async Task TestChildProcessCleanupAfterDispose(bool shortProcess, bool enableEvents)
        {
            // We test using a long and short process. The long process will terminate after Dispose,
            // The short process will terminate at the same time, possibly revealing race conditions.
            int processId = -1;
            using (Process process = shortProcess ? CreateShortProcess() : CreateSleepProcess(durationMs: 500))
            {
                process.Start();
                processId = process.Id;
                if (enableEvents)
                {
                    // Dispose will disable the Exited event.
                    // We enable it to check this doesn't cause issues for process reaping.
                    process.EnableRaisingEvents = true;
                }
            }
            bool processReaped = await TryWaitProcessReapedAsync(processId, timeoutMs: 30000);
            Assert.True(processReaped);
        }

        private static Process CreateShortProcess()
        {
            Process process = new Process();
            process.StartInfo.FileName = "uname";
            return process;
        }

        private static async Task<bool> TryWaitProcessReapedAsync(int pid, int timeoutMs)
        {
            const int SleepTimeMs = 50;
            // When the process is reaped, the '/proc/<pid>' directory to disappears.
            bool procPidExists = true;
            for (int attempt = 0; attempt < (timeoutMs / SleepTimeMs); attempt++)
            {
                procPidExists = Directory.Exists("/proc/" + pid);
                if (procPidExists)
                {
                    await Task.Delay(SleepTimeMs);
                }
                else
                {
                    break;
                }
            }
            return !procPidExists;
        }

        /// <summary>
        /// Tests the ProcessWaitState reference count drops to zero.
        /// </summary>
        [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        public async Task TestProcessWaitStateReferenceCount()
        {
            using (var exitedEventSemaphore = new SemaphoreSlim(0, 1))
            {
                object waitState = null;
                int processId = -1;
                // Process takes a reference
                using (var process = CreateShortProcess())
                {
                    process.EnableRaisingEvents = true;
                    // Exited event takes a reference
                    process.Exited += (o,e) => exitedEventSemaphore.Release();
                    process.Start();

                    processId = process.Id;
                    waitState = GetProcessWaitState(process);

                    process.WaitForExit();

                    Assert.False(GetWaitStateDictionary(childDictionary: false).Contains(processId));
                    Assert.True(GetWaitStateDictionary(childDictionary: true).Contains(processId));
                }
                exitedEventSemaphore.Wait();

                // Child reaping holds a reference too
                int referenceCount = -1;
                const int SleepTimeMs = 50;
                for (int i = 0; i < (30000 / SleepTimeMs); i++)
                {
                    referenceCount = GetWaitStateReferenceCount(waitState);
                    if (referenceCount == 0)
                    {
                        break;
                    }
                    else
                    {
                        // Process was reaped but ProcessWaitState not unrefed yet
                        await Task.Delay(SleepTimeMs);
                    }
                }
                Assert.Equal(0, referenceCount);

                Assert.Equal(0, GetWaitStateReferenceCount(waitState));
                Assert.False(GetWaitStateDictionary(childDictionary: false).Contains(processId));
                Assert.False(GetWaitStateDictionary(childDictionary: true).Contains(processId));
            }
        }

        private static bool IsStressModeEnabledAndRemoteExecutorSupported => TestEnvironment.IsStressModeEnabled && RemoteExecutor.IsSupported;

        /// <summary>
        /// Verifies a new Process instance can refer to a process with a recycled pid for which
        /// there is still an existing Process instance. Operations on the existing instance will
        /// throw since that process has exited.
        /// </summary>
        [ConditionalFact(nameof(IsStressModeEnabledAndRemoteExecutorSupported))]
        public void TestProcessRecycledPid()
        {
            const int LinuxPidMaxDefault = 32768;
            var processes = new Dictionary<int, Process>(LinuxPidMaxDefault);
            bool foundRecycled = false;
            for (int i = 0; i < int.MaxValue; i++)
            {
                var process = CreateProcessLong();
                process.Start();

                Process recycled;
                foundRecycled = processes.TryGetValue(process.Id, out recycled);
                if (foundRecycled)
                {
                    Assert.Throws<InvalidOperationException>(() => recycled.Kill());
                }

                process.Kill();
                process.WaitForExit();

                if (foundRecycled)
                {
                    break;
                }
                else
                {
                    processes.Add(process.Id, process);
                }
            }

            Assert.True(foundRecycled);
        }

        [ConditionalTheory(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        [InlineData("/dev/stdin",  O_RDONLY)]
        [InlineData("/dev/stdout", O_WRONLY)]
        [InlineData("/dev/stderr", O_WRONLY)]
        public void ChildProcessRedirectedIO_FilePathOpenShouldSucceed(string filename, int flags)
        {
            var options = new RemoteInvokeOptions { StartInfo = new ProcessStartInfo { RedirectStandardOutput = true, RedirectStandardInput = true, RedirectStandardError = true }};
            using (RemoteInvokeHandle handle = RemoteExecutor.Invoke(ExecuteChildProcess, filename, flags.ToString(CultureInfo.InvariantCulture), options))
            { }

            static void ExecuteChildProcess(string filename, string flags)
            {
                int result = open(filename, int.Parse(flags, CultureInfo.InvariantCulture));
                Assert.True(result >= 0, $"failed to open file with {result} and errno {Marshal.GetLastWin32Error()}.");
            }
        }

        [Fact]
        [PlatformSpecific(TestPlatforms.OSX)]
        public unsafe void TestTotalProcessorTimeMacOs()
        {
            var rUsage = Interop.libproc.proc_pid_rusage(Environment.ProcessId);
            var timeBase = new Interop.libSystem.mach_timebase_info_data_t();
            Interop.libSystem.mach_timebase_info(&timeBase);

            var nativeUserUs = rUsage.ri_user_time / 1000 * timeBase.numer / timeBase.denom;
            var nativeSystemUs = rUsage.ri_system_time / 1000 * timeBase.numer / timeBase.denom;
            var nativeTotalUs = nativeSystemUs + nativeUserUs;

            var nativeUserTime = TimeSpan.FromMicroseconds(nativeUserUs);
            var nativeSystemTime = TimeSpan.FromMicroseconds(nativeSystemUs);
            var nativeTotalTime = TimeSpan.FromMicroseconds(nativeTotalUs);

            var process = Process.GetCurrentProcess();
            var managedUserTime = process.UserProcessorTime;
            var managedSystemTime = process.PrivilegedProcessorTime;
            var managedTotalTime = process.TotalProcessorTime;

            AssertTime(managedUserTime, nativeUserTime, "user");
            AssertTime(managedSystemTime, nativeSystemTime, "system");
            AssertTime(managedTotalTime, nativeTotalTime, "total");

            void AssertTime(TimeSpan managed, TimeSpan native, string label)
            {
                Assert.True(
                    managed >= native,
                    $"Time '{label}' returned by managed API ({managed}) should be greated or equal to the time returned by native API ({native}).");
            }
        }

        [ConditionalTheory(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Kill_ExitedNonChildProcess_DoesNotThrow(bool killTree)
        {
            // In this test, we kill a process in a way the Process instance
            // is not aware the process has terminated when we invoke Process.Kill.
            DateTime start = DateTime.UtcNow;
            using (Process nonChildProcess = CreateNonChildProcess())
            {
                // Kill the process.
                int rv = kill(nonChildProcess.Id, SIGKILL);
                Assert.Equal(0, rv);

                // Wait until the process is reaped.
                while (rv == 0)
                {
                    rv = kill(nonChildProcess.Id, 0);
                    if (rv == 0)
                    {
                        // process still exists, wait some time.
                        await Task.Delay(100);
                    }

                    DateTime now = DateTime.UtcNow;
                    if (start.Ticks + (Helpers.PassingTestTimeoutMilliseconds * 10_000) <= now.Ticks)
                    {
                        Console.WriteLine("{0} Failed to kill process {1} started at {2}", now, nonChildProcess.Id, start);
                        Helpers.DumpAllProcesses();

                        Assert.Fail("test timed out");
                    }
                }

                // Call Process.Kill.
                nonChildProcess.Kill(killTree);
            }

            Process CreateNonChildProcess()
            {
                // Create a process that isn't a direct child.
                int nonChildPid = -1;
                RemoteInvokeHandle createNonChildProcess = RemoteExecutor.Invoke(arg =>
                {
                    RemoteInvokeHandle nonChildProcess = RemoteExecutor.Invoke(
                        // Process that lives as long as the test process.
                        testProcessPid => Process.GetProcessById(int.Parse(testProcessPid)).WaitForExit(), arg,
                        // Don't pass our standard out to the sleepProcess or the ReadToEnd below won't return.
                        new RemoteInvokeOptions { StartInfo = new ProcessStartInfo() { RedirectStandardOutput = true } });

                    using (nonChildProcess)
                    {
                        Console.WriteLine(nonChildProcess.Process.Id);

                        // Don't wait for the process to exit.
                        nonChildProcess.Process = null;
                    }
                }, Process.GetCurrentProcess().Id.ToString(), new RemoteInvokeOptions { StartInfo = new ProcessStartInfo() { RedirectStandardOutput = true } });
                using (createNonChildProcess)
                {
                    nonChildPid = int.Parse(createNonChildProcess.Process.StandardOutput.ReadToEnd());
                }
                return Process.GetProcessById(nonChildPid);
            }
        }

        private static IDictionary GetWaitStateDictionary(bool childDictionary)
        {
            Assembly assembly = typeof(Process).Assembly;
            Type waitStateType = assembly.GetType("System.Diagnostics.ProcessWaitState");
            FieldInfo dictionaryField = waitStateType.GetField(childDictionary ? "s_childProcessWaitStates" : "s_processWaitStates", BindingFlags.NonPublic | BindingFlags.Static);
            return (IDictionary)dictionaryField.GetValue(null);
        }

        private static object GetProcessWaitState(Process p)
        {
            MethodInfo getWaitState = typeof(Process).GetMethod("GetWaitState", BindingFlags.NonPublic | BindingFlags.Instance);
            return getWaitState.Invoke(p, null);
        }

        private static int GetWaitStateReferenceCount(object waitState)
        {
            FieldInfo referenCountField = waitState.GetType().GetField("_outstandingRefCount", BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)referenCountField.GetValue(waitState);
        }

        [DllImport("libc")]
        private static extern uint geteuid();
        [DllImport("libc")]
        private static extern uint getuid();
        [DllImport("libc")]
        private static extern uint getegid();
        [DllImport("libc")]
        private static extern uint getgid();

        [DllImport("libc", SetLastError = true)]
        private static extern unsafe int getgroups(int size, uint* list);

        private static unsafe HashSet<uint> GetGroups()
        {
            int maxSize = 128;
            Span<uint> groups = stackalloc uint[maxSize];
            fixed (uint* pGroups = groups)
            {
                int rv = getgroups(maxSize, pGroups);
                if (rv == -1)
                {
                    // If this throws with EINVAL, maxSize should be increased.
                    throw new Win32Exception();
                }

                // Return this as a HashSet to filter out duplicates.
                var result = new HashSet<uint>(groups.Slice(0, rv).ToArray());
                // according to https://man7.org/linux/man-pages/man2/getgroups.2.html it's not specified
                // if this group is included in the list returned by getgroups
                result.Add(getegid());
                return result;
            }
        }

        [DllImport("libc")]
        private static extern int seteuid(uint euid);

        [DllImport("libc")]
        private static extern unsafe int setgroups(int length, uint* groups);

        private const int SIGKILL = 9;

        [DllImport("libc", SetLastError = true)]
        private static extern int kill(int pid, int sig);

        [DllImport("libc", SetLastError = true)]
        private static extern int open(string pathname, int flags);

        private const int O_RDONLY = 0;
        private const int O_WRONLY = 1;

        private static readonly string[] s_allowedProgramsToRun = new string[] { "xdg-open", "gnome-open", "kfmclient" };

        private string WriteScriptFile(string directory, string name, int returnValue)
        {
            string filename = Path.Combine(directory, name);
            File.WriteAllText(filename, $"#!/bin/sh\nexit {returnValue}\n");
            File.SetUnixFileMode(filename, ExecutablePermissions);
            return filename;
        }

        private static string StartAndReadToEnd(string filename, string[] arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = filename,
                RedirectStandardOutput = true
            };
            foreach (var arg in arguments)
            {
                psi.ArgumentList.Add(arg);
            }
            using (Process process = Process.Start(psi))
            {
                return process.StandardOutput.ReadToEnd();
            }
        }

        private static void SendSignal(PosixSignal signal, int processId)
        {
            int result = kill(processId, Interop.Sys.GetPlatformSignalNumber(signal));
            if (result != 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to send signal {signal} to process {processId}");
            }
        }

        private static unsafe void ReEnableCtrlCHandlerIfNeeded(PosixSignal signal) { }
    }
}
