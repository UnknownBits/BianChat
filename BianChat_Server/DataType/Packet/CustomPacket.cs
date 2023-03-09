using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat_Server.DataType.Packet
{
    public class CustomPacket : IDataPacket<byte[][]>
    {
        public Threadtcpserver.ClientThread.DataType PacketType { get; set; }
        public List<byte[]> DataList { get; set; } = new List<byte[]>();

        public byte[] GetBytes()
        {
            List<byte> data = new List<byte> { (byte)PacketType };
            lock (DataList)
            {
                data.AddRange(BitConverter.GetBytes(DataList.Count));
                foreach (var item in DataList)
                {
                    int length = item.Length;
                    data.AddRange(BitConverter.GetBytes(length));
                    data.AddRange(item);
                }
            }
            return data.ToArray();
        }

        public byte[][] GetDatas(byte[] bytes)
        {
            List<byte[]> datas = new List<byte[]>();
            int count = BitConverter.ToInt32(bytes);
            int idx = 4;
            for (int i = 0; i < count; i++)
            {
                int length = BitConverter.ToInt32(bytes, idx);
                idx += 4;
                datas.Add(bytes.Skip(idx).Take(length).ToArray());
                idx += length;
            }
            return datas.ToArray();
        }
    }
}
