namespace Tools.Client;

static class Extensions
{
    public static uint64 Fnv1a(this Span<uint8> input, int32 length)
    {
        const uint64 offsetBasis = 0xCBF29CE484222325;
        const uint64 prime = 0x100000001B3;

        var hash = offsetBasis;

        for (var i = 0; i < length; i++)
            hash = prime * (input[i] ^ hash);

        return hash;
    }

    public static int32 Search(this Span<uint8> data, ReadOnlySpan<int16> pattern)
    {
        if (data.Length < pattern.Length)
            return -1;

        for (var i = 0; i <= data.Length - pattern.Length; i++)
        {
            var match = true;

            for (var j = 0; j < pattern.Length; j++)
            {
                if (pattern[j] != -1 && pattern[j] != data[i + j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return i;
        }

        return -1;
    }
}
