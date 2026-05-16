using System.Collections.Generic;

namespace Event.Application.IServices
{
    public interface IMediaStorageService
    {
        Task<string?> NormalizeImageReferenceAsync(string? value, string folderName);
        Task<List<string>> NormalizeImageReferencesAsync(
            IEnumerable<string>? values,
            string folderName,
            int maxCount = 10);
        string? SanitizePublicReference(string? value);
        string SanitizeJsonPayload(string json);
    }
}
