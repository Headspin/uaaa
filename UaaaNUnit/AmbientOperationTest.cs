using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uaaa;

namespace UaaaNUnit
{
    [TestFixture]
    public class AmbientOperationTest
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
                using (DoWorkOperation operation = new DoWorkOperation(this))
                {
                    if (operation.IsPrimary)
                    {
                        for (int idx = 0; idx < 10; idx++)
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
        [Test()]
        public void AmbientOperation_DoWork()
        {
            Work some = new Work();
            int processingCount = 0;
            some.Processing += (sender, args) => {
                Assert.AreEqual("key1", args);
                processingCount++;
            };
            some.WorkFinished += (sender, args) => {
                Assert.AreEqual("key1", args);
            };
            some.DoWork("key1");
            Assert.AreEqual(10, processingCount);
        }
        [Test()]
        public void AmbientOperation_DoWorkAsync()
        {
            Work some = new Work();
            int processingCount = 0;
            some.Processing += (sender, args) => {
                Assert.AreEqual("key1", args);
                processingCount++;
            };
            some.WorkFinished += (sender, args) => {
                Assert.AreEqual("key1", args);
            };
            some.DoWorkAsync("key1");
            Assert.AreEqual(0, processingCount); // async call -> no processing should occur.
            // get operation (wait until it gets created)
            AmbientOperation<Work> operation = Work.DoWorkOperation.GetOperation<Work.DoWorkOperation>(some);
            while (operation == null && processingCount == 0)
                operation = Work.DoWorkOperation.GetOperation<Work.DoWorkOperation>(some);
            operation?.Finished.WaitOne();
            // check processing
            Assert.AreEqual(10, processingCount);
        }

        [Test()]
        public void AmbientOperation_GetCount()
        {
            Work work1 = new Work();
            Work work2 = new Work();
            Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
            Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
            using (new Work.DoWorkOperation(work1))
            {
                Assert.AreEqual(1, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                using (new Work.DoWorkOperation(work1))
                {
                    Assert.AreEqual(2, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                    Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                    using (new Work.DoWorkOperation(work1))
                    {
                        Assert.AreEqual(3, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                        Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                        using (new Work.DoWorkOperation(work2))
                        {
                            Assert.AreEqual(3, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                            Assert.AreEqual(1, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                        }
                        Assert.AreEqual(3, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                        Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                    }
                    Assert.AreEqual(2, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                    Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
                }
                Assert.AreEqual(1, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
                Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
            }
            Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work1));
            Assert.AreEqual(0, Work.DoWorkOperation.GetCount<Work.DoWorkOperation>(work2));
        }


        internal sealed class AmbientOperationA : AmbientOperation<int>
        {
            public AmbientOperationA(int context) : base(context) { }
        }
        internal sealed class AmbientOperationB : AmbientOperation<int>
        {
            public AmbientOperationB(int context) : base(context) { }
        }


        [Test()]
        public void AmbientOperation_IsRunning_OnSameContext()
        {
            const int context1 = 0;
            const int context2 = 1;
            using (AmbientOperationA opA = new AmbientOperationA(context1))
            {
                Assert.IsTrue(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
                Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
                Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
                Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
            }

            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
        }

        [Test()]
        public void AmbientOperation_MultipleOperations_IsRunning_OnSameContext()
        {
            const int context1 = 0;
            const int context2 = 1;

            using (AmbientOperationA opA = new AmbientOperationA(context1))
            {
                using (AmbientOperationB opB = new AmbientOperationB(context1))
                {
                    Assert.IsTrue(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
                    Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
                    Assert.IsTrue(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
                    Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
                }
                Assert.IsTrue(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
                Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
                Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
                Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
            }

            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationA>(context1));
            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationA>(context2));
            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context1));
            Assert.IsFalse(AmbientOperation<int>.IsRunning<AmbientOperationB>(context2));
        }
    }
}
