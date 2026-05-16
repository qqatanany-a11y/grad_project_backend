using System.Text.Json;

namespace events.Helpers
{
    public static class RequestDtoReader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<(T Dto, IFormCollection Form)> ReadAsync<T>(HttpRequest request)
        {
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                var payload = form["data"].ToString();

                if (string.IsNullOrWhiteSpace(payload))
                {
                    throw new Exception("Request data is required.");
                }

                var dtoFromForm = JsonSerializer.Deserialize<T>(payload, JsonOptions);
                if (dtoFromForm == null)
                {
                    throw new Exception("Invalid request data.");
                }

                return (dtoFromForm, form);
            }

            var dto = await request.ReadFromJsonAsync<T>(JsonOptions);
            if (dto == null)
            {
                throw new Exception("Invalid request body.");
            }

            return (dto, null);
        }
    }
}
