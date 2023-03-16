using Lazy.Domain.Entities;

namespace Lazy.Application.Abstractions;

public interface IJwtProvider
{
    string Generate(User user);
}