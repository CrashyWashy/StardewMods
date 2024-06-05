using System;

using System.Reflection;

using StardewValley;

namespace MachineAugmentorsExtended.ThirdParty
{
    public interface ISpaceCore
    {  
        /// <returns>An array of skill IDs</returns>
        string[] GetCustomSkills();

        /// <summary>
        /// Gets the Base level of the custom skill for the farmer.
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetLevelForCustomSkill(Farmer farmer, string skill);

        /// Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        void RegisterSerializerType(Type type);

        void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter);

        List<int> GetLocalIndexForMethod(MethodBase meth, string local);

        public event EventHandler<Action<string, Action>> AdvancedInteractionStarted;
    }
}