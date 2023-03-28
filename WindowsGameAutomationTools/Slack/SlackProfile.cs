namespace WindowsGameAutomationTools.Slack
{
    public class SlackProfile
    {
        public const string DEFAULT_FILENAME = "SlackProfile";
        public const string DEFAULT_CHANNEL_ID = "Paste ChannelId Here (make a post in the channel, Copy Link to message, grab channelId: https://SLACK_NAME_HERE.slack.com/archives/CHANNEL_ID_HERE/MESSAGE_ID_HERE)";
        public const string DEFAULT_OAUTH_TOKEN = "Paste Token Here (https://api.slack.com/apps/, Click on App Name, Copy Client Secret)";

        public string ChannelId { get; set; }
        public string OauthToken { get; set; }

        public SlackProfile()
        {
            ChannelId = DEFAULT_CHANNEL_ID;
            OauthToken = DEFAULT_OAUTH_TOKEN;
        }
    }
}
