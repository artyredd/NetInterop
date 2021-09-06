﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace RemoteInvoke.Net.Transport.Packets
{
    public static class PacketDispatcherExtensions
    {
        public delegate T PacketConverter<T, TPacketKind>(ref Packet<TPacketKind> packet) where TPacketKind : Enum, IConvertible;

        /// <summary>
        /// Waits for a packet and uses <paramref name="Converter"/> to convert the packet into a <typeparamref name="T"/>
        /// <para>
        /// Blocking;
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TPacketKind"></typeparam>
        /// <param name="packetDispatcher"></param>
        /// <param name="Converter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static T? WaitAndConvertPacket<T, TPacketKind>(this IPacketDispatcher<TPacketKind> packetDispatcher, PacketConverter<T, TPacketKind> Converter, CancellationToken token)
            where TPacketKind : Enum, IConvertible
        {
            Packet<TPacketKind> packet = packetDispatcher.WaitForPacket(token);

            T converted = Converter(ref packet);

            return converted;
        }
    }
}
