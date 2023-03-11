// Copyright (c) 2023 UwuTheBoi.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tools.Client.Diablo;

sealed class DiabloPatterns : IPatterns
{
    // Same as WowPatterns
    public int16[][] StartOffsets { get; } =
    {
        new int16[] { 0x00, 0x48, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48, 0x8D },
        new int16[] { 0xFF, 0xFF, 0x48, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48, 0x8D },
        new int16[] { 0x00, 0x4C, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48 },
        new int16[] { 0xFF, 0xFF, 0x4C, 0x8D, -1, -1, -1, 0x0A, 0x00, 0x48 }
    };

    // Same as WowPatterns
    public int16[][] CryptKeys { get; } =
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

    // Sha256 constant.
    public int16[] Validation { get; } = { 0x81, -1, 0x98, 0x2F, 0x8A, 0x42 };
}
