using System.Text.Json.Serialization;

namespace Lazy.Domain.ValueObjects.Post;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VoteDirection
{
    Up = 1,
    Down = 2
}
