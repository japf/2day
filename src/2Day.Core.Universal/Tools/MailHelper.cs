using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class MailHelper
    {
        public static async Task AddAttachment(EmailMessage email, string filename, string name)
        {
            try
            {
                StorageFile attachmentFileCopy = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);

                BasicProperties properties = await attachmentFileCopy.GetBasicPropertiesAsync();
                if (properties.Size > 0)
                {
                    await attachmentFileCopy.CopyAsync(ApplicationData.Current.LocalFolder, filename + ".bak", NameCollisionOption.ReplaceExisting);

                    StorageFile attachmentFile = await ApplicationData.Current.LocalFolder.GetFileAsync(filename + ".bak");
                    if (attachmentFile != null)
                    {
                        var stream = RandomAccessStreamReference.CreateFromFile(attachmentFile);
                        var attachment = new EmailAttachment(name, stream);
                        email.Attachments.Add(attachment);
                    }
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Adding attachment");
            }            
        }
    }
}
