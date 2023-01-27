using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Serilog;

namespace Seq.App.Telegram.Alerts
{
    public class MessageFormatter
    {
        public MessageFormatter(ILogger log, string baseUrl)
        {
            Log = log;
            BaseUrl = baseUrl;
        }

        public ILogger Log { get; }
        public string BaseUrl { get; }

        public string GenerateMessageText(Event<LogEventData> evt)
        {
            var data = evt.Data;

            if (data.Properties is null)
            {
                return "No properties were specified for an event";
            }
            
            if (data.Properties.ContainsKey("Source") && data.Properties.ContainsKey("Alert"))
            {
                dynamic source = data.Properties["Source"];
                dynamic alert = data.Properties["Alert"];

                dynamic alertTitle = alert["Title"];
                dynamic alertUrl = alert["Url"];

                dynamic[] contributingEvents = source["ContributingEvents"];

                // the first one is column titles e.g [id, timestamp, message]
                IEnumerable<dynamic> contributingEventsData = contributingEvents.Skip(1);

                return $"**[Alert]({alertUrl}) \"{alertTitle}\" was triggered**\n" +
                       $"Contributing Events:\n" +
                       string.Join(
                           "\n",
                           contributingEventsData.Select(
                               (e, i) =>
                                   $"[{i} ({e[2]})]({BaseUrl}/#/events?filter=@Id%3D%3D'{e[0]}'&show=expanded)"
                           )
                       );
            }
            else
            {
                return "Event Properties didn't contain Source and Alert fields!";
            }
        }
    }
}
