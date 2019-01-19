using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace StNetease
{
    public class NeteaseMusicAPI
    {
        public string AesKey => "0CoJUm6Qyw8W8jud";
        public string PubKey => "010001";
        public string Modulus => "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7";
        public string RandomSet => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private HttpClient httpClient;
        public NeteaseMusicAPI()
        {
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A356 Safari/604.1");
            this.httpClient.DefaultRequestHeaders.Add("Host", "music.163.com");
        }
        private string EncryptAes(string src, string key)
        {
            Aes encrypt = new AesCryptoServiceProvider();
            encrypt.Mode = CipherMode.CBC;
            encrypt.Padding = PaddingMode.PKCS7;
            encrypt.IV = Encoding.UTF8.GetBytes("0102030405060708");
            encrypt.Key = Encoding.UTF8.GetBytes(key);
            ICryptoTransform encryptor = encrypt.CreateEncryptor();
            byte[] data = Encoding.UTF8.GetBytes(src);
            byte[] result = encryptor.TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(result);
        }
        private string RandomKey()
        {
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < 16; i++)
            {
                sb.Append(RandomSet[rand.Next(0, RandomSet.Length)]);
            }
            return sb.ToString();
        }
        protected string HttpPost(string url, HttpContent content)
        {
            HttpResponseMessage result = this.httpClient.PostAsync(url, content).Result;
            if (result.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                byte[] rawResult = result.Content.ReadAsByteArrayAsync().Result;
                byte[] finResult = StCommon.Decompress_GZip(rawResult);
                return Encoding.UTF8.GetString(finResult);
            }
            return result.Content.ReadAsStringAsync().Result;
        }
        protected string HttpGet(string url)
        {
            HttpResponseMessage result = this.httpClient.GetAsync(url).Result;
            if (result.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                byte[] rawResult = result.Content.ReadAsByteArrayAsync().Result;
                byte[] finResult = StCommon.Decompress_GZip(rawResult);
                return Encoding.UTF8.GetString(finResult);
            }
            return result.Content.ReadAsStringAsync().Result;
        }
        public string SendData(string url, string content, string key = null)
        {
            string encrypted = EncryptAes(content, AesKey);
            key = key ?? RandomKey();
            encrypted = EncryptAes(encrypted, key);
            string encrypedKey = StCommon.EncryptByPublicKey(key, Modulus, PubKey);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("params", encrypted);
            dict.Add("encSecKey", encrypedKey);
            FormUrlEncodedContent form = new FormUrlEncodedContent(dict);
            return this.HttpPost(url, form);
        }
        /// <summary>
        /// 获取歌词
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lv"></param>
        /// <param name="tv"></param>
        /// <returns></returns>
        public JObject GetLyric(int id, int lv = -1, int tv = -1)
        {
            JObject json = new JObject()
            {
                {"id", id },
                {"lv", lv },
                {"tv", tv },
                {"csrf_token", "" }
            };
            string result = this.SendData("http://music.163.com/weapi/song/lyric", json.ToString());
            return JObject.Parse(result);
        }

        public JObject GetComments(string id, int offset = 0, bool total = true, int limit = 1000)
        {
            JObject josn = new JObject()
            {
                {"rid", id },
                {"offset", offset },
                {"total", total },
                {"limit", limit },
                {"csrf_token", "" }
            };
            string result = this.SendData($"http://music.163.com/weapi/v1/resource/comments/{id}", josn.ToString());
            return JObject.Parse(result);
        }
        public JObject GetSongComments(int id, int offset = 0, bool total = true, int limit = 1000)
        {
            return this.GetComments($"R_SO_4_{id}", offset, total, limit);
        }
        public JObject GetPlayListComments(int id, int offset = 0, bool total = true, int limit = 1000)
        {
            return this.GetComments($"R_AL_0_{id}", offset, total, limit);
        }
        public JObject GetAlbumComments(int id, int offset = 0, bool total = true, int limit = 1000)
        {
            return this.GetComments($"R_AL_3_{id}", offset, total, limit);
        }
        private static readonly Regex jsonfinder = new Regex("window\\.REDUX_STATE = ([^\n]+});");
        public JObject GetUserBriefInfo(int uid)
        {
            string result = this.HttpGet($"http://music.163.com/user/home?id={uid}");
            Match match = jsonfinder.Match(result);
            if (!match.Success)
                return null;
            string jsontxt = match.Groups[1].Value;
            JObject json = JObject.Parse(jsontxt);
            return json["User"]["info"]["profile"] as JObject;

        }
        public JObject GetUserPlayRecords(int uid, int type = -1, int offset = 0, bool total = true, int limit = 1000)
        {
            JObject json = new JObject()
            {
                {"uid", uid },
                {"type", type},
                {"limit", limit },
                {"offset", offset },
                {"total", total },
                {"csrf_token", "" }
            };
            string result = this.SendData($"http://music.163.com/weapi/v1/play/record", json.ToString());
            return JObject.Parse(result);
        }
        public JObject GetUserFollowers(int uid, int offset = 0, bool total = true, int limit = 20)
        {
            JObject json = new JObject()
            {
                {"userId", uid },
                {"offset", offset },
                {"total", total },
                {"limit", limit },
                {"csrf_token", "" }
            };
            string result = this.SendData("http://music.163.com/weapi/user/getfolloweds", json.ToString());
            return JObject.Parse(result);
        }
        public JObject GetUserFollow(int uid, int offset = 0, bool total = true, int limit = 20)
        {
            JObject json = new JObject()
            {
                {"uid", uid },
                {"offset", offset },
                {"total", total },
                {"limit", limit },
                {"csrf_token", "" }
            };
            string result = this.SendData($"http://music.163.com/weapi/user/getfollows/{uid}", json.ToString());
            return JObject.Parse(result);
        }
        public JObject GetUserEvents(int uid, bool total = true, int limit = 20, int time= -1, bool getcounts = true)
        {
            JObject json = new JObject()
            {
                {"userId", uid },
                {"total", total },
                {"limit", limit },
                {"time", time },
                {"getcounts", getcounts },
                {"csrf_token", "" }
            };
            string result = this.SendData($"http://music.163.com/weapi/event/get/{uid}", json.ToString());
            return JObject.Parse(result);
        }
        public JObject GetUserPlaylists(int uid, int wordwarp = 7, int offset = 0, bool total = true, int limit = 36)
        {
            JObject json = new JObject()
            {
                {"uid", uid },
                {"wordwarp", wordwarp },
                {"offset", offset },
                {"total", total },
                {"limit", limit },
                {"csrf_token", "" }
            };
            string result = this.SendData("http://music.163.com/weapi/user/playlist", json.ToString());
            return JObject.Parse(result);
        }
        public JObject GetPlaylistDetail(int playlistid, int offset = 0, bool total = true, int limit = 1000, int n = 1000)
        {
            JObject json = new JObject()
            {
                {"id", playlistid },
                {"offset", offset },
                {"total", total },
                {"limit", limit },
                {"n", n },
                {"csrf_token", "" }
            };
            string result = this.SendData("http://music.163.com/weapi/v3/playlist/detail?csrf_token=", json.ToString());
            return JObject.Parse(result);
        }
    }
}
