using System;
using System.Runtime.Serialization;
using Microsoft.Phone.Tasks;


namespace WPCordovaClassLib.Cordova.Commands
{
    [DataContract]
    public class OpenBrowserOptions
    {
        [DataMember]
        public string url;
    }

    public class OpenBrowserCommand : BaseCommand
    {
        public void openWebPage(string options)
        {
            OpenBrowserOptions opts = JSON.JsonHelper.Deserialize<OpenBrowserOptions>(options);

            Uri loc = new Uri(opts.url);
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(opts.url);
            webBrowserTask.Show();
        }
    }
}
