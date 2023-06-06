using Microsoft.AspNetCore.Http;

namespace Lazy.Application.Abstractions.File;

public interface IFileService
{
    string Add(IFormFile file);

    void Delete(string fileName);
}