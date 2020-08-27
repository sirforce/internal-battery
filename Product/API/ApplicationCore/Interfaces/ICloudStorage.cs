﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface ICloudStorage
    {
        Task<string> UploadFileAsync(string path, string fileName, Stream fileStream);
        Task<Stream> GetStreamAsync(string blobName);
        Task<Stream> OpenReadAsync(string blobName);
        Task DownloadToStreamAsync(string blobName, Stream stream);
        Task<bool> DeleteFileAsync(string blobName);
        Task<string> UploadBlobAsync(string blobName, byte[] blobArray);

        Task<string> GetSubscriberVideoSAS(Guid subscriberGuid);
    }
}
