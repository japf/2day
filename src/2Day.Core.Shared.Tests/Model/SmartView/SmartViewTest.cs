using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Smart;
using Chartreuse.Today.Core.Shared.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model.SmartView
{
    [TestClass]
    public class SmartViewTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Empty()
        {
            var handler = SmartViewHandler.FromString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid1()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid2()
        {
            var handler = SmartViewHandler.FromString("Priority Is Star)");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid3()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star) AND");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid4()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star OR Title BeginsWith e AND Title BeginsWith f) AND (Note EndsWith a)");
        }

        [TestMethod]
        public void All_fields_match_one_rule()
        {
            var fields = EnumHelper.GetAllValues<SmartViewField>();
            foreach (SmartViewField field in fields)
            {
                SmartViewRule rule = SmartViewHelper.GetSupportedRule(field);
                Assert.IsNotNull(rule);
            }
        }

        [TestMethod]
        public void Rules_1_block_1()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star)");

            handler.AssertContent(1, SmartViewMatchType.Any);

            handler.AssertRule<SmartViewPriorityRule>(
                0, 0, 1,
                SmartViewMatchType.Any,
                SmartViewField.Priority,
                SmartViewFilter.Is,
                r => r.Value == TaskPriority.Star);
        }

        [TestMethod]
        public void Rules_2_block_1()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star OR Title BeginsWith e)");

            handler.AssertContent(1, SmartViewMatchType.Any);

            handler.AssertRule<SmartViewPriorityRule>(
                0, 0, 2,
                SmartViewMatchType.Any,
                SmartViewField.Priority,
                SmartViewFilter.Is,
                r => r.Value == TaskPriority.Star);

            handler.AssertRule<SmartViewStringRule>(
                0, 1, 2,
                SmartViewMatchType.Any,
                SmartViewField.Title,
                SmartViewFilter.BeginsWith,
                r => r.Value == "e");
        }

        [TestMethod]
        public void Rules_2_block_2_or_and()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star OR Title BeginsWith e) AND (Note EndsWith a)");

            handler.AssertContent(2, SmartViewMatchType.All);

            handler.AssertRule<SmartViewPriorityRule>(
                0, 0, 2,
                SmartViewMatchType.Any,
                SmartViewField.Priority,
                SmartViewFilter.Is,
                r => r.Value == TaskPriority.Star);
            handler.AssertRule<SmartViewStringRule>(
                0, 1, 2,
                SmartViewMatchType.Any,
                SmartViewField.Title,
                SmartViewFilter.BeginsWith,
                r => r.Value == "e");
            handler.AssertRule<SmartViewStringRule>(
                1, 0, 1,
                SmartViewMatchType.Any,
                SmartViewField.Note,
                SmartViewFilter.EndsWith,
                r => r.Value == "a");
        }

        [TestMethod]
        public void Rules_2_block_2_and_and()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star AND Title BeginsWith e) AND (Note EndsWith a)");

            handler.AssertContent(2, SmartViewMatchType.All);

            handler.AssertRule<SmartViewPriorityRule>(
                0, 0, 2,
                SmartViewMatchType.All,
                SmartViewField.Priority,
                SmartViewFilter.Is,
                r => r.Value == TaskPriority.Star);
            handler.AssertRule<SmartViewStringRule>(
                0, 1, 2,
                SmartViewMatchType.All,
                SmartViewField.Title,
                SmartViewFilter.BeginsWith,
                r => r.Value == "e");
            handler.AssertRule<SmartViewStringRule>(
                1, 0, 1,
                SmartViewMatchType.Any,
                SmartViewField.Note,
                SmartViewFilter.EndsWith,
                r => r.Value == "a");
        }

        [TestMethod]
        public void Rules_1_block_2()
        {
            var handler = SmartViewHandler.FromString("(Priority Is Star) AND (Due WasInTheLast 120)");

            handler.AssertContent(2, SmartViewMatchType.All);
            
            handler.AssertRule<SmartViewPriorityRule>(
                0, 0, 1,
                SmartViewMatchType.Any, 
                SmartViewField.Priority, 
                SmartViewFilter.Is, 
                r => r.Value == TaskPriority.Star);

            handler.AssertRule<SmartViewDateRule>(
                1, 0, 1,
                SmartViewMatchType.Any, 
                SmartViewField.Due, 
                SmartViewFilter.WasInTheLast, 
                r => r.Value.Days == 120);
        }

        [TestMethod]
        public void Custom1()
        {
            var handler = SmartViewHandler.FromString("(Due DoesNotExist 0) AND (Context IsNot @context)");

            handler.AssertContent(2, SmartViewMatchType.All);

            handler.AssertRule<SmartViewDateRule>(
                0, 0, 1,
                SmartViewMatchType.Any,
                SmartViewField.Due,
                SmartViewFilter.DoesNotExist,
                r => !r.Value.Date.HasValue);

            handler.AssertRule<SmartViewContextRule>(
                1, 0, 1,
                SmartViewMatchType.Any,
                SmartViewField.Context,
                SmartViewFilter.IsNot,
                r => r.Value == "@context");
        }

        [TestMethod]
        public void FolderIs_With_Spaces()
        {
            var handler = SmartViewHandler.FromString("(Folder Is folder 1 with special name)");

            handler.AssertContent(1, SmartViewMatchType.Any);

            handler.AssertRule<SmartViewFolderRule>(
                0, 0, 1,
                SmartViewMatchType.Any,
                SmartViewField.Folder,
                SmartViewFilter.Is,
                r => r.Value == "folder 1 with special name");
        }

        [TestMethod]
        public void ContextIs_With_Space()
        {
            var handler = SmartViewHandler.FromString("(Context Is context 1 with special name)");

            handler.AssertContent(1, SmartViewMatchType.Any);

            handler.AssertRule<SmartViewContextRule>(
                0, 0, 1,
                SmartViewMatchType.Any,
                SmartViewField.Context,
                SmartViewFilter.Is,
                r => r.Value == "context 1 with special name");
        }
    }

    public static class SmartViewTestHelper
    {
        public static void AssertContent(this SmartViewHandler handler, int blockCount,SmartViewMatchType type)
        {
            Assert.AreEqual(blockCount, handler.Blocks.Count);
            Assert.AreEqual(type, handler.Match);            
        }

        public static T AssertRule<T>(this SmartViewHandler handler, int block, int rule, int ruleCount, SmartViewMatchType type, SmartViewField field, SmartViewFilter filter, Func<T, bool> checkRule)
            where T : SmartViewRule
        {
            SmartViewBlockRule smartViewBlockRule = handler.Blocks[block];

            Assert.AreEqual(ruleCount, smartViewBlockRule.Rules.Count);
            Assert.AreEqual(type, smartViewBlockRule.Match);            

            SmartViewRule smartViewRule = smartViewBlockRule.Rules[rule];

            Assert.AreEqual(field, smartViewRule.Field);
            Assert.AreEqual(filter, smartViewRule.Filter);

            SmartViewRule item = smartViewRule;
            Assert.IsTrue(item is T);

            Assert.IsTrue(checkRule((T) item));

            return (T) item;
        }
    }
}
