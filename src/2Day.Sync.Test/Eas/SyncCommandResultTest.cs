using System;
using System.IO;
using System.Xml.Linq;
using Chartreuse.Today.Exchange.ActiveSync.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Eas
{
    [TestClass]
    public class SyncCommandResultTest
    {
        [TestMethod]
        public void ParseSyncCommandResult_AkrutoSync()
        {
            // non regression test for a case where sync fails with a reponse from AkrutoSync
            string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
            <Add xmlns:airsync=""AirSync"">
                <ServerId xmlns:airsync=""AirSync"">5:16:8052</ServerId>
                <ApplicationData xmlns:airsync=""AirSync"">
                    <Body xmlns:airsyncbase=""AirSyncBase"">
                        <Type xmlns:airsyncbase=""AirSyncBase"">1</Type>
                        <EstimatedDataSize xmlns:airsyncbase=""AirSyncBase"">136</EstimatedDataSize>
                        <Data xmlns:airsyncbase=""AirSyncBase"">In 2016: Christstollen mit Weihnachtskarten</Data>
                    </Body>
                    <Subject xmlns:tasks=""Tasks"">kleine Geschenke an die Hausmeister</Subject>
                    <Importance xmlns:tasks=""Tasks"">1</Importance>
                    <Categories xmlns:tasks=""Tasks"">
                        <Category xmlns:tasks=""Tasks"">0171 LA-Team Kasse</Category>
                    </Categories>
                    <UtcDueDate xmlns:tasks=""Tasks"">2016-11-08T23:00:00.000Z</UtcDueDate>
                    <DueDate xmlns:tasks=""Tasks"">2016-11-09T00:00:00.000Z</DueDate>
                    <Recurrence xmlns:tasks=""Tasks"">
                        <Type xmlns:tasks=""Tasks"">5</Type>
                        <Start xmlns:tasks=""Tasks"">2015-11-09T00:00:00.000Z</Start>
                        <Interval xmlns:tasks=""Tasks"">1</Interval>
                        <DayOfMonth xmlns:tasks=""Tasks"">9</DayOfMonth>
                        <MonthOfYear xmlns:tasks=""Tasks"">11</MonthOfYear>
                        <Regenerate xmlns:tasks=""Tasks"">0</Regenerate>
                        <DeadOccur xmlns:tasks=""Tasks"">0</DeadOccur>
                    </Recurrence>
                    <Complete xmlns:tasks=""Tasks"">1</Complete>
                    <Sensitivity xmlns:tasks=""Tasks"">2</Sensitivity>
                    <ReminderSet xmlns:tasks=""Tasks"">0</ReminderSet>
                </ApplicationData>
            </Add>";

            var xElement = XElement.Load(new StringReader(xml));

            // act
            var exchangeTask = SyncCommandResult.GetExchangeTask(xElement);

            // check that we've fallback to today for the completion date
            Assert.IsNotNull(exchangeTask.Completed);
            Assert.AreEqual(DateTime.Now.Date, exchangeTask.Completed.Value.Date);
        }

        [TestMethod]
        public void ParseSyncCommandResult_Memotoo()
        {
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <Change xmlns:airsync=""AirSync"">
                  <ServerId>3358341</ServerId>
                  <ApplicationData>
                    <Complete xmlns:tasks=""Tasks"">1</Complete>
                    <DueDate xmlns:tasks=""Tasks"">2017-01-10T00:00:00.000Z</DueDate>
                    <UtcDueDate xmlns:tasks=""Tasks"">2017-01-10T21:00:00.000Z</UtcDueDate>
                    <Importance xmlns:tasks=""Tasks"">1</Importance>
                    <ReminderSet xmlns:tasks=""Tasks"">1</ReminderSet>
                    <ReminderTime xmlns:tasks=""Tasks"">2017-01-09T23:00:00.000Z</ReminderTime>
                    <Sensitivity xmlns:tasks=""Tasks"">0</Sensitivity>
                    <StartDate xmlns:tasks=""Tasks"">2017-01-10T00:00:00.000Z</StartDate>
                    <UtcStartDate xmlns:tasks=""Tasks"">2017-01-10T21:00:00.000Z</UtcStartDate>
                    <Subject xmlns:tasks=""Tasks"">Premix Business Manager position, High Frequnecy Plastics, Business Development Director Tuomas Kiikka at + 358 50 386 6420</Subject>
                    <Categories xmlns:tasks=""Tasks"">
                      <Category>Jobs</Category>
                    </Categories>
                  </ApplicationData>
                </Change>";

            var xElement = XElement.Load(new StringReader(xml));

            // act
            var exchangeTask = SyncCommandResult.GetExchangeTask(xElement);

            // check that we've fallback to today for the completion date
            Assert.IsNotNull(exchangeTask.Completed);
            Assert.AreEqual(DateTime.Now.Date, exchangeTask.Completed.Value.Date);
        }
    }
}