using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crash.Core.Drawing
{
    public sealed class JsonColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Color.FromStringHashRgbaOrRgb(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color color, JsonSerializerOptions options)
        {
            writer.WriteStringValue(color.ToStringHashRgba(stackalloc char[Color.LengthOfStringHashRgba]));
        }
    }
}
