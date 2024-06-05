using StardewValley.GameData.Objects;

namespace MachineAugmentorsExtended.Data
{
    public sealed class ModData
    {
        public ModData(List<AugmentorData> augmentorDataList)
        {
            AugmentorDataList = augmentorDataList;
        }

        public List<AugmentorData> AugmentorDataList { get; set; }
    }
    public class AugmentorData
    {
        public string Id { get; set; }
        public ObjectData Object { get; set; }
    }
}
