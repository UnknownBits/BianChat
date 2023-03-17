using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat.DataType.Packet
{
    public class StringCustomPacket : IDataPacket<string[]>
    {
        public AdvancedTcpClient.PacketType PacketType { get; set; }
        public List<string> DataList { get; set; } = new List<string>();
        
        public byte[] GetBytes()
        {
            List<byte> data = new List<byte> { (byte)PacketType };
            lock (DataList)
            {
                data.AddRange(BitConverter.GetBytes(DataList.Count));
                foreach (var item in DataList)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(item);
                    int length = bytes.Length;
                    data.AddRange(BitConverter.GetBytes(length));
                    data.AddRange(bytes);
                }
            }
            return data.ToArray();
        }

        public string[] GetDatas(byte[] bytes)
        {
            List<string> datas = new List<string>() ;
            int count = BitConverter.ToInt32(bytes);
            int idx = 4;
            for (int i = 0; i < count; i++)
            {
                int length = BitConverter.ToInt32(bytes, idx);
                idx += 4;
                datas.Add(Encoding.UTF8.GetString(bytes.Skip(idx).Take(length).ToArray()));
                idx += length;
            }
            return datas.ToArray();
        }
    }
}
