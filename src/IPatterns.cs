// Copyright (c) 2023 UwuTheBoi.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tools.Client;

interface IPatterns
{
    public int16[][] StartOffsets { get; }
    public int16[][] CryptKeys { get; }
    public int16[] Validation { get; }
}
