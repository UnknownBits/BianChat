using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat.DataType.Packet
{
    public interface IDataPacket<TReType>
    {
        public AdvancedTcpClient.PacketType PacketType { get; }

        public byte[] GetBytes();
        public TReType GetDatas(byte[] bytes);
    }
}
