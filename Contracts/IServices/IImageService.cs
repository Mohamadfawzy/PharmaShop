
namespace Contracts.IServices;

public interface IImageService
{
    /// <summary>
    /// Save a new image in different sizes and return its unique ID
    /// </summary>
    //Task<string> SaveImageAsync(Stream imageData, string rootPath, CancellationToken ct = default);
    //Task<string> UpdateImageAsync(Stream imageData, string fileNameWithoutExt, string rootPath, CancellationToken ct = default);
    //void RemoveImage(string fileNameWithoutExt, string rootPath);
    //Task<List<string>> SaveImagesAsync(IEnumerable<Stream> imagesData, string rootPath, CancellationToken ct = default);

    /// <summary>
    /// Save a new image in different sizes and return its unique ID
    /// </summary>
    Task<string> SaveImageAsync(Stream imageData, string rootPath, string prefix = "", CancellationToken ct = default);

    /// <summary>
    /// Replace an existing image with a new one
    /// </summary>
    Task<string> ReplaceImageAsync(Stream imageData, string imageId, string rootPath, CancellationToken ct = default);

    /// <summary>
    /// Remove an image by name
    /// </summary>
    Task RemoveImageAsync(string fileNameWithoutExt, string rootPath);

    /// <summary>
    /// Save multiple images and return their IDs
    /// </summary>
    Task<List<string>> SaveMultipleImagesAsync(IEnumerable<Stream> imagesData, string rootPath, CancellationToken ct = default);

}
