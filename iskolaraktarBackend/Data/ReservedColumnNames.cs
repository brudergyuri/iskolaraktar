namespace iskolaraktarBackend.Data;

/// <summary>Minden dinamikus táblában fixen létrejövő oszlopnevek, ezek a kliens által nem foglalhatók/módosíthatók.</summary>
public static class ReservedColumnNames
{
    /// <summary>Autó-inkrement elsődleges kulcs.</summary>
    public const string Id = "Id";
    /// <summary>Az eszköz egyedi, ember által is olvasható azonosítója.</summary>
    public const string AssetCode = "AssetCode";
    /// <summary>A QR-kódban tárolt GUID, amit a szerver generál beszúráskor és a szkennelés ez alapján azonosítja az eszközt.</summary>
    public const string QrGuid = "QrGuid";
    /// <summary>A legutóbbi leltározás (QR-szkennelés) időpontja, amit a /scan végpont frissít.</summary>
    public const string LastInventoryDate = "LastInventoryDate";

    /// <summary>Ezeket a neveket a kliens nem foglalhatja le/módosíthatja saját oszlopként, mert a rendszer maga hozza létre/kezeli őket.</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Id, AssetCode, QrGuid, LastInventoryDate };
}
