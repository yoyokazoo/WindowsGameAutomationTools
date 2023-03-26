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
                    //ContestEntryProfile profile = SerializationHelper.DeserializeFromJSON<ContestEntryProfile>(ContestEntryProfile.DEFAULT_PROFILE_FILENAME);
                    //OauthToken = profile.SlackOauthToken;

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
        private const string OauthTokenFilename = "SlackHelperOauthToken.txt";
        private static string OauthToken { get; set; }

        public static void SendMessageToChannel(string message, string channelId, Action<PostMessageResponse> callback = null, string thread_ts = null)
        {
            Client.PostMessage(callback, channelId, message, thread_ts: thread_ts);
        }

        public static void SendScreenshotToChannel(
            string channelId,
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
                    new string[] { channelId },
                    fileTitle,
                    fileComment,
                    useAsync: false,
                    fileExtension,
                    thread_ts);
            }
        }

        public static void SendStringAsFileToChannel(
            string channelId,
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
                new string[] { channelId },
                fileTitle,
                fileComment,
                useAsync: false,
                fileExtension,
                thread_ts);
        }

        public static void SendLoggerAsFileToChannel(
            string channelId,
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
                new string[] { channelId },
                fileTitle,
                fileComment,
                useAsync: false,
                FileLogger.LOG_FILE_EXTENSION,
                thread_ts);
        }

        #region OAuthToken

        public static string LoadOauthTokenFromFile()
        {
            return null;
        }

        public static void CreateOauthTokenFile()
        {

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
