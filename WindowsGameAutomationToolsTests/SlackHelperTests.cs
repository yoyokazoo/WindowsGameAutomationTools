using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WindowsGameAutomationTools.Files;
using WindowsGameAutomationTools.Slack;

namespace WindowsGameAutomationToolsTests
{
    [TestClass]
    public class SlackHelperTests
    {
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void MissingSlackProfileTest()
        {
            string profileFilename = $"{SlackProfile.DEFAULT_FILENAME}{SerializationHelper.JSON_SUFFIX}";
            if (File.Exists(profileFilename))
            {
                File.Delete(profileFilename);
            }

            SlackHelper.SendMessageToChannel("Message");
        }
    }
}
