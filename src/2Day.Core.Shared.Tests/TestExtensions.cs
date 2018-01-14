using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests
{
    public static class TestExtensions
    {
        public static void AssertContains(this IFolder folder, params ITask[] tasks)
        {
            Assert.AreEqual(tasks.Length, folder.TaskCount);
            foreach (var task in tasks)
            {
                Assert.IsTrue(folder.Tasks.Contains(task));
            }
        }

        public static void AssertContains<T>(this IList<T> input, params T[] items)
        {
            Assert.AreEqual(input.Count, items.Length);
            for (int i = 0; i < input.Count; i++)
            {
                Assert.AreEqual(input[i], items[i]);
            }
        }
    }
}
