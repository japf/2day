using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Tools.Collection;
using Chartreuse.Today.Tests.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Collection
{
    [TestClass]
    public class SmartCollectionTest
    {
        private SmartCollection<Person> collection;
        private List<Person> data;

        private Person a0_0;

        [TestInitialize]
        public void Setup()
        {
            this.a0_0 = new Person("a0_0", 0);
            this.data = new List<Person>
                {
                    this.a0_0,
                    new Person("a1_1", 1),
                    new Person("b2_2", 2), new Person("b3_2", 2),
                    new Person("c4_3", 3), new Person("c5_3", 3), new Person("d6_3", 3),
                    new Person("d7_4", 4), new Person("e8_4", 4), new Person("e9_4", 4), new Person("f10_4", 4),
                    new Person("f11_5", 5), new Person("g12_5", 5), new Person("g13_5", 5), new Person("h14_5", 5), new Person("h15_5", 5),
                };
            var groupBuilder = new GroupBuilder<Person>(
                p => p.Name,
                p => p.Age.ToString(),
                (g1, g2) => g1.Title.CompareTo(g2.Title),
                (p1, p2) => p1.Name.CompareTo(p2.Name));

            Predicate<Person> filter = p => p.Age % 2 == 0;

            this.collection = new SmartCollection<Person>(this.data, groupBuilder, p => true, filter);
        }

        [TestMethod]
        public void Simple()
        {
            var data = new List<Task>
            {
                new Task { Title = "task1" }
            };
            var groupBuilder = new GroupBuilder<Task>(
                p => p.Title,
                p => p.Title,
                (g1, g2) => g1.Title.CompareTo(g2.Title),
                (p1, p2) => p1.Title.CompareTo(p2.Title));

            var collection = new SmartCollection<Task>(data, groupBuilder, t => !t.Completed.HasValue);

            Assert.AreEqual(1, collection.Items.Count);
            Assert.AreEqual(1, collection.Items[0].Count);
            Assert.AreEqual("task1", collection.Items[0][0].Title);

            data[0].IsCompleted = true;
            collection.InvalidateItem(data[0]);

            Assert.AreEqual(0, collection.Items.Count);

            data[0].IsCompleted = false;
            collection.InvalidateItem(data[0]);

            Assert.AreEqual(1, collection.Items.Count);
            Assert.AreEqual(1, collection.Items[0].Count);
            Assert.AreEqual("task1", collection.Items[0][0].Title);            
        }

        [TestMethod]
        public void Filter()
        {
            this.HasGroups("0", "1", "2", "3", "4", "5");
            this.GroupHas("0", "a0_0");
            this.GroupHas("1", "a1_1");
            this.GroupHas("2", "b2_2", "b3_2");
            this.GroupHas("3", "c4_3", "c5_3", "d6_3");
            this.GroupHas("4", "d7_4", "e8_4", "e9_4", "f10_4");
            this.GroupHas("5", "f11_5", "g12_5", "g13_5", "h14_5", "h15_5");

            this.collection.Filter = p => p.Name.StartsWith("a");

            this.HasGroups("0", "1");
            this.GroupHas("0", "a0_0");
            this.GroupHas("1", "a1_1");

            this.collection.Filter = null;

            this.HasGroups("0", "1", "2", "3", "4", "5");
            this.GroupHas("0", "a0_0");
            this.GroupHas("1", "a1_1");
            this.GroupHas("2", "b2_2", "b3_2");
            this.GroupHas("3", "c4_3", "c5_3", "d6_3");
            this.GroupHas("4", "d7_4", "e8_4", "e9_4", "f10_4");
            this.GroupHas("5", "f11_5", "g12_5", "g13_5", "h14_5", "h15_5");
        }

        [TestMethod]
        public void Stress()
        {
            var sw = new Stopwatch();
            sw.Start();

            // default mode
            this.HasGroups("0", "1", "2", "3", "4", "5");
            this.GroupHas("0", "a0_0");
            this.GroupHas("1", "a1_1");
            this.GroupHas("2", "b2_2", "b3_2");
            this.GroupHas("3", "c4_3", "c5_3", "d6_3");
            this.GroupHas("4", "d7_4", "e8_4", "e9_4", "f10_4");
            this.GroupHas("5", "f11_5", "g12_5", "g13_5", "h14_5", "h15_5");
            Assert.IsFalse(this.collection.IsEmpty);
            Assert.AreEqual(16, this.collection.Count);
            
            // group using the first letter
            this.collection.GroupBuilder = new GroupBuilder<Person>(
                p => p.Name, 
                p => p.Name[0].ToString(), 
                (g1, g2) => g1.Title.CompareTo(g2.Title),
                (p1, p2) => p1.Name.CompareTo(p2.Name));

            this.HasGroups("a", "b", "c", "d", "e", "f", "g", "h");
            this.GroupHas("a", "a0_0", "a1_1");
            this.GroupHas("b", "b2_2", "b3_2");
            this.GroupHas("c", "c4_3", "c5_3");
            this.GroupHas("d", "d6_3", "d7_4");
            this.GroupHas("e", "e8_4", "e9_4");
            this.GroupHas("f", "f10_4", "f11_5");
            this.GroupHas("g", "g12_5", "g13_5");
            this.GroupHas("h", "h14_5", "h15_5");
            Assert.IsFalse(this.collection.IsEmpty);
            Assert.AreEqual(16, this.collection.Count);
            
            sw.Stop();
            Console.WriteLine("Test took " + sw.ElapsedMilliseconds + "ms");
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
                Assert.AreEqual(content[i], group[i].Name);
            }
        }
    }
}
