using LogicWorld.Rendering.Components;
using System.IO;
using System.IO.Compression;

using CheeseUtilMod.Shared.CustomData;
using LICC;

namespace CheeseUtilMod.Client
{
    public class DualPortRamResizableClient : ComponentClientCode<IRamResizableData>, FileLoadable
    {
        protected override void Initialize()
        {
            CheeseUtilClient.fileLoadables.Add(this);
        }

        protected override void OnComponentDestroyed()
        {
            CheeseUtilClient.fileLoadables.Remove(this);
        }

        public void Load(byte[] filedata, LineWriter writer, bool force)
        {
            if (force || GetInputState(Pegs.DualPort.LOAD))
            {
                Data.ClientIncomingData = Compress(filedata);
                Data.State = 1;
            }
        }

        static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        protected override void SetDataDefaultValues()
        {
            Data.initialize();
        }
    }
}
