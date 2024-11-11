using System.IO;
using Seq.Syntax.Templates;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;

namespace Seq.App.Telegram.Alerts
{
    public class MessageFormatter
    {
        public MessageFormatter(ILogger log, string baseUrl, string messageTemplate)
        {
            Log = log;
            BaseUrl = baseUrl;
            MessageTemplate = messageTemplate ?? "[RenderedMessage]";
        }

        public ILogger Log { get; }
        public string MessageTemplate { get; }
        public string BaseUrl { get; }
        
        static string Format(ITextFormatter template, LogEvent evt)
        {
            var writer = new StringWriter();
            template.Format(evt, writer);
            return writer.ToString();
        }

        public string GenerateMessageText(LogEvent evt)
        {
            return Format(new ExpressionTemplate(MessageTemplate), evt);
        }
    }
}
