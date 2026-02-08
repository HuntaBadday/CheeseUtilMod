using CheeseUtilMod.Shared.CustomData;
using LogicWorld.Rendering.Dynamics;
using LogicWorld.SharedCode.Components;
using UnityEngine;
using LogicAPI.Data;

namespace CheeseUtilMod.Client
{
    public class DualPortRamResizablePlacingRulesGenerator : DynamicPlacingRulesGenerator<(int InputCount, int OutputCount)>
    {
        protected override (int InputCount, int OutputCount) GetIdentifierFor(ComponentData componentData)
            => (componentData.InputCount, componentData.OutputCount);

        protected override PlacingRules GeneratePlacingRulesFor((int InputCount, int OutputCount) identifier)
        {
            var dataSize = identifier.OutputCount / 2;
            var addressSize = (identifier.InputCount - Pegs.DualPort.ControlPegs - identifier.OutputCount) / 2;
            float current_width = dataSize;
            if (addressSize > dataSize)
            {
                current_width = addressSize;
            }
            return new PlacingRules
            {
                AllowFineRotation = false,
                GridPlacingDimensions = new Vector2Int((int)current_width, 2),
            };
        }
    }
}
