using System.Text.Json;
using System.Text.Json.Nodes;

namespace TransportApp_API.Services
{
    public class JsonFileService
    {
        private readonly IWebHostEnvironment _environment;

        private readonly JsonSerializerOptions _readOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly JsonSerializerOptions _writeOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public JsonFileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<T>> ReadJsonAsync<T>(string fileName)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Data", fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException(fileName);

            var json = await File.ReadAllTextAsync(path);

            return JsonSerializer.Deserialize<List<T>>(json, _readOptions) ?? new List<T>();
        }

        public async Task WriteJsonAsync<T>(string fileName, List<T> data)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Data", fileName);

            var json = JsonSerializer.Serialize(data, _writeOptions);

            await File.WriteAllTextAsync(path, json);
        }

        public async Task<JsonNode?> ReadJsonNodeAsync(string fileName)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Data", fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException(fileName);

            var json = await File.ReadAllTextAsync(path);

            return JsonNode.Parse(json);
        }

        public async Task WriteJsonNodeAsync(string fileName, JsonNode data)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Data", fileName);

            var json = data.ToJsonString(_writeOptions);

            await File.WriteAllTextAsync(path, json);
        }
    }
}