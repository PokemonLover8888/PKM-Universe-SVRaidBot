using PKHeX.Core;
using System;

namespace SysBot.Pokemon.Helpers;

/// <summary>
/// Helper class to manage raid indices across all regions
/// </summary>
public class RaidIndexHelper
{
    public const int PaldeaRaidCount = 69;
    public const int KitakamiRaidCount = 25;
    public const int BlueberryRaidCount = 23;
    public const int TeraRaidDetailSize = 0x20; // 32 bytes per raid entry

    // Header offsets by region
    private const int PaldeaHeaderSize = 0x20;
    private const int KitakamiHeaderSize = 0x10;
    private const int BlueberryHeaderSize = 0x10;

    /// <summary>
    /// Gets the global index range for a specific region
    /// </summary>
    public static (int Start, int End) GetGlobalIndexRange(TeraRaidMapParent region)
    {
        return region switch
        {
            TeraRaidMapParent.Paldea => (0, PaldeaRaidCount - 1),
            TeraRaidMapParent.Kitakami => (PaldeaRaidCount, PaldeaRaidCount + KitakamiRaidCount - 1),
            TeraRaidMapParent.Blueberry => (PaldeaRaidCount + KitakamiRaidCount,
                                          PaldeaRaidCount + KitakamiRaidCount + BlueberryRaidCount - 1),
            _ => throw new ArgumentException("Invalid region", nameof(region))
        };
    }

    /// <summary>
    /// Gets the region and local index from a global index
    /// </summary>
    public static (TeraRaidMapParent Region, int LocalIndex) GetRegionInfo(int globalIndex)
    {
        if (globalIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(globalIndex), "Index cannot be negative");

        if (globalIndex < PaldeaRaidCount)
            return (TeraRaidMapParent.Paldea, globalIndex);

        globalIndex -= PaldeaRaidCount;

        if (globalIndex < KitakamiRaidCount)
            return (TeraRaidMapParent.Kitakami, globalIndex);

        globalIndex -= KitakamiRaidCount;

        if (globalIndex < BlueberryRaidCount)
            return (TeraRaidMapParent.Blueberry, globalIndex);

        throw new ArgumentOutOfRangeException(nameof(globalIndex), "Index exceeds total raid count");
    }

    /// <summary>
    /// Gets the header size for a specific region
    /// </summary>
    public static int GetHeaderSize(TeraRaidMapParent region)
    {
        return region switch
        {
            TeraRaidMapParent.Paldea => PaldeaHeaderSize,
            TeraRaidMapParent.Kitakami => KitakamiHeaderSize,
            TeraRaidMapParent.Blueberry => BlueberryHeaderSize,
            _ => throw new ArgumentException("Invalid region", nameof(region))
        };
    }

    /// <summary>
    /// Gets the memory pointer for a specific region
    /// </summary>
    public static ulong GetRegionPointer(TeraRaidMapParent region, ulong paldeaPtr, ulong kitakamiPtr, ulong blueberryPtr)
    {
        return region switch
        {
            TeraRaidMapParent.Paldea => paldeaPtr,
            TeraRaidMapParent.Kitakami => kitakamiPtr,
            TeraRaidMapParent.Blueberry => blueberryPtr,
            _ => throw new ArgumentException("Invalid region", nameof(region))
        };
    }
}
