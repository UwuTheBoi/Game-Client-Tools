namespace Tools.Client.Wow.Windows;

static class Patterns
{
    public static int16[][] StartOffsetPatterns =
    {
        new int16[] { 0x00, 0x48, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48, 0x8D },
        new int16[] { 0xFF, 0xFF, 0x48, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48, 0x8D },
        new int16[] { 0x00, 0x4C, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48 },
        new int16[] { 0xFF, 0xFF, 0x4C, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48 }
    };

    public static int16[][] CryptKeyPatterns =
    {
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA },
        new int16[] { 0x4C, 0x8D, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0xF3, 0xAA }
    };

    public static int16[] AdlerValidationPattern = { 0xB9, 0xF1, 0xD8, 0x27, 0x98 };
}
