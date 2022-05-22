﻿using LICC;
using LogicAPI.Server.Components;
using LogicWorld.Server.Circuitry;
using System;
using System.Text;
using CheeseUtilMod.Server;
using CheeseUtilMod.Shared.CustomData;

using System.Timers;
using System.IO;
using System.IO.Compression;

namespace CheeseUtilMod.Components
{
    public class RamResizable : LogicComponent<IRamResizableData>
    {
        public override bool HasPersistentValues
        {
            get
            {
                return true;
            }
        }

        private static int PEG_CS = 0;
        private static int PEG_W = 1;
        private static int PEG_L = 2;
        private static int bitWidth;
        private static int addressWidth;
        private bool loadfromsave;
        private byte[] memory;
        private static int widthToBytes(int width)
        {
            int bas = width / 8;
            int mod = (width % 8) > 0 ? 1 : 0;
            return bas+mod;
        }
        protected override void Initialize()
        {
            bitWidth = Outputs.Count;
            addressWidth = (Inputs.Count - 3) - Outputs.Count;
            Data.bitWidth = 1;
            Data.addressWidth = 1;
            loadfromsave = true;
            memory = new byte[(1 << addressWidth) * widthToBytes(bitWidth)];
        }
        public override void Dispose()
        {
        }

        private ulong getPegShifted(int peg, int shift)
        {
            ulong bas = base.Inputs[peg].On ? 1ul : 0ul;
            return bas << shift;
        }
        protected override void DoLogicUpdate()
        {
            var newBitWidth = Outputs.Count;
            var newAddressWidth = (Inputs.Count - 3) - Outputs.Count;
            if (newBitWidth != bitWidth || newAddressWidth != addressWidth)
            {
                bitWidth = newBitWidth;
                addressWidth = newAddressWidth;
                memory = new byte[(1 << addressWidth) * widthToBytes(bitWidth)];
            }
            ulong bytes = (ulong)widthToBytes(bitWidth);
            ulong address = 0;
            for (int i = 0; i < addressWidth; i++)
            {
                address |= getPegShifted(i + 3 + bitWidth, i);
            }
            address *= (ulong)bytes;
            if (base.Inputs[PEG_W].On)
            {
                ulong data = 0;
                for (int i = 0; i < bitWidth; i++)
                {
                    data |= getPegShifted(i + 3, i);
                }
                for (ulong i = 0; i < bytes; i++)
                {
                    memory[address + i] = (byte)(data & 0xff);
                    data >>= 8;
                }
            }
            if (base.Inputs[PEG_CS].On)
            {
                //int data = memory[address];
                ulong data = 0;
                for (ulong i = 0; i < bytes; i++)
                {
                    var i2 = (bytes-1) - i;
                    data <<= 8;
                    data |= memory[address + i2];
                }
                for (int i = 0; i < bitWidth; i++)
                {
                    base.Outputs[i].On = (data & 1) == 1;
                    data >>= 1;
                }
            }
            else
            {
                for (int i = 0; i < bitWidth; i++)
                {
                    base.Outputs[i].On = false;
                }
            }
        }
        protected override void OnCustomDataUpdated()
        {

            if ((loadfromsave && Data.Data != null || Data.state == 1 && Data.ClientIncomingData != null))
            {
                var to_load_from = Data.Data;
                if (Data.state == 1)
                {
                    Logger.Info("Loading data from client");
                    to_load_from = Data.ClientIncomingData;
                }
                MemoryStream stream = new MemoryStream(to_load_from);
                stream.Position = 0;
                try
                {
                    DeflateStream decompressor = new DeflateStream(stream, CompressionMode.Decompress);
                    decompressor.Read(memory, 0, memory.Length);
                }
                catch
                {
                }
                loadfromsave = false;
                if (Data.state == 1)
                {
                    Data.state = 0;
                    Data.ClientIncomingData = new byte[0];
                }
                QueueLogicUpdate();
            }
         }
        protected override void SetDataDefaultValues()
        {
            Data.Data = new byte[0];
            Data.state = 0;
            Data.ClientIncomingData = new byte[0];
            Data.bitWidth = 1;
            Data.addressWidth = 1;
        }
        protected override void SavePersistentValuesToCustomData()
        {

            MemoryStream memstream = new MemoryStream();
            memstream.Position = 0;
            DeflateStream compressor = new DeflateStream(memstream, CompressionLevel.Optimal, true);
            compressor.Write(memory, 0, memory.Length);
            compressor.Flush();
            int length = (int)memstream.Position;
            memstream.Position = 0;
            byte[] bytes = new byte[length];
            memstream.Read(bytes, 0, length);
            Data.Data = bytes;
        }
    }
}
