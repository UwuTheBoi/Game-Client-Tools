// Copyright (c) 2023 UwuTheBoi.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using Iced.Intel;

namespace Tools.Client.ClientCrypt;

sealed class WindowsGameClientCryptHelper : IGameClientCryptHelper
{
    readonly IPatterns _patterns;

    PEHeaders _peHeaders;

    public WindowsGameClientCryptHelper(IPatterns patterns)
    {
        _patterns = patterns;
    }

    public ReadOnlySpan<uint8> GetCryptKey(Span<uint8> binary, StringBuilder offsetLogger, int32 offset = 0)
    {
        _peHeaders ??= new PEHeaders(new MemoryStream(binary.ToArray()));

        if (offset != 0)
        {
            var rdata = _peHeaders.SectionHeaders.Single(s => s.Name == ".rdata");
            var rdataOffsetDiff = rdata.VirtualAddress - rdata.PointerToRawData;

            return binary[offset..(_peHeaders.PEHeader.ImportTableDirectory.RelativeVirtualAddress - rdataOffsetDiff + _peHeaders.PEHeader.BaseOfCode)];
        }

        foreach (var p in _patterns.CryptKeys)
        {
            var cryptKeyPatternResult = binary.Search(p);

            if (cryptKeyPatternResult == -1)
                continue;

            var instructions = binary[cryptKeyPatternResult..(cryptKeyPatternResult + 7)].ToArray();
            var decoded = Iced.Intel.Decoder.Create(64, instructions, DecoderOptions.NoInvalidCheck).First();
            var text = _peHeaders.SectionHeaders.Single(s => s.Name == ".text");
            var textOffsetDiff = text.VirtualAddress - text.PointerToRawData;
            var rdata = _peHeaders.SectionHeaders.Single(s => s.Name == ".rdata");
            var rdataOffsetDiff = rdata.VirtualAddress - rdata.PointerToRawData;
            var decodedValue = (int32)(cryptKeyPatternResult + (decoded.MemoryDisplacement32 - rdataOffsetDiff + textOffsetDiff));

            Console.WriteLine($"0x{decodedValue:X}");

            offsetLogger?.AppendLine($"key:{decodedValue}");

            return binary[decodedValue..(_peHeaders.PEHeader.ImportTableDirectory.RelativeVirtualAddress - rdataOffsetDiff + _peHeaders.PEHeader.BaseOfCode)];
        }

        return default;
    }

    public (int32 FileOffset, int32 MemoryOffset) GetCryptStartOffset(Span<uint8> binary, StringBuilder offsetLogger)
    {
        _peHeaders ??= new PEHeaders(new MemoryStream(binary.ToArray()));

        var text = _peHeaders.SectionHeaders.Single(s => s.Name == ".text");
        var textOffsetDiff = text.VirtualAddress - text.PointerToRawData;
        var foundResults = new Dictionary<int, int>();

        foreach (var p in _patterns.StartOffsets)
        {
            var startOffsetPatternResult = binary.Search(p);

            if (startOffsetPatternResult == -1)
                continue;

            if (p[0] == 0xFF)
                startOffsetPatternResult += 1;

            var bstart = binary[(startOffsetPatternResult + 4)..];
            var memoryPageOffset = (startOffsetPatternResult + 1 + textOffsetDiff) + Unsafe.ReadUnaligned<int32>(ref bstart[0]) + 7;

            foundResults[memoryPageOffset] = startOffsetPatternResult;
        }

        if (foundResults.Count > 0)
        {
            var memoryPageOffset = foundResults.OrderByDescending(x => x.Key - x.Value).First().Key;
            var aligned = ((memoryPageOffset + (0x1000 - 1)) & ~(0x1000 - 1));
            var filePageOffset = aligned - 0xC00;

            Console.WriteLine($"0x{aligned:X}");

            offsetLogger?.AppendLine($"fPage:{filePageOffset}");
            offsetLogger?.AppendLine($"mPage:{aligned}");

            return (filePageOffset, aligned);
        }


        return (-1, -1);
    }

    public bool Validate(Span<uint8> binary) => binary.Search(_patterns.Validation) != -1;
}
