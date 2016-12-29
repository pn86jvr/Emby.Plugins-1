using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Services;
using MediaBrowser.Plugins.TeleGramNotifications.Configuration;

namespace MediaBrowser.Plugins.TeleGramNotifications.Api
{
    [Route("/Notification/Telegram/Test/{UserID}", "POST", Summary = "Tests Telegram")]
    public class TestNotification : IReturnVoid
    {
        [ApiMember(Name = "UserID", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string UserID { get; set; }
    }

    class ServerApiEndpoints : IService
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public ServerApiEndpoints(ILogManager logManager, IHttpClient httpClient)
        {
            _logger = logManager.GetLogger(GetType().Name);
            _httpClient = httpClient;
        }
        private TeleGramOptions GetOptions(String userID)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.MediaBrowserUserId, userID, StringComparison.OrdinalIgnoreCase));
        }

        public object Post(TestNotification request)
        {
            var options = GetOptions(request.UserID);

            var parameters = new Dictionary<string, string>
            {
                {"bottoken", options.BotToken},
                {"chat_id", options.ChatID},
                {"text", "This is a test notification from MediaBrowser"}
            };

            _logger.Debug("Telegram <TEST> to {0} - {1}", options.BotToken, options.ChatID);

            return _httpClient.Post(new HttpRequestOptions { Url = "https://api.telegram.org/bot" }, parameters);
        }
    }
}
