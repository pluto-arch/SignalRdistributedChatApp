using ChatService.Application.Models;
using Dncy.StackExchangeRedis;
using Microsoft.AspNetCore.RateLimiting;
using NuGet.Packaging.Signing;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;

namespace ChatService.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppController : ControllerBase, IResponseWraps
    {
        private readonly IRedisClient _redis;
        private readonly RSACryptoServiceProvider _rsaProvider;

        public AppController(RedisClientFactory redisFactory, RSACryptoServiceProvider rsaProvider)
        {
            _rsaProvider = rsaProvider;
            _redis = redisFactory["docker01"];
        }

        /// <summary>
        /// app初始化
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultDto> InitAsync()
        {
            var publicKey = await GetRasPublishKey();
            return await Task.FromResult(this.Success(new
            {
                sk = $"-----BEGIN RSA PUBLIC KEY-----\n{publicKey}\n-----END RSA PUBLIC KEY-----"
            }));
        }

        async Task<string> GetRasPublishKey()
        {
            var rsaInfo = await _redis.Db.HashGetAllAsync("RSA");
            if (rsaInfo.Any())
            {
                return rsaInfo.First(x => x.Name == "publishKey").Value;
            }

            var privateKey = _rsaProvider.ExportRSAPrivateKey();
            var publicKey = _rsaProvider.ExportRSAPublicKey();

            var publickKeyString=Convert.ToBase64String(publicKey);
            var privateKeyString=Convert.ToBase64String(privateKey);

            var data = new HashEntry[] {
                new HashEntry("publishKey",  publickKeyString),
                new HashEntry("privateKey", privateKeyString)
            };
            await _redis.Db.HashSetAsync("RSA", data);
            return publickKeyString;
        }


        /// <summary>
        /// app建立连接
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultDto> ConnAsync(Reqw request)
        {

            var rsaInfo = await _redis.Db.HashGetAllAsync("RSA");
            if (rsaInfo.Any())
            {
                _rsaProvider.ImportRSAPrivateKey(Convert.FromBase64String(rsaInfo.First(x => x.Name == "privateKey").Value), out _);
            }

            // 解密公钥假的数据
            byte[] symmetricKey = _rsaProvider.Decrypt(Convert.FromBase64String(request.EncryptedKey), true);
            var aesInfo = Encoding.UTF8.GetString(symmetricKey);

            return this.Success("ok");
        }


        public class Reqw
        {
            public string EncryptedKey{get;set;}

            public string EncryptedData{get;set;}
            public string Signature{get;set;}
        }

        ///// <summary>
        ///// app建立连接
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<ResultDto> ConnAsync(
        //    [FromForm(Name = "data")] string data,
        //    [FromForm(Name = "sign")] string sign)
        //{
        //    var rsaInfo = await _redis.Db.HashGetAllAsync("RSA");
        //    if (rsaInfo.Any())
        //    {
        //        _rsaProvider.ImportRSAPrivateKey(Convert.FromBase64String(rsaInfo.First(x => x.Name == "privateKey").Value), out _);
        //    }

        //    // 解密公钥假的数据
        //    byte[] symmetricKey = _rsaProvider.Decrypt(Convert.FromBase64String(data), true);
        //    var aesInfo=Encoding.UTF8.GetString(symmetricKey);
        //    var dataModel=JsonConvert.DeserializeObject<ClientConnData>(aesInfo);

        //    // 签名验证
        //    var signStr=$"{dataModel.TimeStamp}{dataModel.Nonce}{dataModel.EncryptData}";
        //    var newSign=Encoding.UTF8.GetString(MD5.HashData(Encoding.UTF8.GetBytes(signStr)));
        //    if (newSign!=sign)
        //    {
        //        return await Task.FromResult(this.Error("数据签名不正确"));
        //    }

        //    // 使用对称密钥解密数据
        //    string decryptedData = SymmetricEncryption.Decrypt(dataModel.EncryptData, Encoding.UTF8.GetBytes(dataModel.Key),Encoding.UTF8.GetBytes(dataModel.Iv));
        //    var clientInfo=JsonConvert.DeserializeObject<ClientInfo>(decryptedData);

        //    // 密钥存储
        //    await _redis.Db.HashSetAsync($"AES:{clientInfo.ClientLabel}", dataModel.Key, dataModel.Iv);
        //    return await Task.FromResult(this.Success("连接成功"));
        //}



        record ClientConnData()
        {
            public string TimeStamp { get; set; }
            public string Nonce { get; set; }
            public string Key { get; set; }
            public string Iv {get;set;}
            public string EncryptData { get; set; }
        }


        record ClientInfo()
        {
            public string ClientLabel { get; set; }
        }


        public class SymmetricEncryption
        {
            public static string Encrypt(string data, byte[] key, byte[] iv)
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(data);
                            }
                        }
                
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }

            public static string Decrypt(string encryptedText, byte[] key, byte[] iv)
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
