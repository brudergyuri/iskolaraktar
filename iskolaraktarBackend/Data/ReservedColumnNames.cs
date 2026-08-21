namespace iskolaraktarBackend.Data;

/// <summary>Minden dinamikus táblában fixen létrejövő oszlopnevek, ezek a kliens által nem foglalhatók/módosíthatók.</summary>
public static class ReservedColumnNames
{
    public const string Id = "Id";
    public const string AssetCode = "AssetCode";
    public const string QrGuid = "QrGuid";
    public const string LastInventoryDate = "LastInventoryDate";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Id, AssetCode, QrGuid, LastInventoryDate };
}
