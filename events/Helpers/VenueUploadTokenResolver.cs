namespace events.Helpers
{
    public static class VenueUploadTokenResolver
    {
        public static List<string> ReplaceTokens(
            IEnumerable<string> values,
            IReadOnlyDictionary<string, string> uploadedPathsByToken)
        {
            var resolvedValues = new List<string>();
            if (values == null)
            {
                return resolvedValues;
            }

            foreach (var value in values)
            {
                var resolvedValue = ReplaceToken(value, uploadedPathsByToken);
                if (!string.IsNullOrWhiteSpace(resolvedValue))
                {
                    resolvedValues.Add(resolvedValue);
                }
            }

            return resolvedValues;
        }

        public static string ReplaceToken(
            string value,
            IReadOnlyDictionary<string, string> uploadedPathsByToken)
        {
            var trimmedValue = value?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedValue))
            {
                return trimmedValue;
            }

            if (!trimmedValue.StartsWith("__upload__:", StringComparison.Ordinal))
            {
                return trimmedValue;
            }

            if (!uploadedPathsByToken.TryGetValue(trimmedValue, out var resolvedValue))
            {
                throw new Exception("One or more uploaded venue images are missing.");
            }

            return resolvedValue;
        }
    }
}
