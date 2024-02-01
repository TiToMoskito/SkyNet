namespace SkyNet
{
    /// <summary>
    /// Helper class that implements compression related extension methods for the bit packer.
    /// </summary>
    //public static class CompressorExtensions
    //{
    //    /// <summary>
    //    /// Writes a compressed integer value into the bit packer.
    //    /// </summary>
    //    public static void WriteInt(this NetBuffer bitPacker, int value, CompressorInt compressor)
    //    {
    //        int requiredBits = compressor.BitsRequired;
    //        bitPacker.Write(requiredBits, compressor.Compress(value));
    //        bitPacker.IncreaseOriginalSize(4);
    //    }

    //    /// <summary>
    //    /// Reads a compressed integer value from the bit packer.
    //    /// </summary>
    //    public static int ReadInt(this NetBuffer bitPacker, CompressorInt compressor)
    //    {
    //        int requiredBits = compressor.BitsRequired;
    //        return compressor.Decompress(bitPacker.Read(requiredBits));
    //    }

    //    /// <summary>
    //    /// Peeks at the next compressed integer value from the bit packer.
    //    /// </summary>
    //    public static int PeekInt(this NetBuffer bitPacker, CompressorInt compressor)
    //    {
    //        int requiredBits = compressor.BitsRequired;
    //        return compressor.Decompress(bitPacker.Peek(requiredBits));
    //    }

    //    /// <summary>
    //    /// Writes a compressed single-precision floating-point value into the bit packer.
    //    /// </summary>
    //    public static void WriteFloat(this NetBuffer bitPacker, float value, CompressorFloat compressor)
    //    {
    //        int requiredBits = compressor.BitsRequired;
    //        bitPacker.Write(requiredBits, compressor.Compress(value));
    //        bitPacker.IncreaseOriginalSize(4);
    //    }

    //    /// <summary>
    //    /// Reads a compressed single-precision floating-point value from the bit packer.
    //    /// </summary>
    //    public static float ReadFloat(this NetBuffer bitPacker, CompressorFloat compressor)
    //    {
    //        int requiredBits = compressor.BitsRequired;
    //        return compressor.Decompress(bitPacker.Read(requiredBits));
    //    }

    //    /// <summary>
    //    /// Peeks at the next compressed single-precision floating-point value from the bit packer.
    //    /// </summary>
    //    public static float PeekFloat(this NetBuffer bitPacker, CompressorFloat compressor)
    //    {
    //        int requiredBits = compressor.BitsRequired;
    //        return compressor.Decompress(bitPacker.Peek(requiredBits));
    //    }
    //}
}
