using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MihaZupan;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Seq.App.Telegram.Alerts
{
    [SeqApp("Telegram notifier", Description = "Sends alerts to Telegram.")]
    public class TelegramReactor : SeqApp, ISubscribeToAsync<LogEventData>
    {
        [SeqAppSetting(
            DisplayName = "Bot authentication token",
            HelpText = "Refer to Telegram api documentation https://core.telegram.org/bots/api#authorizing-your-bot")]
        public string BotToken { get; set; }

        [SeqAppSetting(
            DisplayName = "Group chat identifier",
            HelpText = "Unique identifier for your group chat (include minus)")]
        public long ChatId { get; set; }

        [SeqAppSetting(
            DisplayName = "Seq Base URL",
            HelpText = "Used for generating permalinks to events in Telegram messages.",
            IsOptional = true)]
        public string BaseUrl { get; set; }

        [SeqAppSetting(
            DisplayName = "Suppression time (minutes)",
            IsOptional = true,
            HelpText = "Once an event type has been sent to Telegram, the time to wait before sending again. The default is zero.")]
        public int SuppressionMinutes { get; set; } = 0;

        [SeqAppSetting(DisplayName = "Socks5 proxy host name", IsOptional = true)]
        public string Socks5ProxyHost { get; set; }

        [SeqAppSetting(DisplayName = "Socks5 proxy port", IsOptional = true)]
        public int Socks5ProxyPort { get; set; }

        [SeqAppSetting(DisplayName = "Socks5 proxy username", IsOptional = true)]
        public string Socks5ProxyUserName { get; set; }

        [SeqAppSetting(DisplayName = "Socks5 proxy password", IsOptional = true, InputType = SettingInputType.Password)]
        public string Socks5ProxyPassword { get; set; }

        readonly Lazy<TelegramBotClient> _telegram;

        public TelegramReactor()
        {
            _telegram = new Lazy<TelegramBotClient>(CreateTelegramBotClient, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        TelegramBotClient CreateTelegramBotClient()
        {
            if (string.IsNullOrEmpty(Socks5ProxyHost))
                return new TelegramBotClient(BotToken);
            var proxy = string.IsNullOrEmpty(Socks5ProxyUserName)
                ? new HttpToSocks5Proxy(Socks5ProxyHost, Socks5ProxyPort)
                : new HttpToSocks5Proxy(Socks5ProxyHost, Socks5ProxyPort, Socks5ProxyUserName, Socks5ProxyPassword);
            return new TelegramBotClient(BotToken, new HttpClient(new HttpClientHandler { Proxy = proxy }));
        }

        private readonly Throttling<uint> _throttling = new Throttling<uint>();

        private string GetBaseUri() => (BaseUrl ?? Host.BaseUri).TrimEnd('/');

        public async Task OnAsync(Event<LogEventData> evt)
        {
            if (!_throttling.TryBegin(evt.EventType, TimeSpan.FromMinutes(SuppressionMinutes)))
                return;
            var formatter = new MessageFormatter(Log, GetBaseUri());
            var message = formatter.GenerateMessageText(evt);
            try
            {
                await _telegram.Value.SendTextMessageAsync(ChatId, message, ParseMode.Markdown);
            }
            catch (Exception e)
            {
                Log.Error(e, "{message}", message);
            }
        }
    }
}
