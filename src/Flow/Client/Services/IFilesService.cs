using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Http;

namespace Flow.Client.Services;

public interface IFilesService
{
    Task<ApiResponse<ImageDto>> UploadImageAsync(IFormFile image, ImageType imageType, Guid? threadId = null);
    Task<ApiResponse<ImageDto>> GetImageAsync(Guid imageId);
    Task<ApiResponse> DeleteImageAsync(Guid imageId);
}
