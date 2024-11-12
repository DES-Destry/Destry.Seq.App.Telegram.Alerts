using System;
using System.IO;
using Seq.Syntax.Templates;
using Serilog;
using Serilog.Events;

namespace Destry.Seq.App.Telegram.Alerts
{
    public class MessageFormatter(ILogger log, string baseUrl, string messageTemplate)
    {
        private readonly string _messageTemplate = messageTemplate ?? "[RenderedMessage]";
        
        public ILogger Log { get; } = log;
        public string BaseUrl { get; } = baseUrl;

        public string GenerateMessageText(LogEvent evt) => Format(new ExpressionTemplate(_messageTemplate), evt);
        
        private static string Format(ExpressionTemplate template, LogEvent evt)
        {
            ArgumentNullException.ThrowIfNull(template);
            ArgumentNullException.ThrowIfNull(evt);
            
            var writer = new StringWriter();
            template.Format(evt, writer);
            return writer.ToString();
        }
    }
}
