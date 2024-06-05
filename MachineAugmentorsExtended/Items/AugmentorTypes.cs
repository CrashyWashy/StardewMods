using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace MachineAugmentorsExtended.Items
{

    [XmlRoot(ElementName = "AugmentorType", Namespace = "")]
    public enum AugmentorType
    {
        [XmlEnum("Speed")]
        Speed = 0,
        [XmlEnum("Efficiency")]
        Efficiency = 1,
        [XmlEnum("None")]
        None = -1,

    }

    //TODO - array of delegates with
    public class AugmentorTypes
    {
        
    }

    [XmlRoot("SpeedAugmentor", Namespace = "")]
    public class SpeedAugmentor() : Augmentor(AugmentorType.Speed)
    {
        public override Augmentor CreateOne()
        {
            return new SpeedAugmentor();
        }
    }
    [XmlRoot("EfficiencyAugmentor", Namespace = "")]
    public class EfficiencyAugmentor() : Augmentor(AugmentorType.Efficiency)
    {
        public override Augmentor CreateOne()
            {
                return new EfficiencyAugmentor();
            }
    }
}

