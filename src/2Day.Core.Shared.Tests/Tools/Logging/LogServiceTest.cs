using System;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Logging
{
    [TestClass]
    public class LogServiceTest
    {
        private Mock<ILogHandler> handler;

        [TestInitialize]
        public void Initialize()
        {
            this.handler = new Mock<ILogHandler>();
            LogService.Initialize(this.handler.Object);
            LogService.Clear();
        }

        [TestMethod]
        public void LogLevel_None()
        {
            // setup
            LogService.Level = LogLevel.None;

            // act
            LogService.Log(LogLevel.Network, "Hello1", "world1");
            LogService.Log(LogLevel.Debug, "Hello2", "world2");
            LogService.Log(LogLevel.Warning, "Hello3", "world3");
            LogService.Log(LogLevel.Error, "Hello4", "world4");

            // check
            Assert.AreEqual(0, LogService.Messages.Count);
        }

        [TestMethod]
        public void LogLevel_Network()
        {
            // setup
            LogService.Level = LogLevel.Network;

            // act
            LogService.Log(LogLevel.Network, "Hello1", "world1");
            LogService.Log(LogLevel.Debug, "Hello2", "world2");
            LogService.Log(LogLevel.Warning, "Hello3", "world3");
            LogService.Log(LogLevel.Error, "Hello4", "world4");

            // check
            Assert.AreEqual(1, LogService.Messages.Count);
            Assert.AreEqual("Hello1", LogService.Messages[0].Source);
            Assert.AreEqual("world1", LogService.Messages[0].Message);
        }

        [TestMethod]
        public void LogLevel_Debug()
        {
            // setup
            LogService.Level = LogLevel.Debug;

            // act
            LogService.Log(LogLevel.Network, "Hello1", "world1");
            LogService.Log(LogLevel.Debug, "Hello2", "world2");
            LogService.Log(LogLevel.Warning, "Hello3", "world3");
            LogService.Log(LogLevel.Error, "Hello4", "world4");

            // check
            Assert.AreEqual(2, LogService.Messages.Count);
            Assert.AreEqual("Hello2", LogService.Messages[1].Source);
            Assert.AreEqual("world2", LogService.Messages[1].Message);
        }

        [TestMethod]
        public void LogLevel_Warning()
        {
            // setup
            LogService.Level = LogLevel.Warning;

            // act
            LogService.Log(LogLevel.Network, "Hello1", "world1");
            LogService.Log(LogLevel.Debug, "Hello2", "world2");
            LogService.Log(LogLevel.Warning, "Hello3", "world3");
            LogService.Log(LogLevel.Error, "Hello4", "world4");

            // check
            Assert.AreEqual(1, LogService.Messages.Count);
            Assert.AreEqual("Hello3", LogService.Messages[0].Source);
            Assert.AreEqual("world3", LogService.Messages[0].Message);
        }

        [TestMethod]
        public void LogLevel_Error()
        {
            // setup
            LogService.Level = LogLevel.Error;

            // act
            LogService.Log(LogLevel.Network, "Hello1", "world1");
            LogService.Log(LogLevel.Debug, "Hello2", "world2");
            LogService.Log(LogLevel.Warning, "Hello3", "world3");
            LogService.Log(LogLevel.Error, "Hello4", "world4");

            // check
            Assert.AreEqual(1, LogService.Messages.Count);
            Assert.AreEqual("Hello4", LogService.Messages[0].Source);
            Assert.AreEqual("world4", LogService.Messages[0].Message);
        }

        [TestMethod]
        public void LogLevel_Normal()
        {
            // setup
            LogService.Level = LogLevel.Normal;

            // act
            LogService.Log(LogLevel.Network, "Hello1", "world1");
            LogService.Log(LogLevel.Debug, "Hello2", "world2");
            LogService.Log(LogLevel.Warning, "Hello3", "world3");
            LogService.Log(LogLevel.Error, "Hello4", "world4");

            // check
            Assert.AreEqual(4, LogService.Messages.Count);
            Assert.AreEqual("Hello2", LogService.Messages[1].Source);
            Assert.AreEqual("world2", LogService.Messages[1].Message);
            Assert.AreEqual("Hello3", LogService.Messages[2].Source);
            Assert.AreEqual("world3", LogService.Messages[2].Message);
            Assert.AreEqual("Hello4", LogService.Messages[3].Source);
            Assert.AreEqual("world4", LogService.Messages[3].Message);
        }

        [TestMethod]
        public void LogLevel_Verbose()
        {
            // setup
            LogService.Level = LogLevel.Verbose;

            // act
            LogService.Log(LogLevel.Network, "Hello1", "world1");
            LogService.Log(LogLevel.Debug, "Hello2", "world2");
            LogService.Log(LogLevel.Warning, "Hello3", "world3");
            LogService.Log(LogLevel.Error, "Hello4", "world4");

            // check
            Assert.AreEqual(5, LogService.Messages.Count);
            Assert.AreEqual("Hello1", LogService.Messages[1].Source);
            Assert.AreEqual("world1", LogService.Messages[1].Message);
            Assert.AreEqual("Hello2", LogService.Messages[2].Source);
            Assert.AreEqual("world2", LogService.Messages[2].Message);
            Assert.AreEqual("Hello3", LogService.Messages[3].Source);
            Assert.AreEqual("world3", LogService.Messages[3].Message);
            Assert.AreEqual("Hello4", LogService.Messages[4].Source);
            Assert.AreEqual("world4", LogService.Messages[4].Message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LogLevel_Normal_AsArg()
        {
            // setup
            LogService.Level = LogLevel.None;

            // act
            LogService.Log(LogLevel.Normal, "Hello", "world");

            // check
            Assert.AreEqual(0, LogService.Messages.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LogLevel_Verbose_AsArg()
        {
            // setup
            LogService.Level = LogLevel.None;

            // act
            LogService.Log(LogLevel.Verbose, "Hello", "world");

            // check
            Assert.AreEqual(0, LogService.Messages.Count);
        }

        public enum LogLevel2
        {
            None = 0x0000,
            Network = 0x0001,
            Debug = 0x0002,
            Warning = 0x0004,
            Error = 0x0008,

            Normal = Debug | Warning | Error,
            Verbose = Network | Debug | Warning | Error
        }
    }
}
