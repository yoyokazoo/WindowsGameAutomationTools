using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlackAPI;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using WindowsGameAutomationTools.Logging;
using System.Text.Json;
using WindowsGameAutomationTools.Files;

namespace WindowsGameAutomationTools.Slack
{
    public static class SlackHelper
    {
        private static SlackClient _client;
        private static SlackClient Client
        {
            get
            {
                if (_client == null)
                {
                    (ChannelId, OauthToken) = LoadSlackProfile();
                    _client = new SlackClient(OauthToken);
                }
                return _client;
            }

            set
            {
                _client = value;
            }
        }

        // TODO: Some way to store this other than plaintext
        private static string ChannelId { get; set; }
        private static string OauthToken { get; set; }

        public static void SendMessageToChannel(string message, Action<PostMessageResponse> callback = null, string thread_ts = null)
        {
            Client.PostMessage(callback, ChannelId, message, thread_ts: thread_ts);
        }

        public static void SendScreenshotToChannel(
            string fileExtension = null,
            string fileName = null,
            string fileTitle = null,
            string fileComment = null,
            string thread_ts = null)
        {
            fileExtension = fileExtension ?? ".bmp";
            fileName = fileName ?? $"fileName{fileExtension}";
            fileTitle = fileTitle ?? "fileTitle";
            fileComment = fileComment ?? "fileComment";

            Rectangle rect = Screen.PrimaryScreen.Bounds;
            using (Bitmap bitmap = new Bitmap(rect.Width, rect.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(rect.Left, rect.Top), Point.Empty, rect.Size);
                }

                ImageConverter converter = new ImageConverter();
                byte[] bitmapAsBytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

                UploadFile(
                    (asdf) => { Console.WriteLine(asdf.error); },
                    bitmapAsBytes,
                    fileName,
                    new string[] { ChannelId },
                    fileTitle,
                    fileComment,
                    useAsync: false,
                    fileExtension,
                    thread_ts);
            }
        }

        public static void SendStringAsFileToChannel(
            string stringToBeFile,
            string fileExtension = null,
            string fileName = null,
            string fileTitle = null,
            string fileComment = null,
            string thread_ts = null)
        {
            fileExtension = fileExtension ?? ".txt";
            fileName = fileName ?? $"fileName{fileExtension}";
            fileTitle = fileTitle ?? "fileTitle";
            fileComment = fileComment ?? "fileComment";

            byte[] stringAsBytes = Encoding.ASCII.GetBytes(stringToBeFile);

            UploadFile(
                (result) => { /*Console.WriteLine(result.error);*/ },
                stringAsBytes,
                fileName,
                new string[] { ChannelId },
                fileTitle,
                fileComment,
                useAsync: false,
                fileExtension,
                thread_ts);
        }

        public static void SendLoggerAsFileToChannel(
            FileLogger logger,
            string fileTitle = null,
            string fileComment = null,
            string thread_ts = null)
        {
            fileTitle = fileTitle ?? "fileTitle";
            fileComment = fileComment ?? "fileComment";

            byte[] fileAsBytes = System.IO.File.ReadAllBytes(logger.LoggerFileName);

            UploadFile(
                (result) => { /*Console.WriteLine(result.error);*/ },
                fileAsBytes,
                logger.LoggerFileName,
                new string[] { ChannelId },
                fileTitle,
                fileComment,
                useAsync: false,
                FileLogger.LOG_FILE_EXTENSION,
                thread_ts);
        }

        #region OauthToken

        public static (string, string) LoadSlackProfile()
        {
            try
            {
                var profile = SerializationHelper.DeserializeFromJSON<SlackProfile>(SlackProfile.DEFAULT_FILENAME);

                if (SlackProfile.DEFAULT_CHANNEL_ID.Equals(profile.ChannelId))
                {
                    throw new Exception($"Default ChannelId found in SlackProfile.  Add ChannelId otherwise SlackHelper won't function.");
                }

                if (SlackProfile.DEFAULT_OAUTH_TOKEN.Equals(profile.OauthToken))
                {
                    throw new Exception($"Default OauthToken found in SlackProfile.  Add OauthToken otherwise SlackHelper won't function.");
                }

                return (profile.ChannelId, profile.OauthToken);
            }
            catch(FileNotFoundException fnfe)
            {
                CreateEmptySlackProfile();
                throw new FileNotFoundException($"File not found {fnfe.Message}  Created empty profile, fill in with Oauth token before running again.");
            }
        }

        public static void CreateEmptySlackProfile()
        {
            SerializationHelper.SerializeToJSON(SlackProfile.DEFAULT_FILENAME, new SlackProfile());
        }

        #endregion

        #region SlackClient Extensions
        // Extended methods from the SlackClient -- I just needed thread_ts!
        public static void UploadFile(Action<FileUploadResponse> callback, byte[] fileData, string fileName, string[] channelIds, string title = null, string initialComment = null, bool useAsync = false, string fileType = null, string thread_ts = null)
        {
            Uri arg = new Uri(Path.Combine(Client.APIBaseLocation, useAsync ? "files.uploadAsync" : "files.upload"));
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(fileType))
            {
                list.Add(string.Format("{0}={1}", "filetype", fileType));
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                list.Add(string.Format("{0}={1}", "filename", fileName));
            }

            if (!string.IsNullOrEmpty(title))
            {
                list.Add(string.Format("{0}={1}", "title", title));
            }

            if (!string.IsNullOrEmpty(initialComment))
            {
                list.Add(string.Format("{0}={1}", "initial_comment", initialComment));
            }

            if (!string.IsNullOrEmpty(thread_ts))
            {
                list.Add(string.Format("{0}={1}", "thread_ts", thread_ts));
            }

            list.Add(string.Format("{0}={1}", "channels", string.Join(",", channelIds)));
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            multipartFormDataContent.Add(new ByteArrayContent(fileData), "file", fileName);
            string result = PostRequestAsync(string.Format("{0}?{1}", arg, string.Join("&", list.ToArray())), multipartFormDataContent, OauthToken).Result.Content.ReadAsStringAsync().Result;
            callback(result.Deserialize<FileUploadResponse>());
        }

        public static Task<HttpResponseMessage> PostRequestAsync(string requestUri, MultipartFormDataContent form, string token)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = form,
                RequestUri = new Uri(requestUri)
            };
            HttpClient httpClient = new HttpClient();
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpClient.SendAsync(httpRequestMessage);
        }
        #endregion
    }
}
