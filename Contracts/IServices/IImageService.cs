
namespace Contracts.IServices;

public interface IImageService
{
    Task<string> SaveImageAsync(Stream imageData, string fileNameWithoutExt, string rootPath, CancellationToken ct = default);
    Task<string> UpdateImageAsync(Stream imageData, string fileNameWithoutExt, string rootPath, CancellationToken ct = default);
    void RemoveImage(string fileNameWithoutExt, string rootPath);
    Task<List<string>> SaveImagesAsync(IEnumerable<Stream> imagesData, string rootPath, CancellationToken ct = default);
}
