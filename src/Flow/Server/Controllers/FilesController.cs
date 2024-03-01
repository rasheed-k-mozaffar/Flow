using Flow.Shared.ApiResponses;
using Flow.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private static readonly IEnumerable<string> _allowedFileExtensions = new List<string>
    {
        ".jpg", ".jpeg", ".png"
    };


    private readonly IFilesRepository _filesRepository;
    private readonly ILogger<FilesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public FilesController
    (
        IFilesRepository filesRepository,
        ILogger<FilesController> logger,
        IWebHostEnvironment environment
    )
    {
        _filesRepository = filesRepository;
        _logger = logger;
        _environment = environment;
    }

    [HttpPost]
    [Route("upload-picture")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<ImageDto>))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ApiErrorResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(EmptyResult))]

    public async Task<IActionResult> UploadPicture([FromForm] IFormFile file, [FromQuery] ImageType imageType)
    {
        if (file is null)
        {
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = "Invalid request"
            });
        }

        string fileName = file.FileName;
        string fileExtension = Path.GetExtension(fileName);
        \
        if (!_allowedFileExtensions.Contains(fileExtension)) // * use of unallowed extension
        {
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = "File with the extension ({fileExtension}) are not allowed"
            });
        }

        string newFileName = $"{Guid.NewGuid()}{fileExtension}";
        string filePath = Path.Combine
                            (
                                _environment.WebRootPath,
                                "images",
                                newFileName
                            );


        using FileStream stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream);

        try
        {
            string url = $"/images/{newFileName}";
            var image = await _filesRepository
                                .PersistImageAsync
                                (
                                    new Image { RelativeUrl = url, FilePath = filePath },
                                    imageType
                                );

            _logger.LogInformation
            (
                "New image was uploaded with the ID: {imageId}",
                image.Id
            );

            var imageDto = image.ToImageDto();

            return Ok(new ApiResponse<ImageDto>
            {
                Message = "Image saved successfully",
                Body = imageDto,
                IsSuccess = true
            });
        }
        catch (ImagePersistanceFailedException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }


    [HttpGet]
    [Route("get-image/{imageId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<ImageDto>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(EmptyResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> GetImage(Guid imageId)
    {
        try
        {
            var image = await _filesRepository
                .GetImageAsync(imageId);

            var imageDto = image.ToImageDto();

            return Ok(new ApiResponse<ImageDto>
            {
                Message = "Image retrieved successfully",
                Body = imageDto,
                IsSuccess = true
            });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }
    [HttpDelete]
    [Route("delete-image/{imageId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ApiErrorResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(EmptyResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        try
        {
            bool imageRemoved = await _filesRepository
                .RemoveImageAsync(imageId);

            if (imageRemoved)
            {
                _logger.LogInformation
                (
                    "Image with the ID: {imageId} was deleted from the server",
                    imageId
                );

                return Ok(new ApiResponse
                {
                    Message = "Image deleted successfully",
                    IsSuccess = true
                });
            }

            _logger.LogError
            (
                "Failed attempt to delete image with the ID: {imageId}",
                imageId
            );

            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = "Failed to delete image! Please try again"
            });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }
    [HttpGet]
    [Route("delete-pdf/{pdfId}")]
    public async Task<IActionResult> DeletePDFDocument(Guid pdfId)
    {
        try
        {
            bool imageRemoved = await _filesRepository
                .RemovePDFAsync(pdfId);

            if (imageRemoved)
            {
                _logger.LogInformation
                (
                    "Document with the ID: {pdfId} was deleted from the server",
                    pdfId
                );

                return Ok(new ApiResponse
                {
                    Message = "Document deleted successfully",
                    IsSuccess = true
                });
            }

            _logger.LogError
            (
                "Failed attempt to delete document with the ID: {pdfId}",
                pdfId
            );

            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = "Failed to delete document! Please try again"
            });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }

    }
    [HttpPost]
    [Route("upload-PDFfile")]
    public async Task<IActionResult> RecievePDF([FromBody] IFormFile file)
    {
        if (file is null)
        {
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = "Invalid request"
            });
        }
        string fileName = file.FileName;
        string fileExtension = Path.GetExtension(fileName);

        if (fileExtension == ".pdf" && file.Length > 0)
        {
            try
            {

                string filePath = Path.Combine
                                    (
                                        _environment.WebRootPath,
                                        "pdfs"
                                    );
                var resultingPath = await _filesRepository.SavePDF(file, filePath);

                return Ok(new ApiResponse<string>
                {
                    Message = "file uploaded successfully",
                    IsSuccess = true,
                    Body= resultingPath
                });

            }

            catch (Exception ex)
            {
                //Log ex
                return Ok(new ApiResponse
                {
                    Message = "file upload fail",
                    IsSuccess = true
                });
            }
        }
        else
        {
            return Ok(new ApiResponse
            {
                Message = "illegal operation",
                IsSuccess = false
            });
        }
    }
}

