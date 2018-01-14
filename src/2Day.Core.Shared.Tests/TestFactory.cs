using System.IO;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using SQLite.Net.Platform.Generic;
using SQLite.Net.Platform.Win32;

namespace Chartreuse.Today.Core.Shared.Tests
{
    public class TestFactory
    {
        public TestFactory()
        {
            if (File.Exists("test.db"))
                File.Delete("test.db");

            this.Datacontext = new DatabaseContext("./test.db", false, new SQLitePlatformWin32(), new TestTrackingManager());
            this.Datacontext.InitializeDatabase();

            this.Workbook = new Workbook(this.Datacontext, new TestSettings());
            this.Workbook.Initialize();
        }

        public Workbook Workbook { get; private set; }

        public DatabaseContext Datacontext { get; private set; }
    }
}