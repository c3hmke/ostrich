using Emulation;

namespace GameBoy;

public class GBVideoSource : IVideoSource
{
    public int Width  { get; } = 144;
    public int Height { get; } = 160;
}