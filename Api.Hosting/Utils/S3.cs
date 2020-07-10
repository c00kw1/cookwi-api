using Api.Hosting.Settings;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Hosting.Utils
{
    public class S3
    {
        private S3Settings _settings;
        private MinioClient _minio;

        public S3(S3Settings settings)
        {
            _settings = settings;
            _minio = new MinioClient(_settings.Endpoint, _settings.AccessKey, _settings.AccessSecret);
            if (_settings.Ssl)
            {
                _minio = _minio.WithSSL();
            }
        }

        public async Task<List<Bucket>> ListBuckets()
        {
            var res = await _minio.ListBucketsAsync();
            return res.Buckets;
        }

        public async Task AddFile(string name, IFormFile file)
        {
            using (var s = file.OpenReadStream())
            {
                await _minio.PutObjectAsync(_settings.Bucket, name, s, file.Length, file.ContentType);
            }
        }

        public async Task<string> GetPresignedUrl(string name, int expireInSec = 15)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return await _minio.PresignedGetObjectAsync(_settings.Bucket, name, expireInSec);
        }
    }
}
