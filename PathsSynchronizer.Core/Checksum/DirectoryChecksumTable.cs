﻿using Newtonsoft.Json;
using PathsSynchronizer.Core.Support.Compression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace PathsSynchronizer.Core.Checksum
{
    public class DirectoryChecksumTable
    {
        private readonly IReadOnlyDictionary<string, ulong> _checksumTable;
        public string DirectoryPath { get; }
        public FileChecksumMode Mode { get; }

        public int Count => _checksumTable.Count;

        private DirectoryChecksumTable(DirectoryChecksumTableData data)
        {
            if (string.IsNullOrWhiteSpace(data.DirectoryPath))
            {
                throw new ArgumentNullException(nameof(data.DirectoryPath), "The provided path is null or blank");
            }

            if (!Directory.Exists(data.DirectoryPath))
            {
                throw new DirectoryNotFoundException($"The path {data.DirectoryPath} has not been found");
            }

            DirectoryPath = data.DirectoryPath;
            Mode = data.Mode;
            _checksumTable = data.ChecksumTable ?? new ReadOnlyDictionary<string, ulong>(new Dictionary<string, ulong>());
        }

        public DirectoryChecksumTable(string directoryPath, FileChecksumMode mode, IDictionary<string, ulong> table)
            : this(new DirectoryChecksumTableData(table, directoryPath, mode))
        {
        }

        public async Task<byte[]> SerializeAsync()
        {
            DirectoryChecksumTableData toSerializeObj =
                new()
                {
                    Mode = Mode,
                    ChecksumTable = _checksumTable,
                    DirectoryPath = DirectoryPath
                };

            string json = JsonConvert.SerializeObject(toSerializeObj);
            return await GZipHelper.CompressStringAsync(json).ConfigureAwait(false);
        }

        public static async Task<DirectoryChecksumTable> FromSerializedAsync(byte[] bytes)
        {
            string json = await GZipHelper.DecompressStringAsync(bytes).ConfigureAwait(false);
            DirectoryChecksumTableData data = JsonConvert.DeserializeObject<DirectoryChecksumTableData>(json);
            return new DirectoryChecksumTable(data);
        }
    }
}
