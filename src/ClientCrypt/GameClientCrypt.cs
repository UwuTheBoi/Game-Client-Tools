// Copyright (c) 2023 UwuTheBoi.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tools.Client.ClientCrypt;

sealed class GameClientCrypt
{
    readonly bool _encryptMode;

    uint64 _pageHash;

    const int32 pageSize = 0x1000;

    public GameClientCrypt(bool encryptMode)
    {
        _encryptMode = encryptMode;
    }

    public bool DecryptPage(Span<uint8> binary, ReadOnlySpan<uint8> cryptKey, int32 cryptOffsetBase, (int32 FileOffset, int32 MemoryOffset) startOffset, int32 const1)
    {
        if (_encryptMode)
            throw new InvalidOperationException("Cannot decrypt when encryption mode is enabled.");

        var keyState = InitializeKeyState(cryptKey, const1, cryptOffsetBase);

        Process(binary, startOffset, keyState);

        _pageHash = binary[startOffset.FileOffset..].Fnv1a(pageSize);

        return false;
    }

    public void EncryptPage(Span<uint8> binary, ReadOnlySpan<uint8> cryptKey, (int32 FileOffset, int32 MemoryOffset) startOffset, int32 const1, int32 const2)
    {
        if (!_encryptMode)
            throw new InvalidOperationException("Cannot encrypt when encryption mode is disable.");

        var cryptOffsetBase = const1 * ((startOffset.MemoryOffset / pageSize) % const2);
        var keyState = InitializeKeyState(cryptKey, const1, cryptOffsetBase);

        _pageHash = binary[startOffset.FileOffset..].Fnv1a(pageSize);

        Process(binary, startOffset, keyState);
    }

    public void ResetState() => _pageHash = 0;

    byte[] InitializeKeyState(ReadOnlySpan<uint8> cryptKey, int32 const1, int32 cryptOffsetBase)
    {
        var keyState = new uint8[const1 + 0x100];

        for (uint32 i = 0; i < const1; i++)
            keyState[i + 0x100] = (uint8)(cryptKey[(int32)(cryptOffsetBase + i)] ^ BitConverter.GetBytes(_pageHash)[i & 7]);

        for (uint32 i = 0; i < 0x100; ++i)
            keyState[i] = (uint8)i;

        byte prevKeyStateOffset = 0;

        for (uint32 j = 0; j < 0x100; ++j)
        {
            var currKeyState = keyState[j];

            prevKeyStateOffset += (uint8)(keyState[j % const1 + 0x100] + currKeyState);

            ref var prevKeyState = ref keyState[prevKeyStateOffset];

            keyState[j] = prevKeyState;
            prevKeyState = currKeyState;
        }

        return keyState;
    }

    void Process(Span<uint8> binary, (int32 FileOffset, int32 MemoryOffset) startOffset, uint8[] keyState)
    {
        uint8 prevKeyStateOffset = 0;

        for (var i = 0; i < 0x1000; i++)
        {
            ref uint8 currKeyState = ref keyState[(i + 1) % 0x100];

            // Decrypt the current selected byte.
            binary[i + startOffset.FileOffset] ^= currKeyState;

            ref uint8 prevKeyState = ref keyState[prevKeyStateOffset += currKeyState];

            // Swap key states.
            (prevKeyState, currKeyState) = (currKeyState, prevKeyState);
        }
    }
}
