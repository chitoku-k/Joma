using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Joma.Models
{
    public class JomaDownloader
    {
        public static async Task GetBarcodeImage(string url)
        {
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 6_1_3 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.1.3 Mobile/10A403 Safari/8536.25";

            using (var response = await request.GetResponseAsync())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var result = reader.ReadToEnd();
                var barcodeNumber = Regex.Match(result, @"var barcode_number = ""([\w\d]+)"";").Groups[1].Value;
                var number = Regex.Match(url, @"exchange/(\d+)/").Groups[1].Value;
                await DownloadBarcodeImage(number, barcodeNumber);
            }
        }

        private static async Task DownloadBarcodeImage(string number, string code)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Joma", number + ".gif");
            await new WebClient().DownloadFileTaskAsync("http://sbg.jp/nw7/303/100/0/" + code + ".gif", path);
        }
    }
}
