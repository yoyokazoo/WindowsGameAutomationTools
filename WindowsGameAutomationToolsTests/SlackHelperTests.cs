using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using WindowsGameAutomationTools.Files;
using WindowsGameAutomationTools.Slack;

namespace WindowsGameAutomationToolsTests
{
    [TestClass]
    public class SlackHelperTests
    {
        [TestInitialize()]
        public void Startup()
        {
            string profileFilename = $"{SlackProfile.DEFAULT_FILENAME}{SerializationHelper.JSON_SUFFIX}";
            if (File.Exists(profileFilename))
            {
                File.Delete(profileFilename);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void MissingSlackProfileTest()
        {
            SlackHelper.SendMessageToChannel("Message");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void MissingChannelIdTest()
        {
            SlackProfile profile = new SlackProfile();
            SerializationHelper.SerializeToJSON(SlackProfile.DEFAULT_FILENAME, profile);

            SlackHelper.SendMessageToChannel("Message");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void MissingOauthTokenTest()
        {
            SlackProfile profile = new SlackProfile();
            profile.ChannelId = "ChannelId";
            SerializationHelper.SerializeToJSON(SlackProfile.DEFAULT_FILENAME, profile);

            SlackHelper.SendMessageToChannel("Message");
        }

        [TestMethod]
        public void SuccessTest()
        {
            SlackProfile profile = new SlackProfile();
            profile.ChannelId = "ChannelId";
            profile.OauthToken = "OauthToken";
            SerializationHelper.SerializeToJSON(SlackProfile.DEFAULT_FILENAME, profile);

            SlackHelper.SendMessageToChannel("Message");
        }
    }
}
