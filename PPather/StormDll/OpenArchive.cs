using System;

namespace StormDll;

[Flags]
internal enum OpenArchive : uint
{
    BASE_PROVIDER_FILE = 0x00000000,  // Base data source is a file
    BASE_PROVIDER_MAP = 0x00000001,  // Base data source is memory-mapped file
    BASE_PROVIDER_HTTP = 0x00000002,  // Base data source is a file on web server
    BASE_PROVIDER_MASK = 0x0000000F,  // Mask for base provider value
    STREAM_PROVIDER_FLAT = 0x00000000,  // Stream is linear with no offset mapping
    STREAM_PROVIDER_PARTIAL = 0x00000010,  // Stream is partial file (.part)
    STREAM_PROVIDER_MPQE = 0x00000020,  // Stream is an encrypted MPQ
    STREAM_PROVIDER_BLOCK4 = 0x00000030,  // = 0x4000 per block, text MD5 after each block, max = 0x2000 blocks per file
    STREAM_PROVIDER_MASK = 0x000000F0,  // Mask for stream provider value
    STREAM_FLAG_READ_ONLY = 0x00000100,  // Stream is read only
    STREAM_FLAG_WRITE_SHARE = 0x00000200,  // Allow write sharing when open for write
    STREAM_FLAG_USE_BITMAP = 0x00000400,  // If the file has a file bitmap, load it and use it
    STREAM_OPTIONS_MASK = 0x0000FF00,  // Mask for stream options
    STREAM_PROVIDERS_MASK = 0x000000FF,  // Mask to get stream providers
    STREAM_FLAGS_MASK = 0x0000FFFF,  // Mask for all stream flags (providers+options)
    MPQ_OPEN_NO_LISTFILE = 0x00010000,  // Don't load the internal listfile
    MPQ_OPEN_NO_ATTRIBUTES = 0x00020000,  // Don't open the attributes
    MPQ_OPEN_NO_HEADER_SEARCH = 0x00040000,  // Don't search for the MPQ header past the begin of the file
    MPQ_OPEN_FORCE_MPQ_V1 = 0x00080000,  // Always open the archive as MPQ v 1.00, ignore the "wFormatVersion" variable in the header
    MPQ_OPEN_CHECK_SECTOR_CRC = 0x00100000,  // On files with MPQ_FILE_SECTOR_CRC, the CRC will be checked when reading file
    MPQ_OPEN_FORCE_LISTFILE = 0x00400000,  // Force add listfile even if there is none at the moment of opening
    MPQ_OPEN_READ_ONLY = STREAM_FLAG_READ_ONLY
};