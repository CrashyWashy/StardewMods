using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineAugmentorsExtended
{
    public class ModConfig
    {
        public bool EnablePriceVariation { get; set; } = true;
        public float PriceVariation { get; set; } = 0.4f;
        public int BasePrice { get; set; } = 1200;
        public bool EnableSpeedAugmentor { get; set; } = true;
        public bool EnableOutputAugmentor { get; set; } = true;
    }
}
