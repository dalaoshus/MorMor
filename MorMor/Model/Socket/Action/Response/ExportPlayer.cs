﻿using MorMor.Model.Socket.Internet;
using ProtoBuf;

namespace MorMor.Model.Socket.Action.Response;

[ProtoContract]
public class ExportPlayer : BaseActionResponse
{
    [ProtoMember(8)] public List<PlayerFile> PlayerFiles { get; set; }
}
