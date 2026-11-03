namespace GrayBoxMansion
{
    /// <summary>
    /// Things the player can carry. The hotbar, pickups, and locks share these ids.
    /// </summary>
    public enum ItemId
    {
        None = 0,
        Flashlight = 1,
        StudyKey = 2,
        MasterKey = 3,
        SilverLocket = 4,
        MusicBox = 5,
        CrackedPortrait = 6,
        Candelabra = 7
    }

    public static class ItemNames
    {
        public static string Label(ItemId id)
        {
            switch (id)
            {
                case ItemId.Flashlight: return "Flashlight";
                case ItemId.StudyKey: return "Study Key";
                case ItemId.MasterKey: return "Bedroom Key";
                case ItemId.SilverLocket: return "Locket";
                case ItemId.MusicBox: return "Music Box";
                case ItemId.CrackedPortrait: return "Portrait";
                case ItemId.Candelabra: return "Candelabra";
                default: return "";
            }
        }

        public static bool IsRelic(ItemId id)
        {
            return id == ItemId.SilverLocket
                || id == ItemId.MusicBox
                || id == ItemId.CrackedPortrait
                || id == ItemId.Candelabra;
        }
    }
}
