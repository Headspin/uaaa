using System;
using System.Threading.Tasks;
using Xunit;

namespace Uaaa.Core.Tests
{
    public class AmbientOperationTests
    {
        public class Work
        {
            public class DoWorkOperation : AmbientOperation<Work>
            {
                public DoWorkOperation(Work context) : base(context) { }
            }
            public EventHandler<string> Processing;
            public EventHandler<string> WorkFinished;

            public Task DoWorkAsync(string data)
            {
                return Task.Run(() => {
                    if (!DoWorkOperation.IsRunning<DoWorkOperation>(this))
                        DoWork(data);
                });
            }
            public void DoWork(string data)
            {
                using (var operation = new DoWorkOperation(this))
                {
                    if (operation.IsPrimary)
                    {
                        for (var idx = 0; idx < 10; idx++)
                            OnProcessing(data);
                    }
                    else
                        operation.WaitPrimary<DoWorkOperation>();
                    OnWorkFinished(data);
                }
            }
            private void OnProcessing(string data) => Processing?.Invoke(this, data);

            private void OnWorkFinished(string data) => WorkFinished?.Invoke(this, data);
        }
        [Fact]
        public void AmbientOperation_DoWork()
        {
            var some = new Work();
            var processingCount = 0;
            some.Processing += (sender, args) => {
                Assert.Equal("key1", args);
                processingCount++;
            };
            some.WorkFinished += (sender, args) => {
                Assert.Equal("key1", args);
            };
            some.DoWork("key1");
            Assert.Equal(10, processingCount);
        }
        [Fact]
        public void AmbientOperation_DoWorkAsync()
        {
            var some = new Work();
            var processingCount = 0;
            some.Processing += (sender, args) => {
                Assert.Equal("key1", args);
                processingCount++;
            };
            some.WorkFinished += (sender, args) => {
                Assert.Equal("key1", args);
            };
            some.DoWorkAsync("key1");
            Assert.Equal(0, processingCount); // async call -> no processing should occur.
            // get operation (wait until it gets created)
            AmbientOperation<Work> operation = Work.DoWorkOperation.GetOperation<Work.DoWorkOperation>(some);
            while (operation == null && processingCount == 0)
                operation = Work.DoWorkOperation.GetOperation<Work.DoWorkOperation>(some);
            operation?.Finished.WaitOne();
            // check processing
            Assert.Equal(10, processingCount);
        }

        [Fact]
        public void AmbientOperation_GetCount()
        {
            var work1 = new Work();
            var work2 = new Work();
            Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
            Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
            using (new Work.DoWorkOperation(work1))
            {
                Assert.Equal(1, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                using (new Work.DoWorkOperation(work1))
                {
                    Assert.Equal(2, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                    Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                    using (new Work.DoWorkOperation(work1))
                    {
                        Assert.Equal(3, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                        Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                        using (new Work.DoWorkOperation(work2))
                        {
                            Assert.Equal(3, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                            Assert.Equal(1, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                        }
                        Assert.Equal(3, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                        Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                    }
                    Assert.Equal(2, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                    Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                }
                Assert.Equal(1, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
            }
            Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
            Assert.Equal(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
        }


        internal sealed class AmbientOperationA : AmbientOperation<int>
        {
            public AmbientOperationA(int context) : base(context) { }
        }
        internal sealed class AmbientOperationB : AmbientOperation<int>
        {
            public AmbientOperationB(int context) : base(context) { }
        }


        [Fact]
        public void AmbientOperation_IsRunning_OnSameContext()
        {
            const int context1 = 0;
            const int context2 = 1;
            using (var opA = new AmbientOperationA(context1))
            {
                Assert.True(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
                Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
                Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
                Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
            }

            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
        }

        [Fact]
        public void AmbientOperation_MultipleOperations_IsRunning_OnSameContext()
        {
            const int context1 = 0;
            const int context2 = 1;

            using (var opA = new AmbientOperationA(context1))
            {
                using (var opB = new AmbientOperationB(context1))
                {
                    Assert.True(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
                    Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
                    Assert.True(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
                    Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
                }
                Assert.True(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
                Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
                Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
                Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
            }

            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
            Assert.False(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
        }
    }
}
