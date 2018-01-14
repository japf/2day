using System.Collections.Generic;
using System.Xml;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Universal.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools
{
    [TestClass]
    public class NotificationContentBuilderTest
    {
        [TestMethod]
        public void CreateTileNotification_SimpleNoTask()
        {
            CreateTileNotification("title");
        }

        [TestMethod]
        public void CreateTileNotification_SpecialCharacter_1()
        {
            CreateTileNotification("title > title");
        }

        [TestMethod]
        public void CreateTileNotification_SpecialCharacter_2()
        {
            CreateTileNotification("title < title");
        }

        [TestMethod]
        public void CreateTileNotification_SpecialCharacter_3()
        {
            CreateTileNotification("title & title");
        }

        [TestMethod]
        public void CreateTileNotification_SpecialCharacter_4()
        {
            CreateTileNotification("title ' title");
        }

        [TestMethod]
        public void CreateTileNotification_SpecialCharacter_5()
        {
            CreateTileNotification("title \" title");
        }

        [TestMethod]
        public void CreateTileNotification_SpecialCharacter_6()
        {
            CreateTileNotification("title + title");
        }

        [TestMethod]
        public void CreateTileNotification_SpecialCharacter_7()
        {
            char c = (char) (0x0020 - 1);
            CreateTileNotification("title title" + c);
        }

        [TestMethod]
        public void CreateTileNotification_TooLarge()
        {
            // setup
            string content = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer sapien turpis, convallis in est et, convallis aliquam ligula. Donec laoreet id nisi nec pulvinar. Quisque id lectus ut felis suscipit gravida. Etiam arcu nunc, hendrerit imperdiet nisl at, vehicula posuere mauris. Vestibulum elit mauris, accumsan eu sollicitudin nec, tempor sed dolor. Donec sit amet justo vel justo accumsan blandit ut non ante. Nullam eu turpis eu quam accumsan posuere.
                Ut bibendum est sit amet ligula luctus, et blandit libero tincidunt. Curabitur tristique ipsum massa, sed sagittis elit feugiat vel. Etiam at ultricies turpis. Quisque justo ex, fermentum consequat nulla a, semper tincidunt purus. Aenean faucibus nulla vitae sapien sollicitudin, et consequat lacus sodales. Vestibulum viverra eu justo eu cursus. Etiam mauris lorem, blandit in ipsum nec, laoreet scelerisque ligula. Pellentesque a ipsum in magna tincidunt molestie tincidunt a urna. Proin interdum, nulla et aliquam mattis, risus magna congue libero, eu sagittis libero metus et dolor. Nam faucibus mi id justo pellentesque placerat. Praesent ut quam sed metus varius consequat sed ut est.
                In rhoncus purus eget lorem pulvinar vestibulum. Quisque fringilla bibendum eleifend. Nulla erat nibh, interdum vel venenatis ut, aliquam sed leo. Cras tempor tristique ex feugiat dapibus. Curabitur tempor felis diam. In a massa rutrum, luctus felis non, condimentum leo. Etiam consequat eu metus quis tempus. Aliquam erat volutpat. Nullam justo lacus, vulputate vel tincidunt sit amet, tempor porttitor sem. Sed sollicitudin lorem sed lorem tincidunt, id molestie nunc viverra.
                Vestibulum feugiat laoreet tempus. Aenean auctor rhoncus velit a luctus. Phasellus a eleifend orci, in finibus nisl. Maecenas ultricies elit ut ex ornare tempus. Donec vitae mi lorem. Mauris varius in eros ac dictum. Phasellus aliquet arcu vel viverra iaculis. Pellentesque nisl mauris, pharetra sit amet ex non, luctus luctus dui. Nulla facilisi. Vestibulum sagittis nisl eros. Praesent sollicitudin imperdiet lacus id molestie. Sed volutpat accumsan mauris non hendrerit. Suspendisse pretium ex in diam blandit varius. Etiam eu ante a elit vestibulum porttitor sed ut erat. Aenean at egestas tellus, dictum mollis lectus. Sed metus odio, fringilla eget ligula vel, pellentesque convallis magna.
                Quisque eget placerat massa, et interdum nulla. Cras finibus nisl arcu, eget dignissim ex semper sed. In pharetra sapien elit, at eleifend eros mollis vel. Vestibulum fermentum ipsum vitae odio euismod volutpat. Aliquam in purus consectetur, interdum elit nec, ullamcorper magna. Suspendisse id consectetur nisl. Aliquam tempus dapibus eros, eu posuere est porttitor sit amet.
                Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer sapien turpis, convallis in est et, convallis aliquam ligula. Donec laoreet id nisi nec pulvinar. Quisque id lectus ut felis suscipit gravida. Etiam arcu nunc, hendrerit imperdiet nisl at, vehicula posuere mauris. Vestibulum elit mauris, accumsan eu sollicitudin nec, tempor sed dolor. Donec sit amet justo vel justo accumsan blandit ut non ante. Nullam eu turpis eu quam accumsan posuere.
                Ut bibendum est sit amet ligula luctus, et blandit libero tincidunt. Curabitur tristique ipsum massa, sed sagittis elit feugiat vel. Etiam at ultricies turpis. Quisque justo ex, fermentum consequat nulla a, semper tincidunt purus. Aenean faucibus nulla vitae sapien sollicitudin, et consequat lacus sodales. Vestibulum viverra eu justo eu cursus. Etiam mauris lorem, blandit in ipsum nec, laoreet scelerisque ligula. Pellentesque a ipsum in magna tincidunt molestie tincidunt a urna. Proin interdum, nulla et aliquam mattis, risus magna congue libero, eu sagittis libero metus et dolor. Nam faucibus mi id justo pellentesque placerat. Praesent ut quam sed metus varius consequat sed ut est.
                In rhoncus purus eget lorem pulvinar vestibulum. Quisque fringilla bibendum eleifend. Nulla erat nibh, interdum vel venenatis ut, aliquam sed leo. Cras tempor tristique ex feugiat dapibus. Curabitur tempor felis diam. In a massa rutrum, luctus felis non, condimentum leo. Etiam consequat eu metus quis tempus. Aliquam erat volutpat. Nullam justo lacus, vulputate vel tincidunt sit amet, tempor porttitor sem. Sed sollicitudin lorem sed lorem tincidunt, id molestie nunc viverra.
                Vestibulum feugiat laoreet tempus. Aenean auctor rhoncus velit a luctus. Phasellus a eleifend orci, in finibus nisl. Maecenas ultricies elit ut ex ornare tempus. Donec vitae mi lorem. Mauris varius in eros ac dictum. Phasellus aliquet arcu vel viverra iaculis. Pellentesque nisl mauris, pharetra sit amet ex non, luctus luctus dui. Nulla facilisi. Vestibulum sagittis nisl eros. Praesent sollicitudin imperdiet lacus id molestie. Sed volutpat accumsan mauris non hendrerit. Suspendisse pretium ex in diam blandit varius. Etiam eu ante a elit vestibulum porttitor sed ut erat. Aenean at egestas tellus, dictum mollis lectus. Sed metus odio, fringilla eget ligula vel, pellentesque convallis magna.
                Quisque eget placerat massa, et interdum nulla. Cras finibus nisl arcu, eget dignissim ex semper sed. In pharetra sapien elit, at eleifend eros mollis vel. Vestibulum fermentum ipsum vitae odio euismod volutpat. Aliquam in purus consectetur, interdum elit nec, ullamcorper magna. Suspendisse id consectetur nisl. Aliquam tempus dapibus eros, eu posuere est porttitor sit amet.";

            CreateTileNotification(content, 1500);
        }

        [TestMethod]
        public void CreateTileNotification_Html()
        {
            string content = "<html><div>Hello</div></html>";

            var tileNotificationDoc = NotificationContentBuilder.CreateTileNotification(content, new List<ITask> { new Task { Title = content, Note = content } }, null);

            Assert.IsFalse(tileNotificationDoc.InnerText.Contains("html"));
            Assert.IsFalse(tileNotificationDoc.InnerText.Contains("div"));
            Assert.IsTrue(tileNotificationDoc.InnerText.Contains("Hello"));
        }

        private static void CreateTileNotification(string content, int? maxSize = null)
        {
            var tileNotificationDoc = NotificationContentBuilder.CreateTileNotification(content, new List<ITask> { new Task { Title = content, Note = content } }, null);
            Assert.IsNotNull(tileNotificationDoc);
            CheckMaxSize(tileNotificationDoc, maxSize);

            var taskNotificationDoc = NotificationContentBuilder.CreateTaskToastNotification(new Task { Title = content, Note = content });
            Assert.IsNotNull(taskNotificationDoc);
            CheckMaxSize(taskNotificationDoc, maxSize);

            var simpleDoc = NotificationContentBuilder.CreateSimpleToastNotification(content, content);
            Assert.IsNotNull(simpleDoc);
            CheckMaxSize(simpleDoc, maxSize);
        }

        private static void CheckMaxSize(XmlDocument tileNotificationDoc, int? maxSize)
        {
            if (maxSize.HasValue)
                Assert.IsTrue(tileNotificationDoc.InnerText.Length < maxSize.Value);
        }
    }
}