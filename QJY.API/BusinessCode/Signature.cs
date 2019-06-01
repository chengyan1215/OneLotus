using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace QJY.API.BusinessCode
{
    public class Signature
    {
        public static string m_strSecId = "AKIDeGdy84zMfjdHkBnRXZTebNuLdhWIl68v";
        public static string m_strSecKey = "kHZfWHo570sNdoVxSlwDH6heLwIuIXXP";
        public static int m_iRandom;
        public static long m_qwNowTime;
        public static int m_iSignValidDuration;
        public static long GetIntTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        private static byte[] hash_hmac_byte(string signatureString, string secretKey)
        {
            var enc = Encoding.UTF8; HMACSHA1 hmac = new HMACSHA1(enc.GetBytes(secretKey));
            hmac.Initialize();
            byte[] buffer = enc.GetBytes(signatureString);
            return hmac.ComputeHash(buffer);
        }
        public static string GetUploadSignature()
        {
            m_qwNowTime = Signature.GetIntTimeStamp();
            m_iRandom = new Random().Next(0, 1000000);
            m_iSignValidDuration = 3600 * 24 * 2;

            string strContent = "";
            strContent += ("secretId=" + Uri.EscapeDataString((m_strSecId)));
            strContent += ("&currentTimeStamp=" + m_qwNowTime);
            strContent += ("&expireTime=" + (m_qwNowTime + m_iSignValidDuration));
            strContent += ("&random=" + m_iRandom);
            strContent += ("&procedure=wxzhuanma01"); //任务流
            

            byte[] bytesSign = hash_hmac_byte(strContent, m_strSecKey);
            byte[] byteContent = System.Text.Encoding.Default.GetBytes(strContent);
            byte[] nCon = new byte[bytesSign.Length + byteContent.Length];
            bytesSign.CopyTo(nCon, 0);
            byteContent.CopyTo(nCon, bytesSign.Length);
            return Convert.ToBase64String(nCon);
        }
    }


}


