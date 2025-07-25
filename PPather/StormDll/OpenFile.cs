namespace StormDll;

internal enum OpenFile : uint
{
    SFILE_OPEN_FROM_MPQ = 0x00000000,       // Open the file from the MPQ archive
    SFILE_OPEN_CHECK_EXISTS = 0xFFFFFFFC,   // Only check whether the file exists
    SFILE_OPEN_LOCAL_FILE = 0xFFFFFFFF      // Open a local file
};