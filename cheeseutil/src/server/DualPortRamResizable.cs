using LogicWorld.Server.Circuitry;
using System;
using CheeseUtilMod.Shared.CustomData;
using System.IO;
using System.IO.Compression;

namespace CheeseUtilMod.Components
{
    public class DualPortRamResizable : LogicComponent<IRamResizableData>
    {
        public override bool HasPersistentValues => true;
        
        private int bitWidth;
        private int addressWidth;
        private bool loadfromsave;
        private byte[] memory;
        private bool isdatadirty;

        private static int widthToBytes(int width)
        {
            int bas = width / 8;
            int mod = width % 8 > 0 ? 1 : 0;
            return bas + mod;
        }

        protected override void Initialize()
        {
            bitWidth = Outputs.Count / 2;
            addressWidth = (Inputs.Count - Pegs.DualPort.ControlPegs - Outputs.Count) / 2;
            Data.BitWidth = Pegs.DualPort.DefaultBitWidth;
            Data.AddressWidth = Pegs.DualPort.DefaultAddressWidth;
            loadfromsave = true;
            memory = new byte[(1 << addressWidth) * widthToBytes(bitWidth)];
            isdatadirty = false;
        }

        public override void Dispose() { }

        private ulong getPegShifted(int peg, int shift)
        {
            ulong bas = Inputs[peg].On ? 1ul : 0ul;
            return bas << shift;
        }

        protected override void DoLogicUpdate()
        {
            var newBitWidth = Outputs.Count / 2;
            var newAddressWidth = (Inputs.Count - Pegs.DualPort.ControlPegs - Outputs.Count) / 2;
            if (newBitWidth != bitWidth || newAddressWidth != addressWidth)
            {
                bitWidth = newBitWidth;
                addressWidth = newAddressWidth;
                memory = new byte[(1 << addressWidth) * widthToBytes(bitWidth)];
            }

            ulong bytes = (ulong) widthToBytes(bitWidth);
            ulong addressPortA = 0;
            for (int i = 0; i < addressWidth; i++)
            {
                addressPortA |= getPegShifted(Pegs.DualPort.ControlPegs + i, i);
            }

            ulong addressPortB = 0;
            for (int i = 0; i < addressWidth; i++)
            {
                addressPortB |= getPegShifted(Pegs.DualPort.ControlPegs + addressWidth + i, i);
            }

            addressPortA *= bytes;
            addressPortB *= bytes;
            //Write port A
            if (Inputs[Pegs.DualPort.A_W].On)
            {
                ulong data = 0;
                for (int i = 0; i < bitWidth; i++)
                {
                    data |= getPegShifted(Pegs.DualPort.ControlPegs + 2 * addressWidth + i, i);
                }

                for (ulong i = 0; i < bytes; i++)
                {
                    memory[addressPortA + i] = (byte) (data & 0xff);
                    data >>= 8;
                }

                isdatadirty = true;
            }
            
            //Write port B
            if (Inputs[Pegs.DualPort.B_W].On)
            {
                ulong data = 0;
                for (int i = 0; i < bitWidth; i++)
                {
                    data |= getPegShifted(Pegs.DualPort.ControlPegs + 2 * addressWidth + bitWidth + i, i);
                }

                for (ulong i = 0; i < bytes; i++)
                {
                    memory[addressPortB + i] = (byte) (data & 0xff);
                    data >>= 8;
                }

                isdatadirty = true;
            }

            // Port A
            if (Inputs[Pegs.DualPort.A_OE].On)
            {
                //int data = memory[address];
                ulong data = 0;
                for (ulong i = 0; i < bytes; i++)
                {
                    var i2 = bytes - 1 - i;
                    data <<= 8;
                    data |= memory[addressPortA + i2];
                }

                for (int i = 0; i < bitWidth; i++)
                {
                    Outputs[i].On = (data & 1) == 1;
                    data >>= 1;
                }
            }
            else
            {
                for (int i = 0; i < bitWidth; i++)
                {
                    Outputs[i].On = false;
                }
            }
            // Port B
            if (Inputs[Pegs.DualPort.B_OE].On)
            {
                ulong data = 0;
                for (ulong i = 0; i < bytes; i++)
                {
                    var i2 = bytes - 1 - i;
                    data <<= 8;
                    data |= memory[addressPortB + i2];
                }

                for (int i = bitWidth; i < 2 * bitWidth; i++)
                {
                    Outputs[i].On = (data & 1) == 1;
                    data >>= 1;
                }
            }
            else
            {
                for (int i = bitWidth; i < 2 * bitWidth; i++)
                {
                    Outputs[i].On = false;
                }
            }
        }

        protected override void OnCustomDataUpdated()
        {
            if (loadfromsave && Data.Data != null || Data.State == 1 && Data.ClientIncomingData != null)
            {
                var to_load_from = Data.Data;
                if (Data.State == 1)
                {
                    Logger.Info("Loading data from client");
                    to_load_from = Data.ClientIncomingData;
                }

                MemoryStream stream = new MemoryStream(to_load_from);
                stream.Position = 0;
                byte[] mem1 = new byte[memory.Length];
                try
                {
                    DeflateStream decompressor = new DeflateStream(stream, CompressionMode.Decompress);
                    int bytesRead;
                    int nextStartIndex = 0;
                    while ((bytesRead = decompressor.Read(mem1, nextStartIndex, mem1.Length - nextStartIndex)) > 0)
                    {
                        nextStartIndex += bytesRead;
                    }

                    Buffer.BlockCopy(mem1, 0, memory, 0, mem1.Length);
                }
                catch (Exception ex) {
                    Logger.Error("[CheeseUtilMod] Loading data from client failed with exception: " + ex);
                }

                loadfromsave = false;
                if (Data.State == 1)
                {
                    Data.State = 0;
                    Data.ClientIncomingData = new byte[0];
                    isdatadirty = true;
                }

                QueueLogicUpdate();
            }
        }

        protected override void SetDataDefaultValues()
        {
            Data.initialize();
        }

        protected override void SavePersistentValuesToCustomData()
        {
            if (!isdatadirty) return;
            isdatadirty = false;

            MemoryStream memstream = new MemoryStream();
            memstream.Position = 0;
            DeflateStream compressor = new DeflateStream(memstream, CompressionLevel.Optimal, true);
            compressor.Write(memory, 0, memory.Length);
            compressor.Flush();
            int length = (int) memstream.Position;
            memstream.Position = 0;
            byte[] bytes = new byte[length];
            memstream.Read(bytes, 0, length);
            Data.Data = bytes;
        }
    }
}