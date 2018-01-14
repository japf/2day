using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.UI.Popups;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.Tools
{
    public class AppointmentHelper
    {
        public static async Task CreateAppointment(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task.Due.HasValue)
            {
                var appointment = new Appointment
                {
                    AllDay = true,
                    Subject = task.Title,
                    Details = task.Note
                };

                DateTime date = task.Due.Value;
                TimeSpan timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                DateTimeOffset startTime = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, timeZoneOffset);

                appointment.StartTime = startTime;

                string appointmentId = await AppointmentManager.ShowAddAppointmentAsync(
                    appointment, 
                    new Rect(20, 20, 100, 100), 
                    Placement.Default);
            }
        }
    }
}
