using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace com.b_velop.Slipways.API.Infrastructure
{
    public class SecretProvider
    {
        public string GetSecret(
            string key)
        {
            const string DOCKER_SECRET_PATH = "/run/secrets/";
            if (Directory.Exists(DOCKER_SECRET_PATH))
            {
                using var provider = new PhysicalFileProvider(DOCKER_SECRET_PATH);
                var fileInfo = provider.GetFileInfo(key);
                if (fileInfo.Exists)
                {
                    using var stream = fileInfo.CreateReadStream();
                    using var streamReader = new StreamReader(stream);
                    return streamReader.ReadToEnd();
                }
            }
            return Environment.GetEnvironmentVariable(key);
        }
    }
}
