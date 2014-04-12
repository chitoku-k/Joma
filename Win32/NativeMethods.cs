using System.Runtime.InteropServices;

namespace Joma.Win32
{
    internal class NativeMethods
    {
        /// <summary>
        /// Creates a cookie associated with the specified URL.
        /// </summary>
        /// <param name="lpszUrl">The URL for which the cookie should be set.</param>
        /// <param name="lpszCookieName">The name to be associated with the cookie data.</param>
        /// <param name="lpszCookieData">The actual data to be associated with the URL.</param>
        /// <returns></returns>
        [DllImport("wininet.dll")]
        internal extern static bool InternetSetCookie(string lpszUrl, string lpszCookieName, string lpszCookieData);
    }
}
