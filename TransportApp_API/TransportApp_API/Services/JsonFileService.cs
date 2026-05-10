using System.Text.Json;

namespace TransportApp_API.Services
{
    public class JsonFileService
    {
        private readonly IWebHostEnvironment _environment;

        public JsonFileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<T>> ReadJsonAsync<T>(string fileName)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Data", fileName);
            if (!File.Exists(path)) throw new FileNotFoundException(fileName);
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        public async Task WriteJsonAsync<T>(string fileName, List<T> data)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Data", fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(path, json);
        }
    }
}