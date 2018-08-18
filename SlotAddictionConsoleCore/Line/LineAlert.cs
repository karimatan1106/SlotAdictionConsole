using System.Net;
using System.Text;
using System.Web;

namespace SlotAddictionLogic.Line
{
    public static class LineAlert
    {
        public static void Message(string token, string notifyMessage)
        {
            var enc = Encoding.UTF8;
            var payload = "message=" + HttpUtility.UrlEncode(notifyMessage, enc); ;

            using (var wc = new WebClient())
            {
                wc.Encoding = enc;
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                wc.Headers.Add("Authorization", "Bearer " + token);
                wc.UploadString("https://notify-api.line.me/api/notify", payload);
            }
        }
    }
}
