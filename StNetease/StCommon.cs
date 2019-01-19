using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Globalization;

namespace StNetease
{
    public static class StCommon
    {
        public static byte[] Decompress_GZip(byte[] src)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(src), CompressionMode.Decompress))
            {
                byte[] buffer = new byte[1024];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, buffer.Length);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    return memory.ToArray();
                }

            }
        }
        public static string EncryptByPublicKey(string dataStr, string n, string e)
        {
            BigInteger biN = BigInteger.Parse(n, NumberStyles.HexNumber);
            BigInteger biE = BigInteger.Parse(e, NumberStyles.HexNumber);
            string result = EncryptString(dataStr, biE, biN);
            while (true)
            {
                if (result.First() == '0' && result.Length % 2 != 0)
                    result = result.Substring(1);
                else
                {
                    break;
                }
            }
            return result;
        }
        public static string EncryptString(string dataStr, BigInteger keyNum, BigInteger nNum)
        {
            byte[] bys = Encoding.UTF8.GetBytes(dataStr);
            int len = bys.Length;
            int len1 = 0;
            if ((len % 120) == 0)
                len1 = len / 120;
            else
                len1 = len / 120 + 1;
            List<byte> tempbys = new List<byte>();
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < len1; i++)
            {
                int blockLen = len;
                if(len >= 120)
                {
                    blockLen = 120;
                }
                byte[] oText = new byte[blockLen];
                Array.Copy(bys, i * 120, oText, 0, blockLen);
                string res = Encoding.UTF8.GetString(oText);
                BigInteger biText = new BigInteger(oText);
                BigInteger biEnText = BigInteger.ModPow(biText, keyNum, nNum);
                string resultStr = biEnText.ToString("x");
                if(resultStr.Length < 256)
                {
                    for(int j = resultStr.Length; j < 256; j++)
                    {
                        sb.Append('0');
                    }
                }
                sb.Append(resultStr);
            }
            return sb.ToString();
        }
    }
}
