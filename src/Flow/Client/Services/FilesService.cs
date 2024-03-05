using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Http;

namespace Flow.Client.Services;

public class FilesService : IFilesService
{
    private const string BASE_URL = "/api/files";
    private const string UPLOAD_IMAGE_URL = $"{BASE_URL}/upload-picture";
    private const string GET_IMAGE_URL = $"{BASE_URL}/get-image";
    private const string DELETE_IMAGE_URL = $"{BASE_URL}/delete-image";

    private readonly HttpClient _httpClient;

    public FilesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<ApiResponse> DeleteImageAsync(Guid imageId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<ImageDto>> GetImageAsync(Guid imageId)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<ImageDto>> UploadImageAsync(IFormFile image, ImageType imageType)
    {
        var formData = new MultipartFormDataContent();
        var streamContent = new StreamContent(image.OpenReadStream());
        formData.Add(streamContent, "file", image.FileName);

        string requestUrl = $"{UPLOAD_IMAGE_URL}?imageType={imageType}";
        HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, formData);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new FileUploadFailedException(errorResponse!.ErrorMessage);
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<ImageDto>>();
        return data!;
    }
}
