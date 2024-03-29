﻿using NetInterop.Transport.Core.Abstractions.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetInterop
{
    public interface IPointerProvider : IPacketSerializer<INetPtr>, IPacketDeserializer<INetPtr>
    {
        INetPtr Create(ushort typeId, ushort instanceId);
        INetPtr<T> Create<T>(ushort typeId, ushort instanceId);
    }
}
