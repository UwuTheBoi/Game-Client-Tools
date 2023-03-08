using System.Text;

namespace Tools.Client.Wow;

interface IGameClientCryptHelper
{
    ReadOnlySpan<uint8> GetCryptKey(Span<uint8> binary, StringBuilder offsetLogger, int32 offset = 0);
    (int32 FileOffset, int32 MemoryOffset) GetCryptStartOffset(Span<uint8> binary, StringBuilder offsetLogger);
}
