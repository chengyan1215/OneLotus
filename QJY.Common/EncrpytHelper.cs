using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using System.Security.Cryptography;

namespace QJY.Common
{
    /// <summary>
    /// 密码加密助手类
    /// </summary>
    public static class EncrpytHelper
    {
        #region
        private static string EncrpytKey
        {
            get
            {
                return  CommonHelp.GetConfig("EncrpytKey", "qijiekeji2016");
            }
        }

        /// <summary>
        /// 3des加密字符串
        /// </summary>
        /// <param name="text">要加密的字符串</param>
        /// <returns>加密后并经base64编码的字符串</returns>
        /// <remarks>静态方法，采用默认ascii编码</remarks>
        public static string Encrypt(string text)
        {
            return Encrypt(text, EncrpytKey);
        }


        /// <summary>
        /// 3des加密字符串
        /// </summary>
        /// <param name="text">要加密的字符串</param>
        /// <param name="key">密钥</param>
        /// <returns>加密后并经base64编码的字符串</returns>
        /// <remarks>静态方法，采用默认ascii编码</remarks>
        public static string Encrypt(string text, string key)
        {
            TripleDESCryptoServiceProvider DES = new
                TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();

            DES.Key = hashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));
            DES.Mode = CipherMode.ECB;

            ICryptoTransform DESEncrypt = DES.CreateEncryptor();

            byte[] Buffer = ASCIIEncoding.UTF8.GetBytes(text);
            string pass = Base64Helper.ToBase64String(DESEncrypt.TransformFinalBlock
                (Buffer, 0, Buffer.Length));
            return pass;
        }//end method


        /// <summary>
        /// 3des解密字符串
        /// </summary>
        /// <param name="text">要解密的字符串</param>
        /// <returns>解密后的字符串</returns>
        /// <exception cref="">密钥错误</exception>
        /// <remarks>静态方法，采用默认ascii编码</remarks>
        public static string Decrypt(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            else
            {
                text = text.Trim().Replace(' ', '+');//处理Request的+号变空格问题。
                return Decrypt(text, EncrpytKey);
            }
        }//end method


        /// <summary>
        /// 3des解密字符串
        /// </summary>
        /// <param name="text">要解密的字符串</param>
        /// <param name="key">密钥</param>
        /// <returns>解密后的字符串</returns>
        /// <exception cref="">密钥错误</exception>
        /// <remarks>静态方法，采用默认ascii编码</remarks>
        public static string Decrypt(string text, string key)
        {
            TripleDESCryptoServiceProvider DES = new
                TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();

            DES.Key = hashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));
            DES.Mode = CipherMode.ECB;

            ICryptoTransform DESDecrypt = DES.CreateDecryptor();

            string result = "";
            try
            {
                byte[] Buffer = Base64Helper.FromBase64String(text);
                result = ASCIIEncoding.UTF8.GetString(DESDecrypt.TransformFinalBlock
                    (Buffer, 0, Buffer.Length));
            }
            catch
            {
                return text;
            }

            return result;
        }






        /// <summary>
        /// 基于Sha1的自定义加密字符串方法：输入一个字符串，返回一个由40个字符组成的十六进制的哈希散列（字符串）。
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public static string Sha1(this string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);

            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// sha1 加密，与PHP加密结果一样
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Sha1Sign(this string data)
        {
            byte[] temp1 = Encoding.UTF8.GetBytes(data);
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] temp2 = sha.ComputeHash(temp1);
            sha.Clear();            // 注意， 不能用这个           
                                    // string output = Convert.ToBase64String(temp2);// 不能直接转换成 base64string            
            var output = BitConverter.ToString(temp2);
            output = output.Replace("-", "");
            output = output.ToLower();
            return output;
        }
        #endregion
    }


    public static class Base64Helper
    {
       // static readonly string base64Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
        static readonly string base64Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        static readonly int[] base64Index = new int[]
        {
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,63,-1,-1,52,53,54,55,56,57,58,59,60,61,-1,-1,-1,-1,-1,-1,-1,0,1,
            2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,-1,
            -1,-1,-1,62,-1,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,
            43,44,45,46,47,48,49,50,51,-1,-1,-1,-1,-1,-1
        };
        public static byte[] FromBase64String(string inData)
        {
            int inDataLength = inData.Length;
            int lengthmod4 = inDataLength % 4;
            int calcLength = (inDataLength - lengthmod4);
            byte[] outData = new byte[inDataLength / 4 * 3 + 3];
            int j = 0;
            int i;
            int num1, num2, num3, num4;

            for (i = 0; i < calcLength; i += 4, j += 3)
            {
                num1 = base64Index[inData[i]];
                num2 = base64Index[inData[i + 1]];
                num3 = base64Index[inData[i + 2]];
                num4 = base64Index[inData[i + 3]];

                outData[j] = (byte)((num1 << 2) | (num2 >> 4));
                outData[j + 1] = (byte)(((num2 << 4) & 0xf0) | (num3 >> 2));
                outData[j + 2] = (byte)(((num3 << 6) & 0xc0) | (num4 & 0x3f));
            }
            i = calcLength;
            switch (lengthmod4)
            {
                case 3:
                    num1 = base64Index[inData[i]];
                    num2 = base64Index[inData[i + 1]];
                    num3 = base64Index[inData[i + 2]];

                    outData[j] = (byte)((num1 << 2) | (num2 >> 4));
                    outData[j + 1] = (byte)(((num2 << 4) & 0xf0) | (num3 >> 2));
                    j += 2;
                    break;
                case 2:
                    num1 = base64Index[inData[i]];
                    num2 = base64Index[inData[i + 1]];

                    outData[j] = (byte)((num1 << 2) | (num2 >> 4));
                    j += 1;
                    break;
            }
            Array.Resize(ref outData, j);
            return outData;
        }
        public static string ToBase64String(byte[] inData)
        {
            int inDataLength = inData.Length;
            int outDataLength = (int)(inDataLength / 3 * 4) + 4;
            char[] outData = new char[outDataLength];

            int lengthmod3 = inDataLength % 3;
            int calcLength = (inDataLength - lengthmod3);
            int j = 0;
            int i;

            for (i = 0; i < calcLength; i += 3, j += 4)
            {
                outData[j] = base64Table[inData[i] >> 2];
                outData[j + 1] = base64Table[((inData[i] & 0x03) << 4) | (inData[i + 1] >> 4)];
                outData[j + 2] = base64Table[((inData[i + 1] & 0x0f) << 2) | (inData[i + 2] >> 6)];
                outData[j + 3] = base64Table[(inData[i + 2] & 0x3f)];
            }

            i = calcLength;
            switch (lengthmod3)
            {
                case 2:
                    outData[j] = base64Table[inData[i] >> 2];
                    outData[j + 1] = base64Table[((inData[i] & 0x03) << 4) | (inData[i + 1] >> 4)];
                    outData[j + 2] = base64Table[(inData[i + 1] & 0x0f) << 2];
                    j += 3;
                    break;
                case 1:
                    outData[j] = base64Table[inData[i] >> 2];
                    outData[j + 1] = base64Table[(inData[i] & 0x03) << 4];
                    j += 2;
                    break;
            }
            return new string(outData, 0, j);
        }
        public static string Base64Encode(string source)
        {
            byte[] barray = Encoding.Default.GetBytes(source);
            return Base64Helper.ToBase64String(barray);
        }
        public static string Base64Decode(string source)
        {
            byte[] barray = Base64Helper.FromBase64String(source);
            return Encoding.Default.GetString(barray);
        }
    }




}
