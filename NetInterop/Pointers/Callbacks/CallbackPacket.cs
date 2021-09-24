﻿using NetInterop.Transport.Core.Abstractions.Packets;
using NetInterop.Transport.Core.Packets.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetInterop
{
    /// <summary>
    /// Registers the given callback with the delegate handler and obtains and appends the callback id to the front of the packet
    /// </summary>
    /// <typeparam name="TPacket"></typeparam>
    public class CallbackPacket<TPacket> : IPacketSerializable<TPacket> where TPacket : Enum, IConvertible
    {
        private readonly Action<bool, IPacket> callback;
        private readonly IDelegateHandler<bool, IPacket> packetCallbackHandler;
        private readonly IPacketSerializable packet;

        /// <summary>
        /// Registers the given callback with the delegate handler and obtains and appends the callback id to the front of the packet
        /// </summary>
        /// <typeparam name="TPacket"></typeparam>
        public CallbackPacket(Action<bool, IPacket> callback, IPacketSerializable packet, IDelegateHandler<bool, IPacket> packetCallbackHandler)
        {
            this.packet = packet;
            this.packetCallbackHandler = packetCallbackHandler;
            this.callback = callback;
        }

        public TPacket PacketType { get; }

        public int EstimatePacketSize() => sizeof(ushort) + packet.EstimatePacketSize();

        public void Serialize(IPacket packetBuilder)
        {
            if (callback is null)
            {
                packetBuilder.AppendUShort(0);
            }
            else
            {
                packetBuilder.AppendUShort(packetCallbackHandler.Register(callback));
            }

            packet.Serialize(packetBuilder);
        }
    }
}