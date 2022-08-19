﻿using PathsSynchronizer.Core.Checksum;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PathsSynchronizer.Core.Hashing
{
    public class FileHashProvider<THash> where THash : notnull
    {
        private readonly IHashProvider<THash> _hashProvider;
        private readonly FileChecksumMode _mode;

        public FileHashProvider(IHashProvider<THash> hashProvider, FileChecksumMode mode)
        {
            _hashProvider = hashProvider;
            _mode = mode;
        }

        public async ValueTask<THash> HashFileAsync(string filePath)
        {
            THash hash =
                await (_mode switch
                {
                    FileChecksumMode.FileName => HashFileByPathAsync(filePath),
                    FileChecksumMode.FileHash => HashFileByBytesAsync(filePath),
                    _ => throw new NotImplementedException()
                })
                .ConfigureAwait(false);

            return hash;
        }

        private ValueTask<THash> HashFileByBytesAsync(string filePath) =>
            _hashProvider.HashFileAsync(filePath);

        private async ValueTask<THash> HashFileByPathAsync(string filePath)
        {
            byte[] allFile = Encoding.UTF8.GetBytes(filePath);
            return await _hashProvider.HashBytesAsync(allFile).ConfigureAwait(false);
        }
    }
}
