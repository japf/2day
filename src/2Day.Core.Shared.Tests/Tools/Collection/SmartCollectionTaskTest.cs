using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Tools.Collection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Collection
{
    [TestClass]
    public class SmartTaskCollectionTest
    {
        private SmartCollection<MetaTask> collection;
        private List<MetaTask> data;

        [TestInitialize]
        public void Setup()
        {
            this.data = new List<MetaTask>
                {
                    new MetaTask { Title = "task0", IsCompleted = false, Meta = 0, Order = 1 },
                    new MetaTask { Title = "task1", IsCompleted = false, Meta = 0, Order = 2 },
                    new MetaTask { Title = "task2", IsCompleted = true, Meta = 0, Order = 3 },
                    new MetaTask { Title = "task3", IsCompleted = false, Meta = 1, Order = 1 },
                    new MetaTask { Title = "task4", IsCompleted = false, Meta = 1, Order = 2 },
                    new MetaTask { Title = "task5", IsCompleted = true, Meta = 1, Order = 3 },
                    new MetaTask { Title = "task6", IsCompleted = true, Meta = 2, Order = 1 },
                    new MetaTask { Title = "task7", IsCompleted = true, Meta = 2, Order = 2 },
                    new MetaTask { Title = "task8", IsCompleted = false, Meta = 2, Order = 3 },
                    new MetaTask { Title = "task9", IsCompleted = true, Meta = 3, Order = 1 },
                };

            var groupBuilder = new GroupBuilder<MetaTask>(
                t => t.Title,
                t => t.Meta.ToString(),
                (g1, g2) => g1[0].Meta.CompareTo(g2[0].Meta),
                (t1, t2) => t1.Order.CompareTo(t2.Order));

            Predicate<MetaTask> focusFilter = t => t.Meta == 0;

            this.collection = new SmartCollection<MetaTask>(this.data, groupBuilder, p => true, focusFilter);
        }

        [TestMethod]
        public void Stress()
        {
            this.HasGroups("0", "1", "2", "3");
            this.GroupHas("0", "task0", "task1", "task2");
            this.GroupHas("1", "task3", "task4", "task5");
            this.GroupHas("2", "task6", "task7", "task8");
            this.GroupHas("3", "task9");
            Assert.AreEqual(10, this.collection.Count);

            this.collection.Filter = t => !t.IsCompleted;
            this.collection.Filter = null;
            this.collection.Filter = t => !t.IsCompleted;

            this.HasGroups("0", "1", "2");
            this.GroupHas("0", "task0", "task1");
            this.GroupHas("1", "task3", "task4");
            this.GroupHas("2", "task8");
            Assert.AreEqual(5, this.collection.Count);

            this.collection.Filter = null;
            this.collection.Filter = t => !t.IsCompleted;
            this.collection.Filter = null;
            
            this.HasGroups("0", "1", "2", "3");
            this.GroupHas("0", "task0", "task1", "task2");
            this.GroupHas("1", "task3", "task4", "task5");
            this.GroupHas("2", "task6", "task7", "task8");
            this.GroupHas("3", "task9");
            Assert.AreEqual(10, this.collection.Count);
        }
        
        public void HasGroups(params string[] groups)
        {
            Assert.AreEqual(groups.Length, this.collection.Items.Count);
            for (int i = 0; i < groups.Length; i++)
            {
                Assert.AreEqual(groups[i], this.collection.Items[i].Title);
            }
        }

        public void GroupHas(string groupTitle, params string[] content)
        {
            var group = this.collection.Items.FirstOrDefault(g => g.Title == groupTitle);
            Assert.IsNotNull(group);
            Assert.AreEqual(content.Length, group.Count);
            for (int i = 0; i < content.Length; i++)
            {
                Assert.AreEqual(content[i], group[i].Title);
            }

        }

        private class MetaTask : Task
        {
            public int Meta { get; set; }

            public int Order { get; set; }
        }     
    }
}
