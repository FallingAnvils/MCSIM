using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cmpdl_wrapper
{
    public class ConsoleFileDownloader
    {
        private WebClient client;
        private ProgressBarWriter writer;
        private EventWaitHandle handle;

        public ConsoleFileDownloader()
        {
            this.client = new WebClient();
            this.writer = new ProgressBarWriter();
            this.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
        }

        public void DownloadWithName(string extraData, string url, string destFolder, string fileName)
        {
            writer.ExtraData = extraData + " " + fileName;
            client.DownloadFileAsync(new Uri(url), destFolder + "/" + fileName);
            handle.WaitOne();
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            handle.Set();
        }

        private int oldPercent = 0;

        private bool isPrinting = false;

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage != oldPercent)
            {
                oldPercent = e.ProgressPercentage;
                writer.Percent = e.ProgressPercentage;
                if(!isPrinting)writer.Write(ref isPrinting);
            }
        }
    }
}
