using Joma.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Joma.Models
{
    public class JomaChallenge
    {
        private static CookieContainer container = new CookieContainer();
        private static Random random = new Random();

        private static async Task<HttpWebResponse> PostData(string url, string data, string referer)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.Referer = referer;
            request.CookieContainer = container;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = await request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
                return await request.GetResponseAsync() as HttpWebResponse;
            }
        }

        public static void OpenApplyPage(ApplyPageType type)
        {
            switch (type)
            {
                case ApplyPageType.Twitter:
                    Process.Start("https://gift2.jomajoma.com/apply/tw/?flg=tw_reg");
                    break;
                case ApplyPageType.Facebook:
                    Process.Start("https://gift2.jomajoma.com/apply/fb/?flg=fb_reg");
                    break;
            }
        }

        public static async Task<string> GetGiftPage()
        {
            var request = HttpWebRequest.Create("https://gift.jomajoma.com/drawing/index2.php") as HttpWebRequest;
            request.CookieContainer = container;
            request.Referer = "https://gift.jomajoma.com";

            using (var response = await request.GetResponseAsync())
            {
                if (response.ResponseUri.AbsoluteUri == "https://gift.jomajoma.com/drawing/index.php")
                    return null;
                else
                    return await PostConfirmApplyPage(response.ResponseUri.AbsoluteUri);
            }
        }

        private static async Task<string> PostConfirmApplyPage(string referer)
        {
            var code = string.Format("{0}{1}106", random.Next(10000, 99999), random.Next(10000, 99999));

            using (var response = await PostData("https://gift.jomajoma.com/apply/index.php?type=rgst", "gete_a=" + code, referer))
            {
                return await PostApplyPage(response.ResponseUri.AbsoluteUri);
            }
        }

        private static async Task<string> PostApplyPage(string referer)
        {
            using (var response = await PostData("https://gift.jomajoma.com/apply/form.php?flg=ml_reg", "q1=%e3%81%9d%e3%81%ae1", referer))
            {
                return await PostMailApplyFormPage(response.ResponseUri.AbsoluteUri);
            }
        }

        private static async Task<string> PostMailApplyFormPage(string referer)
        {
            var info = "mail01=your_email%2bjoma" + random.Next() + "@gmail.com&pass01=your_password&pass02=your_password&regist=";

            using (var response = await PostData("https://gift.jomajoma.com/apply/form.php", info, referer))
            {
                return await PostConfirmMailApplyFormPage(response.ResponseUri.AbsoluteUri);
            }
        }

        private static async Task<string> PostConfirmMailApplyFormPage(string referer)
        {
            using (var response = await PostData("https://gift.jomajoma.com/apply/confirm.php", "regist.x=0&regist.y=0&regist=", referer))
            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                var result = reader.ReadToEnd();
                var url = Regex.Match(result, @"(http:\/\/sbg\.jp\/cp\/exchange\/[\d\w\/]+)").Groups[1].Value;

                if (string.IsNullOrWhiteSpace(url))
                    throw new UriFormatException();

                return url;
            }
        }
    }

    public enum ApplyPageType
    {
        Twitter,
        Facebook
    }
}