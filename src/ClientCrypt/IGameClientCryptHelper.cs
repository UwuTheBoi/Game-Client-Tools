// Copyright (c) 2023 UwuTheBoi.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Tools.Client.ClientCrypt;

interface IGameClientCryptHelper
{
    ReadOnlySpan<uint8> GetCryptKey(Span<uint8> binary, StringBuilder offsetLogger, int32 offset = 0);
    (int32 FileOffset, int32 MemoryOffset) GetCryptStartOffset(Span<uint8> binary, StringBuilder offsetLogger);
    bool Validate(Span<uint8> binary);
}
