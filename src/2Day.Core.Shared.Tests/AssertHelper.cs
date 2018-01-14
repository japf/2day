using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests
{
    public static class AssertHelper
    {
        public static void AssertOrderIs(this List<ITask> source, params ITask[] tasks)
        {
            Assert.AreEqual(tasks.Length, source.Count);
            for (int i = 0; i < tasks.Length; i++)
                Assert.AreEqual(tasks[i], source[i]);
        }

        public static void AssertOrderIs(this List<ITask> source, params string[] titles)
        {
            Assert.AreEqual(titles.Length, source.Count);
            for (int i = 0; i < titles.Length; i++)
                Assert.AreEqual(titles[i], source[i].Title);
        }

        public static void AssertArrayAreEqual<T>(T[] expected, T[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length, "Length are different");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}