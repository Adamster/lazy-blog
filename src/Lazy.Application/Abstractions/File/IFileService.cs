﻿using Microsoft.AspNetCore.Http;

namespace Lazy.Application.Abstractions.File;

public interface IFileService
{
    Task<string?> UploadAsync(IFormFile file, string userName, CancellationToken ct);

    Task DeleteAsync(string fileName, CancellationToken ct);
}