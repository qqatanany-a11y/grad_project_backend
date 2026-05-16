using System.Text.Json;
using System.Text.Json.Nodes;
using Event.Application.IServices;
using Microsoft.AspNetCore.Http;

namespace events.Services
{
    public class MediaStorageService : IMediaStorageService
    {
        private const int MaxImageBytes = 5 * 1024 * 1024;
        private static readonly StringComparer PathComparer = StringComparer.Ordinal;
        private static readonly Dictionary<string, string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

        private readonly IWebHostEnvironment _environment;

        public MediaStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> NormalizeImageReferenceAsync(string value, string folderName)
        {
            var trimmedValue = NormalizeString(value);
            if (trimmedValue == null)
            {
                return null;
            }

            if (!IsDataUrl(trimmedValue))
            {
                return NormalizeStoredPath(trimmedValue);
            }

            var (mimeType, base64Value) = ParseDataUrl(trimmedValue);
            ValidateBase64PayloadSize(base64Value);

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64Value);
            }
            catch (FormatException)
            {
                throw new Exception("Invalid image data was provided.");
            }

            var extension = ValidateImagePayload(mimeType, bytes);
            return await SaveBytesAsync(bytes, folderName, extension);
        }

        public async Task<List<string>> NormalizeImageReferencesAsync(
            IEnumerable<string> values,
            string folderName,
            int maxCount = 10)
        {
            var normalizedValues = new List<string>();

            if (values == null)
            {
                return normalizedValues;
            }

            foreach (var value in values)
            {
                var normalizedValue = await NormalizeImageReferenceAsync(value, folderName);
                if (string.IsNullOrWhiteSpace(normalizedValue))
                {
                    continue;
                }

                if (!normalizedValues.Contains(normalizedValue, PathComparer))
                {
                    normalizedValues.Add(normalizedValue);
                }
            }

            if (maxCount > 0 && normalizedValues.Count > maxCount)
            {
                throw new Exception($"A maximum of {maxCount} images is allowed.");
            }

            return normalizedValues;
        }

        public async Task<string> SaveUploadedImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length <= 0)
            {
                return null;
            }

            if (file.Length > MaxImageBytes)
            {
                throw new Exception("Each image must be 5 MB or smaller.");
            }

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var bytes = memoryStream.ToArray();
            var extension = ValidateImagePayload(file.ContentType, bytes);

            return await SaveBytesAsync(bytes, folderName, extension);
        }

        public async Task<Dictionary<string, string>> SaveUploadedImagesByTokenAsync(
            IEnumerable<string> tokens,
            IReadOnlyList<IFormFile> files,
            string folderName)
        {
            var normalizedTokens = tokens
                .Select(NormalizeString)
                .Where(token => token != null)
                .Cast<string>()
                .ToList();

            if (normalizedTokens.Count != files.Count)
            {
                throw new Exception("Uploaded image data is invalid.");
            }

            var uniqueTokens = new HashSet<string>(normalizedTokens, StringComparer.Ordinal);
            if (uniqueTokens.Count != normalizedTokens.Count)
            {
                throw new Exception("Uploaded image data contains duplicate tokens.");
            }

            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var index = 0; index < normalizedTokens.Count; index++)
            {
                result[normalizedTokens[index]] =
                    await SaveUploadedImageAsync(files[index], folderName)
                    ?? throw new Exception("Uploaded image data is invalid.");
            }

            return result;
        }

        public string SanitizePublicReference(string value)
        {
            var trimmedValue = NormalizeString(value);
            if (trimmedValue == null || IsDataUrl(trimmedValue))
            {
                return null;
            }

            return NormalizeStoredPath(trimmedValue);
        }

        public string SanitizeJsonPayload(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return json;
            }

            try
            {
                var node = JsonNode.Parse(json);
                var sanitizedNode = SanitizeNode(node);
                return sanitizedNode?.ToJsonString() ?? "{}";
            }
            catch
            {
                return json;
            }
        }

        private static JsonNode SanitizeNode(JsonNode node)
        {
            if (node == null)
            {
                return null;
            }

            if (node is JsonObject jsonObject)
            {
                var sanitizedObject = new JsonObject();
                foreach (var item in jsonObject)
                {
                    sanitizedObject[item.Key] = SanitizeNode(item.Value);
                }

                return sanitizedObject;
            }

            if (node is JsonArray jsonArray)
            {
                var sanitizedArray = new JsonArray();
                foreach (var item in jsonArray)
                {
                    var sanitizedItem = SanitizeNode(item);
                    if (sanitizedItem != null)
                    {
                        sanitizedArray.Add(sanitizedItem);
                    }
                }

                return sanitizedArray;
            }

            if (node is JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<string>(out var stringValue))
                {
                    return IsDataUrl(stringValue)
                        ? null
                        : JsonValue.Create(stringValue);
                }

                return node.DeepClone();
            }

            return node.DeepClone();
        }

        private async Task<string> SaveBytesAsync(byte[] bytes, string folderName, string extension)
        {
            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads");
            var normalizedFolderName = NormalizeFolderName(folderName);
            var targetDirectory = Path.Combine(uploadsRoot, normalizedFolderName);

            Directory.CreateDirectory(targetDirectory);

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(targetDirectory, fileName);

            await File.WriteAllBytesAsync(filePath, bytes);

            return $"/uploads/{normalizedFolderName}/{fileName}";
        }

        private static string NormalizeFolderName(string folderName)
        {
            var trimmedFolderName = NormalizeString(folderName);
            return trimmedFolderName == null
                ? "general"
                : trimmedFolderName.Replace('\\', '/').Trim('/').ToLowerInvariant();
        }

        private static string NormalizeStoredPath(string value)
        {
            return value.Replace('\\', '/').Trim();
        }

        private static (string MimeType, string Base64Value) ParseDataUrl(string dataUrl)
        {
            var commaIndex = dataUrl.IndexOf(',');
            if (commaIndex <= 0)
            {
                throw new Exception("Invalid image data was provided.");
            }

            var metadata = dataUrl[..commaIndex];
            var base64Value = dataUrl[(commaIndex + 1)..];
            if (!metadata.EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Only base64-encoded images are supported.");
            }

            var mimeType = metadata["data:".Length..^";base64".Length];
            return (mimeType, base64Value);
        }

        private static string ValidateImagePayload(string mimeType, byte[] bytes)
        {
            var normalizedMimeType = NormalizeString(mimeType);
            if (normalizedMimeType == null || !AllowedImageMimeTypes.TryGetValue(normalizedMimeType, out var extension))
            {
                throw new Exception("Only JPEG, PNG, and WebP images are allowed.");
            }

            if (bytes.Length == 0)
            {
                throw new Exception("Empty image uploads are not allowed.");
            }

            if (bytes.Length > MaxImageBytes)
            {
                throw new Exception("Each image must be 5 MB or smaller.");
            }

            if (!MatchesImageSignature(normalizedMimeType, bytes))
            {
                throw new Exception("The uploaded file content does not match its image type.");
            }

            return extension;
        }

        private static bool MatchesImageSignature(string mimeType, byte[] bytes)
        {
            return mimeType.ToLowerInvariant() switch
            {
                "image/jpeg" => bytes.Length >= 3 &&
                                bytes[0] == 0xFF &&
                                bytes[1] == 0xD8 &&
                                bytes[2] == 0xFF,
                "image/png" => bytes.Length >= 8 &&
                               bytes[0] == 0x89 &&
                               bytes[1] == 0x50 &&
                               bytes[2] == 0x4E &&
                               bytes[3] == 0x47 &&
                               bytes[4] == 0x0D &&
                               bytes[5] == 0x0A &&
                               bytes[6] == 0x1A &&
                               bytes[7] == 0x0A,
                "image/webp" => bytes.Length >= 12 &&
                                bytes[0] == (byte)'R' &&
                                bytes[1] == (byte)'I' &&
                                bytes[2] == (byte)'F' &&
                                bytes[3] == (byte)'F' &&
                                bytes[8] == (byte)'W' &&
                                bytes[9] == (byte)'E' &&
                                bytes[10] == (byte)'B' &&
                                bytes[11] == (byte)'P',
                _ => false
            };
        }

        private static void ValidateBase64PayloadSize(string base64Value)
        {
            var sanitizedValue = NormalizeString(base64Value);
            if (sanitizedValue == null)
            {
                throw new Exception("Invalid image data was provided.");
            }

            var padding = 0;
            if (sanitizedValue.EndsWith("==", StringComparison.Ordinal))
            {
                padding = 2;
            }
            else if (sanitizedValue.EndsWith("=", StringComparison.Ordinal))
            {
                padding = 1;
            }

            var estimatedBytes = (long)sanitizedValue.Length * 3 / 4 - padding;
            if (estimatedBytes > MaxImageBytes)
            {
                throw new Exception("Each image must be 5 MB or smaller.");
            }
        }

        private static bool IsDataUrl(string value)
        {
            return value.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeString(string value)
        {
            var trimmedValue = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmedValue) ? null : trimmedValue;
        }
    }
}
