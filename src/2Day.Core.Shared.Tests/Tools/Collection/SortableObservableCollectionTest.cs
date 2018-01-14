using Chartreuse.Today.Core.Shared.Tools.Collection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Collection
{
    [TestClass]
    public class SortableObservableCollectionTest
    {
        private SortableObservableCollection<int> c;

        [TestInitialize]
        public void Setup()
        {
            this.c = new SortableObservableCollection<int>
                {
                    5,
                    4,
                    3,
                    2,
                    1
                };
        }

        [TestMethod]
        public void CollectionIsSorted()
        {
            var g = new GenericComparer<int>((a, b) => a < b ? -1 : a == b ? 0 : 1);

            this.c.Sort(g);

            Assert.AreEqual(1, this.c[0]);
            Assert.AreEqual(2, this.c[1]);
            Assert.AreEqual(3, this.c[2]);
            Assert.AreEqual(4, this.c[3]);
            Assert.AreEqual(5, this.c[4]);
        }

        [TestMethod]
        public void ChangesAreCorrect()
        {
            var g = new GenericComparer<int>((a, b) => a < b ? -1 : a == b ? 0 : 1);

            var o = this.c.Sort(g);

            Assert.IsTrue(o.ContainsKey(0));
            Assert.IsTrue(o.ContainsKey(1));
            Assert.IsTrue(o.ContainsKey(3));
            Assert.IsTrue(o.ContainsKey(4));
            Assert.AreEqual(4, o[0]);
            Assert.AreEqual(3, o[1]);
            Assert.AreEqual(1, o[3]);
            Assert.AreEqual(0, o[4]);
        }

        [TestMethod]
        public void EmptyChangesDoNotSendNotification()
        {
            var g = new GenericComparer<int>((a, b) => a < b ? 1 : a == b ? 0 : -1);
            bool change = false;

            this.c.CollectionChanged += (s, e) => change = true;
            var o = this.c.Sort(g);

            Assert.AreEqual(0, o.Count);
            Assert.IsFalse(change);
        }
    }
}
