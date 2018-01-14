using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Collection
{
    [TestClass]
    public class CollectionExtensionTest
    {
        [TestInitialize]
        public void Initialize()
        {
            
        }

        [TestMethod]
        public void It_does_nothing_on_empty_lists()
        {
            var empty = new List<int>();

            empty.ReorderFrom(new List<int>());

            Assert.AreEqual(0, empty.Count);
        }

        [TestMethod]
        public void It_does_nothing_if_order_is_already_the_same()
        {
            var input = new List<int> {1, 2, 3};

            input.ReorderFrom(new List<int> { 1, 2, 3});

            input.AssertContains(1, 2, 3);
        }

        [TestMethod]
        public void It_sorts_when_all_changes()
        {
            var input = new List<int> { 1, 2, 3 };

            input.ReorderFrom(new List<int> { 3, 2, 1 });

            input.AssertContains(3, 2, 1);
        }

        [TestMethod]
        public void It_sorts_when_1_change()
        {
            var input = new List<int> { 1, 2, 3 };

            input.ReorderFrom(new List<int> { 2, 1, 3 });

            input.AssertContains(2, 1, 3);
        }

        [TestMethod]
        public void It_sorts_sublist_middle()
        {
            var input = new List<int> { 1, 2, 3, 4, 5 };

            input.ReorderFrom(new List<int> { 4, 3, 2 });

            input.AssertContains(1, 4, 3, 2, 5);
        }

        [TestMethod]
        public void It_sorts_sublist_same_order()
        {
            var input = new List<int> { 1, 2, 3, 4, 5 };

            input.ReorderFrom(new List<int> { 2, 3, 4 });

            input.AssertContains(1, 2, 3, 4, 5);
        }

        [TestMethod]
        public void It_sorts_sublist_start()
        {
            var input = new List<int> { 1, 2, 3, 4, 5 };

            input.ReorderFrom(new List<int> { 3, 2, 1 });

            input.AssertContains(3, 2, 1, 4, 5);
        }

        [TestMethod]
        public void It_sorts_sublist_end()
        {
            var input = new List<int> { 1, 2, 3, 4, 5 };

            input.ReorderFrom(new List<int> { 5, 4, 3 });

            input.AssertContains(1, 2, 5, 4, 3);
        }

        [TestMethod]
        public void It_excludes_unkown_items()
        {
            var input = new List<int> { 1, 2, 3, 4, 5 };

            input.ReorderFrom(new List<int> { 7 });

            input.AssertContains(1, 2, 3, 4, 5);
        }

        [TestMethod]
        public void It_excludes_single_item()
        {
            var input = new List<int> { 1, 2, 3, 4, 5 };

            input.ReorderFrom(new List<int> { 3 });

            input.AssertContains(1, 2, 3, 4, 5);
        }

        [TestMethod]
        public void It_sort_1_item()
        {
            var input = new List<int> { 1, 2, 3, 4 };

            input.ReorderFrom(new List<int> { 1, 4, 2, 3});

            input.AssertContains(1, 4, 2, 3);
        }

        [TestMethod]
        public void Contains_1()
        {
            var input = new List<int> {};

            var source = new List<int> {};

            Assert.IsTrue(source.Contains(input));
        }

        [TestMethod]
        public void Contains_2()
        {
            var input = new List<int> { 1 };

            var source = new List<int> { };

            Assert.IsFalse(source.Contains(input));
        }

        [TestMethod]
        public void Contains_3()
        {
            var input = new List<int> { 1 };

            var source = new List<int> { 1 };

            Assert.IsTrue(source.Contains(input));
        }

        [TestMethod]
        public void Contains_4()
        {
            var input = new List<int> { 1, 2 };

            var source = new List<int> { 3 };

            Assert.IsFalse(source.Contains(input));
        }

        [TestMethod]
        public void Contains_5()
        {
            var input = new List<int> { 1, 2 };

            var source = new List<int> { 3, 2, 1 };

            Assert.IsTrue(source.Contains(input));
        }

        [TestMethod]
        public void Contains_6()
        {
            var input = new List<int> { 1, 2, 4 };

            var source = new List<int> { 3, 2, 1 };

            Assert.IsFalse(source.Contains(input));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Rotate_null_arg()
        {
            CollectionExtension.AlternateTwoColumns(null);
        }

        [TestMethod]
        public void Rotate_empty()
        {
            var result = new List<object>().AlternateTwoColumns();

            Assert.IsNotNull(result);
            result.AssertContains();
        }

        [TestMethod]
        public void Rotate_1_element()
        {
            var result = new List<object> { "a" }.AlternateTwoColumns();

            Assert.IsNotNull(result);
            result.AssertContains("a");
        }

        [TestMethod]
        public void Rotate_2_elements()
        {
            var result = new List<object> { "a", "b" }.AlternateTwoColumns();

            Assert.IsNotNull(result);
            result.AssertContains("a", "b");
        }

        [TestMethod]
        public void Rotate_3_elements()
        {
            var result = new List<object> { "a", "b", "c" }.AlternateTwoColumns();

            Assert.IsNotNull(result);
            result.AssertContains("a", "c", "b");
        }

        [TestMethod]
        public void Rotate_4_elements()
        {
            var result = new List<object> { "a", "b", "c", "d" }.AlternateTwoColumns();

            Assert.IsNotNull(result);
            result.AssertContains("a", "c", "b", "d");
        }

        [TestMethod]
        public void Rotate_5_elements()
        {
            var result = new List<object> { "a", "b", "c", "d", "e" }.AlternateTwoColumns();

            Assert.IsNotNull(result);
            result.AssertContains("a", "d", "b", "e", "c");
        }

        [TestMethod]
        public void Rotate_6_elements()
        {
            var result = new List<object> { "a", "b", "c", "d", "e", "f" }.AlternateTwoColumns();

            Assert.IsNotNull(result);
            result.AssertContains("a", "d", "b", "e", "c", "f");
        }
    }
}
