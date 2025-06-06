// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Channels.Tests
{
    public abstract partial class ChannelTestBase : TestBase
    {
        protected Channel<int> CreateChannel() => CreateChannel<int>();
        protected abstract Channel<T> CreateChannel<T>();

        protected Channel<int> CreateFullChannel() => CreateFullChannel<int>();
        protected abstract Channel<T> CreateFullChannel<T>();

        protected virtual bool AllowSynchronousContinuations => false;
        protected virtual bool RequiresSingleReader => false;
        protected virtual bool RequiresSingleWriter => false;
        protected virtual bool BuffersItems => true;
        protected virtual bool HasDebuggerTypeProxy => true;

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsDebuggerTypeProxyAttributeSupported))]
        public void ValidateDebuggerAttributes()
        {
            Channel<int> c = CreateChannel();
            DebuggerAttributes.ValidateDebuggerDisplayReferences(c);
            if (HasDebuggerTypeProxy)
            {
                DebuggerAttributes.InvokeDebuggerTypeProxyProperties(c);
            }
        }

        [Fact]
        public void Cast_MatchesInOut()
        {
            Channel<int> c = CreateChannel();
            ChannelReader<int> rc = c;
            ChannelWriter<int> wc = c;
            Assert.Same(rc, c.Reader);
            Assert.Same(wc, c.Writer);
        }

        [Fact]
        public void Completion_Idempotent()
        {
            Channel<int> c = CreateChannel();

            Task completion = c.Reader.Completion;
            Assert.Equal(TaskStatus.WaitingForActivation, completion.Status);

            Assert.Same(completion, c.Reader.Completion);
            c.Writer.Complete();
            Assert.Same(completion, c.Reader.Completion);

            Assert.Equal(TaskStatus.RanToCompletion, completion.Status);
        }

        [Fact]
        public async Task Complete_AfterEmpty_NoWaiters_TriggersCompletion()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            await c.Reader.Completion;
        }

        [Fact]
        public async Task Complete_AfterEmpty_WaitingReader_TriggersCompletion()
        {
            Channel<int> c = CreateChannel();
            Task<int> r = c.Reader.ReadAsync().AsTask();
            c.Writer.Complete();
            await c.Reader.Completion;
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => r);
        }

        [Fact]
        public async Task Complete_BeforeEmpty_WaitingReaders_TriggersCompletion()
        {
            Channel<int> c = CreateChannel();
            ValueTask<int> read = c.Reader.ReadAsync();
            c.Writer.Complete();
            await c.Reader.Completion;
            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await read);
        }

        [Fact]
        public void Complete_Twice_ThrowsInvalidOperationException()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            Assert.ThrowsAny<InvalidOperationException>(() => c.Writer.Complete());
        }

        [Fact]
        public void TryComplete_Twice_ReturnsTrueThenFalse()
        {
            Channel<int> c = CreateChannel();
            Assert.True(c.Writer.TryComplete());
            Assert.False(c.Writer.TryComplete());
            Assert.False(c.Writer.TryComplete());
        }

        [Fact]
        public async Task TryComplete_ErrorsPropagate()
        {
            Channel<int> c;

            // Success
            c = CreateChannel();
            Assert.True(c.Writer.TryComplete());
            await c.Reader.Completion;

            // Error
            c = CreateChannel();
            Assert.True(c.Writer.TryComplete(new FormatException()));
            await Assert.ThrowsAsync<FormatException>(() => c.Reader.Completion);

            // Canceled
            c = CreateChannel();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.True(c.Writer.TryComplete(new OperationCanceledException(cts.Token)));
            await AssertExtensions.CanceledAsync(cts.Token, c.Reader.Completion);
        }

        [Fact]
        public void Count_ThrowsIfUnsupported()
        {
            Channel<int> c = CreateChannel();
            if (!c.Reader.CanCount)
            {
                Assert.Throws<NotSupportedException>(() => c.Reader.Count);
            }
            else
            {
                Assert.InRange(c.Reader.Count, 0, int.MaxValue);
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsThreadingSupported))]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public void SingleProducerConsumer_ConcurrentReadWrite_Success()
        {
            Channel<int> c = CreateChannel();

            const int NumItems = 100000;
            Task.WaitAll(
                Task.Run(async () =>
                {
                    for (int i = 0; i < NumItems; i++)
                    {
                        await c.Writer.WriteAsync(i);
                    }
                }),
                Task.Run(async () =>
                {
                    for (int i = 0; i < NumItems; i++)
                    {
                        Assert.Equal(i, await c.Reader.ReadAsync());
                    }
                }));
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsThreadingSupported))]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public void SingleProducerConsumer_PingPong_Success()
        {
            Channel<int> c1 = CreateChannel();
            Channel<int> c2 = CreateChannel();

            const int NumItems = 100000;
            Task.WaitAll(
                Task.Run(async () =>
                {
                    for (int i = 0; i < NumItems; i++)
                    {
                        Assert.Equal(i, await c1.Reader.ReadAsync());
                        await c2.Writer.WriteAsync(i);
                    }
                }),
                Task.Run(async () =>
                {
                    for (int i = 0; i < NumItems; i++)
                    {
                        await c1.Writer.WriteAsync(i);
                        Assert.Equal(i, await c2.Reader.ReadAsync());
                    }
                }));
        }

        [ConditionalTheory(typeof(PlatformDetection), nameof(PlatformDetection.IsThreadingSupported))]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void ManyProducerConsumer_ConcurrentReadWrite_Success(int numReaders, int numWriters)
        {
            if (RequiresSingleReader && numReaders > 1)
            {
                return;
            }

            if (RequiresSingleWriter && numWriters > 1)
            {
                return;
            }

            Channel<int> c = CreateChannel();

            const int NumItems = 10000;

            long readTotal = 0;
            int remainingWriters = numWriters;
            int remainingItems = NumItems;

            Task[] tasks = new Task[numWriters + numReaders];

            for (int i = 0; i < numReaders; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    while (await c.Reader.WaitToReadAsync())
                    {
                        while (c.Reader.TryRead(out int value))
                        {
                            Interlocked.Add(ref readTotal, value);
                        }
                    }
                });
            }

            for (int i = 0; i < numWriters; i++)
            {
                tasks[numReaders + i] = Task.Run(async () =>
                {
                    while (true)
                    {
                        int value = Interlocked.Decrement(ref remainingItems);
                        if (value < 0)
                        {
                            break;
                        }
                        await c.Writer.WriteAsync(value + 1);
                    }
                    if (Interlocked.Decrement(ref remainingWriters) == 0)
                    {
                        c.Writer.Complete();
                    }
                });
            }

            Task.WaitAll(tasks);
            Assert.Equal((NumItems * (NumItems + 1L)) / 2, readTotal);
        }

        [Fact]
        public void WaitToReadAsync_DataAvailableBefore_CompletesSynchronously()
        {
            Channel<int> c = CreateChannel();
            ValueTask write = c.Writer.WriteAsync(42);
            ValueTask<bool> read = c.Reader.WaitToReadAsync();
            Assert.True(read.IsCompletedSuccessfully);
        }

        [Fact]
        public void WaitToReadAsync_DataAvailableAfter_CompletesAsynchronously()
        {
            Channel<int> c = CreateChannel();
            ValueTask<bool> read = c.Reader.WaitToReadAsync();
            Assert.False(read.IsCompleted);
            ValueTask write = c.Writer.WriteAsync(42);
            Assert.True(read.Result);
        }

        [Fact]
        public void WaitToReadAsync_AfterComplete_SynchronouslyCompletes()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            ValueTask<bool> read = c.Reader.WaitToReadAsync();
            Assert.True(read.IsCompletedSuccessfully);
            Assert.False(read.Result);
        }

        [Fact]
        public void WaitToReadAsync_BeforeComplete_AsynchronouslyCompletes()
        {
            Channel<int> c = CreateChannel();
            ValueTask<bool> read = c.Reader.WaitToReadAsync();
            Assert.False(read.IsCompleted);
            c.Writer.Complete();
            Assert.False(read.Result);
        }

        [Fact]
        public void WaitToWriteAsync_AfterComplete_SynchronouslyCompletes()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            ValueTask<bool> write = c.Writer.WaitToWriteAsync();
            Assert.True(write.IsCompletedSuccessfully);
            Assert.False(write.Result);
        }

        [Fact]
        public void WaitToWriteAsync_EmptyChannel_SynchronouslyCompletes()
        {
            if (!BuffersItems)
            {
                return;
            }

            Channel<int> c = CreateChannel();
            ValueTask<bool> write = c.Writer.WaitToWriteAsync();
            Assert.True(write.IsCompletedSuccessfully);
            Assert.True(write.Result);
        }

        [Fact]
        public async Task WaitToWriteAsync_ManyConcurrent_SatisfiedByReaders()
        {
            if (RequiresSingleReader || RequiresSingleWriter)
            {
                return;
            }

            Channel<int> c = CreateChannel();

            Task[] writers = Enumerable.Range(0, 100).Select(_ => c.Writer.WaitToWriteAsync().AsTask()).ToArray();
            Task[] readers = Enumerable.Range(0, 100).Select(_ => c.Reader.ReadAsync().AsTask()).ToArray();

            await Task.WhenAll(writers);
        }

        [Fact]
        public void WaitToWriteAsync_BlockedReader_ReturnsTrue()
        {
            Channel<int> c = CreateChannel();
            ValueTask<int> reader = c.Reader.ReadAsync();
            AssertSynchronousSuccess(c.Writer.WaitToWriteAsync());
        }

        [Fact]
        public async Task TryRead_DataAvailable_Success()
        {
            Channel<int> c = CreateChannel();
            ValueTask write = c.Writer.WriteAsync(42);
            Assert.True(c.Reader.TryRead(out int result));
            Assert.Equal(42, result);
            await write;
        }

        [Fact]
        public void TryRead_AfterComplete_ReturnsFalse()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            Assert.False(c.Reader.TryRead(out _));
        }

        [Fact]
        public async Task TryPeek_SucceedsWhenDataAvailable()
        {
            Channel<int> c = CreateChannel();

            Assert.True(c.Reader.CanPeek); // all built-in readers support peeking

            for (int i = 0; i < 3; i++)
            {
                // Write a value
                ValueTask write = c.Writer.WriteAsync(42);

                // Can peek at the written value
                Assert.True(c.Reader.TryPeek(out int peekedResult));
                Assert.Equal(42, peekedResult);

                // Can still read out that value
                Assert.True(c.Reader.TryRead(out int readResult));
                Assert.Equal(42, readResult);

                await write;

                // Peeking no longer finds it
                Assert.False(c.Reader.TryPeek(out int noResult));
                Assert.Equal(0, noResult);
            }

            if (!BuffersItems)
            {
                return;
            }

            // Write another value
            Assert.True(c.Writer.WriteAsync(84).IsCompletedSuccessfully);

            // Mark as completed
            c.Writer.Complete();

            // Can peek at the written value
            Assert.True(c.Reader.TryPeek(out int lastPeekedResult));
            Assert.Equal(84, lastPeekedResult);

            // Can still read out that value
            Assert.True(c.Reader.TryRead(out int lastReadResult));
            Assert.Equal(84, lastReadResult);

            // Peeking no longer finds it
            Assert.False(c.Reader.TryPeek(out int lastNoResult));
            Assert.Equal(0, lastNoResult);
        }

        [Fact]
        public void TryWrite_AfterComplete_ReturnsFalse()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            Assert.False(c.Writer.TryWrite(42));
        }

        [Fact]
        public async Task WriteAsync_AfterComplete_ThrowsException()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await c.Writer.WriteAsync(42));
        }

        [Fact]
        public async Task Complete_WithException_PropagatesToCompletion()
        {
            Channel<int> c = CreateChannel();
            var exc = new FormatException();
            c.Writer.Complete(exc);
            Assert.Same(exc, await Assert.ThrowsAsync<FormatException>(() => c.Reader.Completion));
        }

        [Fact]
        public async Task Complete_WithCancellationException_PropagatesToCompletion()
        {
            Channel<int> c = CreateChannel();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Exception exc = null;
            try { cts.Token.ThrowIfCancellationRequested(); }
            catch (Exception e) { exc = e; }

            c.Writer.Complete(exc);
            await AssertExtensions.CanceledAsync(cts.Token, c.Reader.Completion);
        }

        [Fact]
        public async Task Complete_WithException_PropagatesToExistingWriter()
        {
            Channel<int> c = CreateFullChannel();
            if (c != null)
            {
                ValueTask write = c.Writer.WriteAsync(42);
                var exc = new FormatException();
                c.Writer.Complete(exc);
                Assert.Same(exc, (await Assert.ThrowsAsync<ChannelClosedException>(async () => await write)).InnerException);
            }
        }

        [Fact]
        public async Task Complete_WithException_PropagatesToNewWriter()
        {
            Channel<int> c = CreateChannel();
            var exc = new FormatException();
            c.Writer.Complete(exc);
            ValueTask write = c.Writer.WriteAsync(42);
            Assert.Same(exc, (await Assert.ThrowsAsync<ChannelClosedException>(async () => await write)).InnerException);
        }

        [Fact]
        public async Task Complete_WithException_PropagatesToExistingWaitingReader()
        {
            Channel<int> c = CreateChannel();
            ValueTask<bool> read = c.Reader.WaitToReadAsync();
            var exc = new FormatException();
            c.Writer.Complete(exc);
            await Assert.ThrowsAsync<FormatException>(async () => await read);
        }

        [Fact]
        public async Task Complete_WithException_PropagatesToNewWaitingReader()
        {
            Channel<int> c = CreateChannel();
            var exc = new FormatException();
            c.Writer.Complete(exc);
            ValueTask<bool> read = c.Reader.WaitToReadAsync();
            await Assert.ThrowsAsync<FormatException>(async () => await read);
        }

        [Fact]
        public async Task Complete_WithException_PropagatesToNewWaitingWriter()
        {
            Channel<int> c = CreateChannel();
            var exc = new FormatException();
            c.Writer.Complete(exc);
            ValueTask<bool> write = c.Writer.WaitToWriteAsync();
            await Assert.ThrowsAsync<FormatException>(async () => await write);
        }

        [Fact]
        public void ManyWriteAsync_ThenManyTryRead_Success()
        {
            if (RequiresSingleReader || RequiresSingleWriter)
            {
                return;
            }

            Channel<int> c = CreateChannel();

            const int NumItems = 2000;

            ValueTask[] writers = new ValueTask[NumItems];
            for (int i = 0; i < writers.Length; i++)
            {
                writers[i] = c.Writer.WriteAsync(i);
            }

            Task<int>[] readers = new Task<int>[NumItems];
            for (int i = 0; i < readers.Length; i++)
            {
                int result;
                Assert.True(c.Reader.TryRead(out result));
                Assert.Equal(i, result);
            }

            Assert.All(writers, w => Assert.True(w.IsCompleted));
        }

        [Fact]
        public void Precancellation_Writing_ReturnsImmediately()
        {
            Channel<int> c = CreateChannel();

            ValueTask writeTask = c.Writer.WriteAsync(42, new CancellationToken(true));
            Assert.True(writeTask.IsCanceled);

            ValueTask<bool> waitTask = c.Writer.WaitToWriteAsync(new CancellationToken(true));
            Assert.True(waitTask.IsCanceled);
        }

        [Fact]
        public void Write_WaitToReadAsync_CompletesSynchronously()
        {
            Channel<int> c = CreateChannel();
            c.Writer.WriteAsync(42);
            AssertSynchronousTrue(c.Reader.WaitToReadAsync());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Precancellation_WaitToReadAsync_ReturnsImmediately(bool dataAvailable)
        {
            Channel<int> c = CreateChannel();

            ValueTask write = default;
            if (dataAvailable)
            {
                write = c.Writer.WriteAsync(42);
            }

            ValueTask<bool> waitTask = c.Reader.WaitToReadAsync(new CancellationToken(true));
            Assert.True(waitTask.IsCanceled);

            if (BuffersItems)
            {
                await write;
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task WaitToReadAsync_DataWritten_CompletesSuccessfully(bool cancelable)
        {
            Channel<int> c = CreateChannel();
            CancellationToken token = cancelable ? new CancellationTokenSource().Token : default;

            ValueTask<bool> read = c.Reader.WaitToReadAsync(token);
            Assert.False(read.IsCompleted);

            ValueTask write = c.Writer.WriteAsync(42, token);

            Assert.True(await read);
        }

        [Fact]
        public async Task WaitToReadAsync_NoDataWritten_Canceled_CompletesAsCanceled()
        {
            Channel<int> c = CreateChannel();
            var cts = new CancellationTokenSource();

            ValueTask<bool> read = c.Reader.WaitToReadAsync(cts.Token);
            Assert.False(read.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await read);
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public async Task ReadAsync_ThenWriteAsync_Succeeds()
        {
            Channel<int> c = CreateChannel();

            ValueTask<int> r = c.Reader.ReadAsync();
            Assert.False(r.IsCompleted);

            ValueTask w = c.Writer.WriteAsync(42);
            AssertSynchronousSuccess(w);

            Assert.Equal(42, await r);
        }

        [Fact]
        public async Task WriteAsync_ReadAsync_Succeeds()
        {
            Channel<int> c = CreateChannel();

            ValueTask w = c.Writer.WriteAsync(42);
            ValueTask<int> r = c.Reader.ReadAsync();

            await Task.WhenAll(w.AsTask(), r.AsTask());

            Assert.Equal(42, await r);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Precancellation_ReadAsync_ReturnsImmediately(bool dataAvailable)
        {
            Channel<int> c = CreateChannel();
            if (dataAvailable)
            {
                c.Writer.WriteAsync(42);
            }

            ValueTask<int> readTask = c.Reader.ReadAsync(new CancellationToken(true));
            Assert.True(readTask.IsCanceled);
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public async Task ReadAsync_Canceled_CanceledAsynchronously()
        {
            Channel<int> c = CreateChannel();
            var cts = new CancellationTokenSource();

            ValueTask<int> r = c.Reader.ReadAsync(cts.Token);
            Assert.False(r.IsCompleted);

            cts.Cancel();

            await AssertExtensions.CanceledAsync(cts.Token, async () => await r);

            ValueTask vt = c.Writer.WriteAsync(42);
            Assert.Equal(42, await c.Reader.ReadAsync());
            await vt;
        }

        [Fact]
        public async Task ReadAsync_WriteAsync_ManyConcurrentReaders_SerializedWriters_Success()
        {
            if (RequiresSingleReader)
            {
                return;
            }

            Channel<int> c = CreateChannel();
            const int Items = 100;

            ValueTask<int>[] readers = (from i in Enumerable.Range(0, Items) select c.Reader.ReadAsync()).ToArray();
            for (int i = 0; i < Items; i++)
            {
                await c.Writer.WriteAsync(i);
            }

            Assert.Equal((Items * (Items - 1)) / 2, Enumerable.Sum(await Task.WhenAll(readers.Select(r => r.AsTask()))));
        }

        [Fact]
        public async Task ReadAsync_TryWrite_ManyConcurrentReaders_SerializedWriters_Success()
        {
            if (RequiresSingleReader)
            {
                return;
            }

            Channel<int> c = CreateChannel();
            const int Items = 100;

            Task<int>[] readers = (from i in Enumerable.Range(0, Items) select c.Reader.ReadAsync().AsTask()).ToArray();
            var remainingReaders = new List<Task<int>>(readers);

            for (int i = 0; i < Items; i++)
            {
                Assert.True(c.Writer.TryWrite(i), $"Failed to write at {i}");
                Task<int> r = await Task.WhenAny(remainingReaders);
                await r;
                remainingReaders.Remove(r);
            }

            Assert.Equal((Items * (Items - 1)) / 2, Enumerable.Sum(await Task.WhenAll(readers)));
        }

        [Fact]
        public async Task ReadAsync_AlreadyCompleted_Throws()
        {
            Channel<int> c = CreateChannel();
            c.Writer.Complete();
            await Assert.ThrowsAsync<ChannelClosedException>(() => c.Reader.ReadAsync().AsTask());
        }

        [Fact]
        public async Task ReadAsync_SubsequentlyCompleted_Throws()
        {
            Channel<int> c = CreateChannel();
            Task<int> r = c.Reader.ReadAsync().AsTask();
            Assert.False(r.IsCompleted);
            c.Writer.Complete();
            await Assert.ThrowsAsync<ChannelClosedException>(() => r);
        }

        [Fact]
        public async Task ReadAsync_AfterFaultedChannel_Throws()
        {
            Channel<int> c = CreateChannel();

            var e = new FormatException();
            c.Writer.Complete(e);
            Assert.True(c.Reader.Completion.IsFaulted);

            ChannelClosedException cce = await Assert.ThrowsAsync<ChannelClosedException>(() => c.Reader.ReadAsync().AsTask());
            Assert.Same(e, cce.InnerException);
        }

        [Fact]
        public async Task ReadAsync_AfterCanceledChannel_Throws()
        {
            Channel<int> c = CreateChannel();

            var e = new OperationCanceledException();
            c.Writer.Complete(e);
            Assert.True(c.Reader.Completion.IsCanceled);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => c.Reader.ReadAsync().AsTask());
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public async Task ReadAsync_Canceled_WriteAsyncCompletesNextReader()
        {
            Channel<int> c = CreateChannel();

            for (int i = 0; i < 5; i++)
            {
                var cts = new CancellationTokenSource();
                ValueTask<int> r = c.Reader.ReadAsync(cts.Token);
                cts.Cancel();
                await AssertExtensions.CanceledAsync(cts.Token, async () => await r);
            }

            for (int i = 0; i < 7; i++)
            {
                ValueTask<int> r = c.Reader.ReadAsync();
                await c.Writer.WriteAsync(i);
                Assert.Equal(i, await r);
            }
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public async Task ReadAsync_ConsecutiveReadsSucceed()
        {
            Channel<int> c = CreateChannel();
            for (int i = 0; i < 5; i++)
            {
                ValueTask<int> r = c.Reader.ReadAsync();
                await c.Writer.WriteAsync(i);
                Assert.Equal(i, await r);
            }
        }

        [Fact]
        public void ReadAllAsync_NotIdempotent()
        {
            Channel<int> c = CreateChannel();
            IAsyncEnumerable<int> e = c.Reader.ReadAllAsync();
            Assert.NotNull(e);
            Assert.NotSame(e, c.Reader.ReadAllAsync());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ReadAllAsync_UseMoveNextAsyncAfterCompleted_ReturnsFalse(bool completeWhilePending)
        {
            Channel<int> c = CreateChannel();
            IAsyncEnumerator<int> e = c.Reader.ReadAllAsync().GetAsyncEnumerator();

            ValueTask<bool> vt;
            if (completeWhilePending)
            {
                c.Writer.Complete();
                vt = e.MoveNextAsync();
                Assert.True(vt.IsCompletedSuccessfully);
                Assert.False(vt.Result);
            }
            else
            {
                vt = e.MoveNextAsync();
                Assert.False(vt.IsCompleted);
                c.Writer.Complete();
                Assert.False(await vt);
            }

            vt = e.MoveNextAsync();
            Assert.True(vt.IsCompletedSuccessfully);
            Assert.False(vt.Result);
        }

        [Fact]
        public async Task ReadAllAsync_AvailableDataCompletesSynchronously()
        {
            Channel<int> c = CreateChannel();

            IAsyncEnumerator<int> e = c.Reader.ReadAllAsync().GetAsyncEnumerator();
            try
            {
                for (int i = 100; i < 110; i++)
                {
                    ValueTask write = c.Writer.WriteAsync(i);
                    ValueTask<bool> vt = e.MoveNextAsync();
                    Assert.True(vt.IsCompletedSuccessfully);
                    Assert.True(vt.Result);
                    Assert.Equal(i, e.Current);
                    await write;
                }
            }
            finally
            {
                ValueTask vt = e.DisposeAsync();
                Assert.True(vt.IsCompletedSuccessfully);
                vt.GetAwaiter().GetResult();
            }
        }

        [Fact]
        public async Task ReadAllAsync_UnavailableDataCompletesAsynchronously()
        {
            Channel<int> c = CreateChannel();

            IAsyncEnumerator<int> e = c.Reader.ReadAllAsync().GetAsyncEnumerator();
            try
            {
                for (int i = 100; i < 110; i++)
                {
                    ValueTask<bool> vt = e.MoveNextAsync();
                    Assert.False(vt.IsCompleted);
                    Task producer = Task.Run(() => c.Writer.WriteAsync(i).AsTask());
                    Assert.True(await vt);
                    await producer;
                    Assert.Equal(i, e.Current);
                }
            }
            finally
            {
                ValueTask vt = e.DisposeAsync();
                Assert.True(vt.IsCompletedSuccessfully);
                vt.GetAwaiter().GetResult();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(128)]
        public async Task ReadAllAsync_ProducerConsumer_ConsumesAllData(int items)
        {
            Channel<int> c = CreateChannel();

            int producedTotal = 0, consumedTotal = 0;
            await Task.WhenAll(
                Task.Run(async () =>
                {
                    for (int i = 0; i < items; i++)
                    {
                        await c.Writer.WriteAsync(i);
                        producedTotal += i;
                    }
                    c.Writer.Complete();
                }),
                Task.Run(async () =>
                {
                    IAsyncEnumerator<int> e = c.Reader.ReadAllAsync().GetAsyncEnumerator();
                    try
                    {
                        while (await e.MoveNextAsync())
                        {
                            consumedTotal += e.Current;
                        }
                    }
                    finally
                    {
                        await e.DisposeAsync();
                    }
                }));

            Assert.Equal(producedTotal, consumedTotal);
        }

        [Fact]
        public async Task ReadAllAsync_MultipleEnumerationsToEnd()
        {
            Channel<int> c = CreateChannel();

            ValueTask write = c.Writer.WriteAsync(42);

            IAsyncEnumerable<int> enumerable = c.Reader.ReadAllAsync();
            IAsyncEnumerator<int> e = enumerable.GetAsyncEnumerator();

            Assert.True(await e.MoveNextAsync());
            Assert.Equal(42, e.Current);

            await write;
            c.Writer.Complete();

            Assert.False(await e.MoveNextAsync());
            Assert.False(await e.MoveNextAsync());

            await e.DisposeAsync();

            e = enumerable.GetAsyncEnumerator();
            Assert.Same(enumerable, e);

            Assert.False(await e.MoveNextAsync());
            Assert.False(await e.MoveNextAsync());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ReadAllAsync_MultipleSingleElementEnumerations_AllItemsEnumerated(bool sameEnumerable, bool dispose)
        {
            Channel<int> c = CreateChannel();
            IAsyncEnumerable<int> enumerable = c.Reader.ReadAllAsync();

            for (int i = 0; i < 10; i++)
            {
                ValueTask write = c.Writer.WriteAsync(i);
                IAsyncEnumerator<int> e = (sameEnumerable ? enumerable : c.Reader.ReadAllAsync()).GetAsyncEnumerator();
                ValueTask<bool> vt = e.MoveNextAsync();
                await write;
                Assert.True(vt.IsCompletedSuccessfully);
                Assert.True(vt.Result);
                Assert.Equal(i, e.Current);
                if (dispose)
                {
                    ValueTask dvt = e.DisposeAsync();
                    Assert.True(dvt.IsCompletedSuccessfully);
                    dvt.GetAwaiter().GetResult();
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ReadAllAsync_DualConcurrentEnumeration_AllItemsEnumerated(bool sameEnumerable)
        {
            if (RequiresSingleReader)
            {
                return;
            }

            Channel<int> c = CreateChannel();

            IAsyncEnumerable<int> enumerable = c.Reader.ReadAllAsync();

            IAsyncEnumerator<int> e1 = enumerable.GetAsyncEnumerator();
            IAsyncEnumerator<int> e2 = (sameEnumerable ? enumerable : c.Reader.ReadAllAsync()).GetAsyncEnumerator();
            Assert.NotSame(e1, e2);

            ValueTask<bool> vt1, vt2;
            int producerTotal = 0, consumerTotal = 0;
            for (int i = 0; i < 10; i++)
            {
                vt1 = e1.MoveNextAsync();
                vt2 = e2.MoveNextAsync();

                await c.Writer.WriteAsync(i);
                producerTotal += i;
                await c.Writer.WriteAsync(i * 2);
                producerTotal += i * 2;

                Assert.True(await vt1);
                Assert.True(await vt2);
                consumerTotal += e1.Current;
                consumerTotal += e2.Current;
            }

            vt1 = e1.MoveNextAsync();
            vt2 = e2.MoveNextAsync();
            c.Writer.Complete();
            Assert.False(await vt1);
            Assert.False(await vt2);

            Assert.Equal(producerTotal, consumerTotal);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ReadAllAsync_CanceledBeforeMoveNextAsync_Throws(bool dataAvailable)
        {
            Channel<int> c = CreateChannel();

            if (dataAvailable)
            {
                _ = c.Writer.WriteAsync(42);
            }

            var cts = new CancellationTokenSource();
            cts.Cancel();

            IAsyncEnumerator<int> e = c.Reader.ReadAllAsync(cts.Token).GetAsyncEnumerator();
            ValueTask<bool> vt = e.MoveNextAsync();
            Assert.True(vt.IsCompleted);
            Assert.False(vt.IsCompletedSuccessfully);
            OperationCanceledException oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await vt);
            Assert.Equal(cts.Token, oce.CancellationToken);
        }

        [Fact]
        public async Task ReadAllAsync_CanceledAfterMoveNextAsync_Throws()
        {
            Channel<int> c = CreateChannel();
            var cts = new CancellationTokenSource();

            IAsyncEnumerator<int> e = c.Reader.ReadAllAsync(cts.Token).GetAsyncEnumerator();
            ValueTask<bool> vt = e.MoveNextAsync();
            Assert.False(vt.IsCompleted);

            cts.Cancel();
            OperationCanceledException oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await vt);
            Assert.Equal(cts.Token, oce.CancellationToken);

            vt = e.MoveNextAsync();
            Assert.True(vt.IsCompletedSuccessfully);
            Assert.False(vt.Result);
        }

        [Fact]
        public async Task WaitToReadAsync_ConsecutiveReadsSucceed()
        {
            Channel<int> c = CreateChannel();
            for (int i = 0; i < 5; i++)
            {
                ValueTask<bool> r = c.Reader.WaitToReadAsync();
                ValueTask write = c.Writer.WriteAsync(i);
                Assert.True(await r);
                Assert.True(c.Reader.TryRead(out int item));
                Assert.Equal(i, item);
                await write;
            }
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, null)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void WaitToReadAsync_MultipleContinuations_Throws(bool onCompleted, bool? continueOnCapturedContext)
        {
            Channel<int> c = CreateChannel();

            ValueTask<bool> read = c.Reader.WaitToReadAsync();
            switch (continueOnCapturedContext)
            {
                case null:
                    if (onCompleted)
                    {
                        read.GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        read.GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;

                default:
                    if (onCompleted)
                    {
                        read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;
            }
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, null)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void ReadAsync_MultipleContinuations_Throws(bool onCompleted, bool? continueOnCapturedContext)
        {
            Channel<int> c = CreateChannel();

            ValueTask<int> read = c.Reader.ReadAsync();
            switch (continueOnCapturedContext)
            {
                case null:
                    if (onCompleted)
                    {
                        read.GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        read.GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;

                default:
                    if (onCompleted)
                    {
                        read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => read.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;
            }
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public async Task WaitToReadAsync_AwaitThenGetResult_Throws()
        {
            Channel<int> c = CreateChannel();

            ValueTask<bool> read = c.Reader.WaitToReadAsync();

            ValueTask write = c.Writer.WriteAsync(42);
            Assert.True(await read);
            Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().IsCompleted);
            Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().OnCompleted(() => { }));
            Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().GetResult());
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/60472", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public async Task ReadAsync_AwaitThenGetResult_Throws()
        {
            Channel<int> c = CreateChannel();

            ValueTask<int> read = c.Reader.ReadAsync();
            ValueTask write = c.Writer.WriteAsync(42);
            Assert.Equal(42, await read);
            await write;
            Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().IsCompleted);
            Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().OnCompleted(() => { }));
            Assert.Throws<InvalidOperationException>(() => read.GetAwaiter().GetResult());
        }

        [Fact]
        public async Task WaitToWriteAsync_AwaitThenGetResult_Throws()
        {
            Channel<int> c = CreateFullChannel();
            if (c == null)
            {
                return;
            }

            ValueTask<bool> write = c.Writer.WaitToWriteAsync();
            ValueTask<int> read = c.Reader.ReadAsync();
            Assert.True(await write);
            Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().IsCompleted);
            Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().OnCompleted(() => { }));
            Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().GetResult());
        }

        [Fact]
        public async Task WriteAsync_AwaitThenGetResult_Throws()
        {
            Channel<int> c = CreateFullChannel();
            if (c == null)
            {
                return;
            }

            ValueTask write = c.Writer.WriteAsync(42);
            await c.Reader.ReadAsync();
            await write;
            Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().IsCompleted);
            Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().OnCompleted(() => { }));
            Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().GetResult());
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, null)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void WaitToWriteAsync_MultipleContinuations_Throws(bool onCompleted, bool? continueOnCapturedContext)
        {
            Channel<int> c = CreateFullChannel();
            if (c == null)
            {
                return;
            }

            ValueTask<bool> write = c.Writer.WaitToWriteAsync();
            switch (continueOnCapturedContext)
            {
                case null:
                    if (onCompleted)
                    {
                        write.GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        write.GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;

                default:
                    if (onCompleted)
                    {
                        write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;
            }
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, null)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void WriteAsync_MultipleContinuations_Throws(bool onCompleted, bool? continueOnCapturedContext)
        {
            Channel<int> c = CreateFullChannel();
            if (c == null)
            {
                return;
            }

            ValueTask write = c.Writer.WriteAsync(42);
            switch (continueOnCapturedContext)
            {
                case null:
                    if (onCompleted)
                    {
                        write.GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        write.GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;

                default:
                    if (onCompleted)
                    {
                        write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(() => { }));
                    }
                    else
                    {
                        write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { });
                        Assert.Throws<InvalidOperationException>(() => write.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(() => { }));
                    }
                    break;
            }
        }

        public static IEnumerable<object[]> Reader_ContinuesOnCurrentContextIfDesired_MemberData() =>
            from readOrWait in new[] { true, false }
            from completeBeforeOnCompleted in new[] { true, false }
            from flowExecutionContext in new[] { true, false }
            from continueOnCapturedContext in new bool?[] { null, false, true }
            from setNonDefaultTaskScheduler in new[] { true, false }
            select new object[] { readOrWait, completeBeforeOnCompleted, flowExecutionContext, continueOnCapturedContext, setNonDefaultTaskScheduler };

        [Theory]
        [MemberData(nameof(Reader_ContinuesOnCurrentContextIfDesired_MemberData))]
        public async Task Reader_ContinuesOnCurrentSynchronizationContextIfDesired(
            bool readOrWait, bool completeBeforeOnCompleted, bool flowExecutionContext, bool? continueOnCapturedContext, bool setNonDefaultTaskScheduler)
        {
            if (AllowSynchronousContinuations)
            {
                return;
            }

            await Task.Factory.StartNew(async () =>
            {
                Assert.Null(SynchronizationContext.Current);

                Channel<bool> c = CreateChannel<bool>();
                ValueTask<bool> vt = readOrWait ?
                    c.Reader.ReadAsync() :
                    c.Reader.WaitToReadAsync();

                var continuationRan = new TaskCompletionSource<bool>();
                var asyncLocal = new AsyncLocal<int>();
                bool schedulerWasFlowed = false;
                bool executionContextWasFlowed = false;
                Action continuation = () =>
                {
                    schedulerWasFlowed = SynchronizationContext.Current is CustomSynchronizationContext;
                    executionContextWasFlowed = 42 == asyncLocal.Value;
                    continuationRan.SetResult(true);
                };

                if (completeBeforeOnCompleted)
                {
                    Assert.False(vt.IsCompleted);
                    Assert.False(vt.IsCompletedSuccessfully);
                    _ = c.Writer.WriteAsync(true);
                }

                SynchronizationContext.SetSynchronizationContext(new CustomSynchronizationContext());
                asyncLocal.Value = 42;
                switch (continueOnCapturedContext)
                {
                    case null:
                        if (flowExecutionContext)
                        {
                            vt.GetAwaiter().OnCompleted(continuation);
                        }
                        else
                        {
                            vt.GetAwaiter().UnsafeOnCompleted(continuation);
                        }
                        break;
                    default:
                        if (flowExecutionContext)
                        {
                            vt.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(continuation);
                        }
                        else
                        {
                            vt.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(continuation);
                        }
                        break;
                }
                asyncLocal.Value = 0;
                SynchronizationContext.SetSynchronizationContext(null);

                if (!completeBeforeOnCompleted)
                {
                    Assert.False(vt.IsCompleted);
                    Assert.False(vt.IsCompletedSuccessfully);
                    _ = c.Writer.WriteAsync(true);
                }

                await continuationRan.Task;
                Assert.True(vt.IsCompleted);
                Assert.True(vt.IsCompletedSuccessfully);

                Assert.Equal(continueOnCapturedContext != false, schedulerWasFlowed);
                if (completeBeforeOnCompleted) // OnCompleted will simply queue using a mechanism that happens to flow
                {
                    Assert.True(executionContextWasFlowed);
                }
                else
                {
                    Assert.Equal(flowExecutionContext, executionContextWasFlowed);
                }
            }, CancellationToken.None, TaskCreationOptions.None, setNonDefaultTaskScheduler ? new CustomTaskScheduler() : TaskScheduler.Default);
        }

        public static IEnumerable<object[]> Reader_ContinuesOnCurrentSchedulerIfDesired_MemberData() =>
            from readOrWait in new[] { true, false }
            from completeBeforeOnCompleted in new[] { true, false }
            from flowExecutionContext in new[] { true, false }
            from continueOnCapturedContext in new bool?[] { null, false, true }
            from setDefaultSyncContext in new[] { true, false }
            select new object[] { readOrWait, completeBeforeOnCompleted, flowExecutionContext, continueOnCapturedContext, setDefaultSyncContext };

        [Theory]
        [MemberData(nameof(Reader_ContinuesOnCurrentSchedulerIfDesired_MemberData))]
        public async Task Reader_ContinuesOnCurrentTaskSchedulerIfDesired(
            bool readOrWait, bool completeBeforeOnCompleted, bool flowExecutionContext, bool? continueOnCapturedContext, bool setDefaultSyncContext)
        {
            if (AllowSynchronousContinuations)
            {
                return;
            }

            await Task.Run(async () =>
            {
                Assert.Null(SynchronizationContext.Current);

                Channel<bool> c = CreateChannel<bool>();
                ValueTask<bool> vt = readOrWait ?
                    c.Reader.ReadAsync() :
                    c.Reader.WaitToReadAsync();

                var continuationRan = new TaskCompletionSource<bool>();
                var asyncLocal = new AsyncLocal<int>();
                bool schedulerWasFlowed = false;
                bool executionContextWasFlowed = false;
                Action continuation = () =>
                {
                    schedulerWasFlowed = TaskScheduler.Current is CustomTaskScheduler;
                    executionContextWasFlowed = 42 == asyncLocal.Value;
                    continuationRan.SetResult(true);
                };

                if (completeBeforeOnCompleted)
                {
                    Assert.False(vt.IsCompleted);
                    Assert.False(vt.IsCompletedSuccessfully);
                    _ = c.Writer.WriteAsync(true);
                }

                await Task.Factory.StartNew(() =>
                {
                    if (setDefaultSyncContext)
                    {
                        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                    }

                    Assert.IsType<CustomTaskScheduler>(TaskScheduler.Current);
                    asyncLocal.Value = 42;
                    switch (continueOnCapturedContext)
                    {
                        case null:
                            if (flowExecutionContext)
                            {
                                vt.GetAwaiter().OnCompleted(continuation);
                            }
                            else
                            {
                                vt.GetAwaiter().UnsafeOnCompleted(continuation);
                            }
                            break;
                        default:
                            if (flowExecutionContext)
                            {
                                vt.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().OnCompleted(continuation);
                            }
                            else
                            {
                                vt.ConfigureAwait(continueOnCapturedContext.Value).GetAwaiter().UnsafeOnCompleted(continuation);
                            }
                            break;
                    }
                    asyncLocal.Value = 0;
                }, CancellationToken.None, TaskCreationOptions.None, new CustomTaskScheduler());

                if (!completeBeforeOnCompleted)
                {
                    Assert.False(vt.IsCompleted);
                    Assert.False(vt.IsCompletedSuccessfully);
                    _ = c.Writer.WriteAsync(true);
                }

                await continuationRan.Task;
                Assert.True(vt.IsCompleted);
                Assert.True(vt.IsCompletedSuccessfully);

                Assert.Equal(continueOnCapturedContext != false, schedulerWasFlowed);
                if (completeBeforeOnCompleted)
                {
                    // OnCompleted may or may not flow ExecutionContext here; it's not needed,
                    // and we avoid it when it's easy, but it's also not wrong to do so.
                }
                else
                {
                    Assert.Equal(flowExecutionContext, executionContextWasFlowed);
                }
            });
        }

        [Fact]
        public void ValueTask_GetResultWhenNotCompleted_Throws()
        {
            ValueTaskAwaiter<int> readVt = CreateChannel().Reader.ReadAsync().GetAwaiter();
            Assert.Throws<InvalidOperationException>(() => readVt.GetResult());

            ValueTaskAwaiter<bool> waitReadVt = CreateChannel().Reader.WaitToReadAsync().GetAwaiter();
            Assert.Throws<InvalidOperationException>(() => waitReadVt.GetResult());

            if (CreateFullChannel() != null)
            {
                ValueTaskAwaiter writeVt = CreateFullChannel().Writer.WriteAsync(42).GetAwaiter();
                Assert.Throws<InvalidOperationException>(() => writeVt.GetResult());

                ValueTaskAwaiter<bool> waitWriteVt = CreateFullChannel().Writer.WaitToWriteAsync().GetAwaiter();
                Assert.Throws<InvalidOperationException>(() => waitWriteVt.GetResult());
            }
        }

        [Fact]
        public void ValueTask_MultipleContinuations_Throws()
        {
            ValueTaskAwaiter<int> readVt = CreateChannel().Reader.ReadAsync().GetAwaiter();
            readVt.OnCompleted(() => { });
            Assert.Throws<InvalidOperationException>(() => readVt.OnCompleted(() => { }));

            ValueTaskAwaiter<bool> waitReadVt = CreateChannel().Reader.WaitToReadAsync().GetAwaiter();
            waitReadVt.OnCompleted(() => { });
            Assert.Throws<InvalidOperationException>(() => waitReadVt.OnCompleted(() => { }));

            if (CreateFullChannel() != null)
            {
                ValueTaskAwaiter writeVt = CreateFullChannel().Writer.WriteAsync(42).GetAwaiter();
                writeVt.OnCompleted(() => { });
                Assert.Throws<InvalidOperationException>(() => writeVt.OnCompleted(() => { }));

                ValueTaskAwaiter<bool> waitWriteVt = CreateFullChannel().Writer.WaitToWriteAsync().GetAwaiter();
                waitWriteVt.OnCompleted(() => { });
                Assert.Throws<InvalidOperationException>(() => waitWriteVt.OnCompleted(() => { }));
            }
        }

        private sealed class CustomSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    SetSynchronizationContext(this);
                    try
                    {
                        d(state);
                    }
                    finally
                    {
                        SetSynchronizationContext(null);
                    }
                }, null);
            }
        }

        private sealed class CustomTaskScheduler : TaskScheduler
        {
            protected override void QueueTask(Task task) => ThreadPool.QueueUserWorkItem(_ => TryExecuteTask(task));
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
            protected override IEnumerable<Task> GetScheduledTasks() => null;
        }
    }
}
