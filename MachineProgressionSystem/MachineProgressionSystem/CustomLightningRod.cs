using System.Diagnostics;

namespace MachineProgressionSystem;
internal static class CustomLightningRod
{
    public static int LightningRodId(string itemId)
    {
        return itemId switch
        {
            var id when id.Contains("GoldLightningRod") => 2,
            var id when id.Contains("DiamondLightningRod") => 3,
            var id when id.Contains("RadioactiveLightningRod") => 4,
            var id when id.Contains("IridiumLightningRod") => 5,
            var id when id.Contains("PrismaticLightningRod") => 6,
            _ => 0
        };
    }
}