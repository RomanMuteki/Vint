﻿using System.Reflection;
using System.Runtime.CompilerServices;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class StructCodec(
    Type type,
    List<PropertyRequest> properties
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        foreach ((PropertyInfo property, ICodecInfo codecInfo) in properties) {
            ICodec codec = Protocol.GetCodec(codecInfo);
            object? item = property.GetValue(value);

            codec.Encode(buffer, item!);
        }
    }

    public override object Decode(ProtocolBuffer buffer) {
        object value = RuntimeHelpers.GetUninitializedObject(type);

        foreach ((PropertyInfo property, ICodecInfo codecInfo) in properties) {
            ICodec codec = Protocol.GetCodec(codecInfo);
            object item = codec.Decode(buffer);

            property.SetValue(value, item, null);
        }

        return value;
    }
}

public readonly record struct PropertyRequest(
    PropertyInfo Property,
    ICodecInfo CodecInfo
);