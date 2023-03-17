using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat_Server.DataType.Packet
{
    public interface IDataPacket<TReType>
    {
        public Threadtcpserver.ClientThread.DataType PacketType { get; }

        public byte[] GetBytes();
        public TReType GetDatas(byte[] bytes);
    }
}
