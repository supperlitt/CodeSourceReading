using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace WinAndroidSourceReading
{
    public class BaiduTranslateHelper
    {
        private static JavaScriptSerializer js = new JavaScriptSerializer();

        public static BaiduTranslateRootobject Translate(string msg)
        {
            string url = "https://fanyi.baidu.com/v2transapi";
            string postData = "from=en&to=zh&query=hello+world&transtype=realtime&simple_means_flag=3&sign=288018.34339&token=";

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                client.Headers.Add("X-Requested-With", "XMLHttpRequest");
                string result = client.UploadString(url, postData);
                var model = js.Deserialize<BaiduTranslateRootobject>(result);

                return model;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="appid">
        /// 作者的appid 真实使用请更换成自己的appid 防止变更后无法使用
        /// </param>
        /// <param name="miyao">
        /// 作者的秘钥 真实使用请更换成自己的秘钥 防止变更后无法使用
        /// </param>
        /// <returns></returns>
        public static BaiduApiInfo Translate_ByAPI(string msg, string appid = "20181016000219930", string miyao = "V_D1MswE9rJhU0PCLH1a")
        {
            // appid+q+salt+密钥 的MD5值
            int salt = (int)JsTool.GetIntFromTime();
            string sign = JsTool.GetMD5String(appid + msg + salt + miyao).ToLower();
            string url = "http://fanyi-api.baidu.com/api/trans/vip/translate";
            string postData = string.Format("from=en&to=zh&q={0}&salt={1}&sign={2}&appid={3}", HttpUtility.UrlEncode(msg), salt, sign, appid);

            using (TMWebClient client = new TMWebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                string result = client.UploadString(url, postData);
                var model = js.Deserialize<BaiduApiInfo>(result);

                return model;
            }
        }
    }
}
