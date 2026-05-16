using System;
using System.Collections.Generic;
using System.Linq;
using events.domain.Entities;

namespace Event.Application.Helpers
{
    public static class VenueImageRequestHelper
    {
        public static List<string> Resolve(
            IEnumerable<string>? imageUrls = null,
            string? coverPhotoDataUrl = null,
            IEnumerable<string>? galleryPhotoDataUrls = null,
            IEnumerable<string>? photoDataUrls = null)
        {
            var orderedUrls = new List<string>();

            AppendUrl(orderedUrls, coverPhotoDataUrl);
            AppendUrls(orderedUrls, photoDataUrls);
            AppendUrls(orderedUrls, galleryPhotoDataUrls);
            AppendUrls(orderedUrls, imageUrls);

            return orderedUrls
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        public static List<string> OrderExistingImages(IEnumerable<VenueImage>? images)
        {
            if (images == null)
            {
                return new List<string>();
            }

            return images
                .Where(image =>
                    !string.IsNullOrWhiteSpace(image.ImageUrl) &&
                    !LooksLikeDataUrl(image.ImageUrl))
                .OrderByDescending(image => image.IsCover)
                .ThenBy(image => image.Id)
                .Select(image => image.ImageUrl.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static void AppendUrls(List<string> target, IEnumerable<string>? source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var value in source)
            {
                AppendUrl(target, value);
            }
        }

        private static void AppendUrl(List<string> target, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            target.Add(value.Trim());
        }

        private static bool LooksLikeDataUrl(string value)
        {
            return value.TrimStart().StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        }
    }
}
