using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using Tools.Client.Wow.Windows;

namespace Tools.Client.Wow;

class GameClientCryptService
{
    readonly CommandLineOptions _commandLineOptions;
    readonly IGameClientCryptHelper _gameClientCryptHelper;
    readonly GameClientCrypt _gameClientCrypt;

    string _inputFile;
    string _outputFile;
    bool _encryptMode;
    uint8[] _binary;

    StringBuilder _outOffsets;

    // Windows specific for now.
    SectionHeader _peHeader;

    const int32 pageSize = 0x1000;

    public GameClientCryptService(CommandLineOptions commandLineOptions, IGameClientCryptHelper gameClientCryptHelper, GameClientCrypt gameClientCrypt)
    {
        _commandLineOptions = commandLineOptions;
        _gameClientCryptHelper = gameClientCryptHelper;
        _gameClientCrypt = gameClientCrypt;
    }

    public void Run(string[] args)
    {
        _commandLineOptions.RootCommand.SetHandler(context =>
        {
            PrepareCommandLine(context.ParseResult);
        });

        _commandLineOptions.Instance.Invoke(args);

        var fInfo = FileVersionInfo.GetVersionInfo(_inputFile);

        _outOffsets.AppendLine($"Build {fInfo.FileVersion}");
        _outOffsets.AppendLine();

        // First get the required variables for decryption.

        Console.Write($"Getting crypt offsets... ");
        var cryptStartOffsets = _gameClientCryptHelper.GetCryptStartOffset(_binary, _outOffsets);

        Console.Write($"Getting crypt key... ");
        var cryptKey = _gameClientCryptHelper.GetCryptKey(_binary, _outOffsets);

        if (_encryptMode)
        {
            var lines = File.ReadAllLines(fInfo.FileVersion + ".txt");
            var key = 0;
            var fPage = 0;
            var mPage = 0;
            var const1 = 0;
            var const2 = 0;

            // Skip header lines
            foreach (var l in lines.Skip(2))
            {
                if (!string.IsNullOrEmpty(l))
                {
                    var line = l.Split(':', StringSplitOptions.TrimEntries);

                    switch (line[0])
                    {
                        case "key":
                            key = int32.Parse(line[1]);
                            break;
                        case "fPage":
                            fPage = int32.Parse(line[1]);
                            break;
                        case "mPage":
                            mPage = int32.Parse(line[1]);
                            break;
                        case "const1":
                            const1 = int32.Parse(line[1]);
                            break;
                        case "const2":
                            const2 = int32.Parse(line[1]);
                            break;
                    }
                }
            }

            Console.WriteLine("Encrypting...");

            Encrypt(_binary, cryptKey, _peHeader.SizeOfRawData, const1, const2, (fPage, mPage));

            Console.WriteLine("Saving...");

            File.WriteAllBytes(_outputFile, _binary.ToArray());

            Console.WriteLine($"Done!");
            Console.WriteLine($"Output saved at '{_outputFile}'");
        }
        else
        {
            if (Decrypt(cryptKey, cryptStartOffsets))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Done!");

                File.WriteAllTextAsync(fInfo.FileVersion + ".txt", _outOffsets.ToString());
                File.WriteAllBytes(_outputFile, _binary.ToArray());

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Output saved at '{_outputFile}'");
            }
        }
    }

    void PrepareCommandLine(ParseResult parseResult)
    {
        _inputFile = parseResult.GetValueForOption(_commandLineOptions.GameInput);

        if (File.Exists(_inputFile))
        {
            _binary = File.ReadAllBytes(_inputFile);
            _outOffsets = new();

            // Windows specific for now.
            _peHeader = new PEHeaders(new MemoryStream(_binary)).SectionHeaders.Single(s => s.Name.ToLower().Contains(".text"));
        }

        _outputFile = parseResult.GetValueForOption(_commandLineOptions.GameOutput);
        _encryptMode = parseResult.GetValueForOption(_commandLineOptions.GameEncryptMode);
    }

    void Encrypt(Span<uint8> binary, ReadOnlySpan<uint8> cryptKey, int32 size, int32 const1, int32 const2, (int32 FileOffset, int32 MemoryOffset) startOffset)
    {
        while (startOffset.FileOffset < size && (startOffset.FileOffset != -1 && startOffset.MemoryOffset != -1))
        {
            _gameClientCrypt.EncryptPage(binary, cryptKey, startOffset, const1, const2);

            startOffset.FileOffset += pageSize;
            startOffset.MemoryOffset += pageSize;
        }
    }

    bool Decrypt(ReadOnlySpan<uint8> cryptKey, (int32 FileOffset, int32 MemoryOffset) startOffset)
    {
        var bin = new uint8[_binary.Length];

        Unsafe.CopyBlockUnaligned(ref bin[0], ref _binary[0], (uint32)_binary.Length);

        // Windows specific for now.
        // Only decrypt the first 24 pages for the initial constant bruteforce.
        var textSize = pageSize * 24;

        if (textSize < startOffset.FileOffset)
            textSize += startOffset.FileOffset;

        Console.Write("Getting crypt constants... ");

        for (var i = 0x1FF; i >= 0x100; i--)
        {
            for (var j = 0xFF; j >= 0x10; j--)
            {
                var cryptOffsetBase = i * ((startOffset.MemoryOffset / pageSize) % j);

                if (cryptOffsetBase + i >= cryptKey.Length)
                    continue;

                if (TryFullDecrypt(bin, cryptKey, textSize, i, j, startOffset, true))
                {
                    Console.WriteLine("Decrypting...");

                    Unsafe.CopyBlockUnaligned(ref bin[0], ref _binary[0], (uint32)_binary.Length);

                    TryFullDecrypt(bin, cryptKey, _peHeader.SizeOfRawData, i, j, startOffset, false);

                    Console.WriteLine("Validating...");

                    if (bin.AsSpan().Search(Patterns.AdlerValidationPattern) == -1)
                    {
                        Unsafe.CopyBlockUnaligned(ref bin[0], ref _binary[0], (uint32)_binary.Length);
                        continue;
                    }

                    // We do not longer need the original data.
                    _binary = bin;

                    _outOffsets.AppendLine($"const1:{i}");
                    _outOffsets.AppendLine($"const2:{j}");
                    _outOffsets.AppendLine();

                    return true;
                }
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Decryption failed :(");
        Console.ForegroundColor = ConsoleColor.Gray;

        return false;
    }

    bool TryFullDecrypt(Span<uint8> binary, ReadOnlySpan<uint8> cryptKey, int32 size, int32 const1, int32 const2, (int32 FileOffset, int32 MemoryOffset) startOffset, bool bf)
    {
        _gameClientCrypt.ResetState();

        while (startOffset.FileOffset < size && (startOffset.FileOffset != -1 && startOffset.MemoryOffset != -1))
        {
            var cryptOffsetBase = const1 * ((startOffset.MemoryOffset / pageSize) % const2);

            if (cryptOffsetBase + const1 >= cryptKey.Length)
                return false;

            _gameClientCrypt.DecryptPage(binary, cryptKey, cryptOffsetBase, startOffset, const1);

            // Check if the found constants fulfill all necessary conditions.
            if (bf && Unsafe.ReadUnaligned<int64>(ref binary[startOffset.FileOffset]) == 0)
            {
                while (startOffset.FileOffset < _peHeader.SizeOfRawData)
                {
                    cryptOffsetBase = const1 * ((startOffset.MemoryOffset / pageSize) % const2);

                    if (cryptOffsetBase + const1 >= cryptKey.Length)
                    {
                        Unsafe.CopyBlockUnaligned(ref binary[startOffset.FileOffset], ref _binary[startOffset.FileOffset], pageSize);
                        return false;
                    }

                    startOffset.FileOffset += pageSize;
                    startOffset.MemoryOffset += pageSize;
                }

                Unsafe.CopyBlockUnaligned(ref binary[startOffset.FileOffset], ref _binary[startOffset.FileOffset], pageSize);

                return true;
            }

            // Copy the original data back for future constant finding attempts.
            if (bf)
                Unsafe.CopyBlockUnaligned(ref binary[startOffset.FileOffset], ref _binary[startOffset.FileOffset], pageSize);

            startOffset.FileOffset += pageSize;
            startOffset.MemoryOffset += pageSize;
        }

        return false;
    }
}
