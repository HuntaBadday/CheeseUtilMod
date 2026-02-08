using LogicWorld.Rendering.Dynamics;
using LogicWorld.SharedCode.Components;
using System.Collections.Generic;
using CheeseUtilMod.Shared.CustomData;
using UnityEngine;
using JimmysUnityUtilities;
using LogicAPI.Data;

namespace CheeseUtilMod.Client
{
    public class DualPortRamResizablePrefabGenerator : DynamicPrefabGenerator<(int InputCount, int OutputCount)>
    {
        private static Color24 blockColor = new Color24(127, 127, 127);

        protected override (int InputCount, int OutputCount) GetIdentifierFor(ComponentData componentData)
            => (componentData.InputCount, componentData.OutputCount);

        public override (int inputCount, int outputCount) GetDefaultPegCounts()
            => (Pegs.DualPort.ControlPegs + 2 * Pegs.DualPort.DefaultBitWidth + 2 * Pegs.DualPort.DefaultAddressWidth,
                2 * Pegs.DualPort.DefaultBitWidth);
        
        // Input pegs in order:
        // Control pegs (5)
        // Port A address (addressWidth)
        // Port B address (addressWidth)
        // Port A input (bitWidth)
        // Port B input (bitWidth)
        
        // Output pegs in order:
        // Port A output (bitWidth)
        // Port B output (bitWidth)

        protected override Prefab GeneratePrefabFor((int InputCount, int OutputCount) identifier)
        {
            var dataSize = identifier.OutputCount / 2;
            var addressSize = (identifier.InputCount - Pegs.DualPort.ControlPegs - identifier.OutputCount) / 2;
            var prefabBlock = new Block
            {
                RawColor = blockColor
            };
            float current_width = dataSize;
            if (addressSize > dataSize)
            {
                current_width = addressSize;
            }

            //Generate the chip select and write lines
            List<ComponentInput> inputs = new List<ComponentInput>();
            //Load
            inputs.Add(new ComponentInput
            {
                Position = new Vector3(1f, 5f, 0.5f),
                Rotation = new Vector3(0f, 0f, 0f),
                Length = 0.6f
            });
            //Output enable A
            inputs.Add(new ComponentInput
            {
                Position = new Vector3(-1f, 5f, 0f),
                Rotation = new Vector3(0f, 0f, 0f),
                Length = 1f,
            });
            //Write A
            inputs.Add(new ComponentInput
            {
                Position = new Vector3(0f, 5f, 0f),
                Rotation = new Vector3(0f, 0f, 0f),
                Length = 0.8f
            });
            //Output enable B
            inputs.Add(new ComponentInput
            {
                Position = new Vector3(-1f, 5f, 1f),
                Rotation = new Vector3(0f, 0f, 0f),
                Length = 1f,
            });
            //Write B
            inputs.Add(new ComponentInput
            {
                Position = new Vector3(0f, 5f, 1f),
                Rotation = new Vector3(0f, 0f, 0f),
                Length = 0.8f
            });

            //Port A address pins
            float baseInputX = -current_width / 2f + 1f;
            //Make the inputs different lengths to signal endianness
            //Start with the smallest length one being little endian
            float start_length = 0.2f;
            float end_length = 0.6f;
            float step_length = (end_length - start_length) / addressSize;
            float length = start_length;
            for (int i = 0; i < addressSize; i++)
            {
                inputs.Add(new ComponentInput
                {
                    Position = new Vector3(baseInputX, 4f, -0.5f),
                    Rotation = new Vector3(-90f, 0f, 0f),
                    Length = length
                });
                baseInputX += 1;
                length += step_length;
            }

            //Port B address pins
            baseInputX = -current_width / 2f + 1f;
            step_length = (end_length - start_length) / addressSize;
            length = start_length;
            for (int i = 0; i < addressSize; i++)
            {
                inputs.Add(new ComponentInput
                {
                    Position = new Vector3(baseInputX, 4f, 1.5f),
                    Rotation = new Vector3(90f, 0f, 0f),
                    Length = length
                });
                baseInputX += 1;
                length += step_length;
            }

            //Port A data input pins
            baseInputX = (-current_width / 2f) + 1f;
            step_length = (end_length - start_length) / dataSize;
            length = start_length;
            for (int i = 0; i < dataSize; i++)
            {
                inputs.Add(new ComponentInput
                {
                    Position = new Vector3(baseInputX, 2.5f, -0.5f),
                    Rotation = new Vector3(-90f, 0f, 0f),
                    Length = length
                });
                baseInputX += 1;
                length += step_length;
            }

            //Port B data input pins
            baseInputX = (-current_width / 2f) + 1f;
            step_length = (end_length - start_length) / dataSize;
            length = start_length;
            for (int i = 0; i < dataSize; i++)
            {
                inputs.Add(new ComponentInput
                {
                    Position = new Vector3(baseInputX, 2.5f, 1.5f),
                    Rotation = new Vector3(90f, 0f, 0f),
                    Length = length
                });
                baseInputX += 1;
                length += step_length;
            }

            List<ComponentOutput> outputs = new List<ComponentOutput>();
            float baseOutputX = -current_width / 2f + 1f;
            //Port A outputs
            for (int i = 0; i < dataSize; i++)
            {
                outputs.Add(new ComponentOutput
                {
                    Position = new Vector3(baseOutputX, 1f, -0.5f),
                    Rotation = new Vector3(-90f, 0f, 0f),
                });
                baseOutputX += 1;
            }

            baseOutputX = -current_width / 2f + 1f;
            //Port B outputs
            for (int i = 0; i < dataSize; i++)
            {
                outputs.Add(new ComponentOutput
                {
                    Position = new Vector3(baseOutputX, 1f, 1.5f),
                    Rotation = new Vector3(90f, 0f, 0f),
                });
                baseOutputX += 1;
            }

            prefabBlock.Scale = new Vector3(current_width, 5f, 2f);
            prefabBlock.Position = new Vector3(0.5f, 0f, 0.5f);
            return new Prefab
            {
                Blocks = new Block[] {prefabBlock},
                Outputs = outputs.ToArray(),
                Inputs = inputs.ToArray()
            };
        }
    }
}