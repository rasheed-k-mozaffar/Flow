
using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public class FilesRepository : IFilesRepository
{
    private readonly AppDbContext _db;
    private readonly UserInfo _userInfo;

    public FilesRepository
    (
        AppDbContext db,
        UserInfo userInfo
    )
    {
        _db = db;
        _userInfo = userInfo;
    }

    public async Task<Image> GetImageAsync(Guid imageId)
    {
        var image = await _db
                        .Images
                        .FirstOrDefaultAsync(img => img.Id == imageId);

        if (image is null)
            throw new ResourceNotFoundException(message: "The image you're looking for was not found");

        return image;
    }

    public async Task<Image> PersistImageAsync(Image image, ImageType imageType, Guid? threadId = null)
    {
        if (imageType is ImageType.ProfilePicture) // give the AppUserId property a value before persisting the image
        {
            image.AppUserId = _userInfo.UserId;
        }
        else if (imageType is ImageType.GroupPicture)
        {
            image.ChatThreadId = threadId;
        }

        image.Type = imageType;
        var entityEntry = await _db
                                .Images
                                .AddAsync(image);

        if (entityEntry.State is EntityState.Added)
        {
            await _db.SaveChangesAsync();
            return image;
        }

        throw new ImagePersistanceFailedException(message: "Something failed while saving the image");
    }

    public async Task<bool> RemoveImageAsync(Guid imageId)
    {
        var image = await _db
                        .Images
                        .FindAsync(imageId);

        if (image is null)
            throw new ResourceNotFoundException(message: "The image you're looking for was not found");

        try
        {
            File.Delete(image.FilePath);

            var removalResult = _db.Images.Remove(image);

            if (removalResult.State == EntityState.Deleted)
            {
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (ArgumentNullException)
        {
            return false;
        }

        return false; // failure for uncaught reason
    }
    public async Task<string> SavePDF(IFormFile file, string filePath)
    {
        string path = "";

        Guid fileIdentifier = Guid.NewGuid();
        try
        {

            path = Path.Combine(filePath, fileIdentifier.ToString());
            string url = $"/pdfs/{fileIdentifier}";
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            var PdfFile = new PDF { FilePath = path, RelativeUrl = url };
            PdfFile.AppUserId = _userInfo.UserId;
            await _db.PDFs.AddAsync(PdfFile);
            await _db.SaveChangesAsync();
            return PdfFile.RelativeUrl;

        }
        catch (Exception)
        {
            throw new PDFdocumentSaveFailedException("PDF document save failed");
        }
    }
    public async Task<bool> RemovePDFAsync(Guid documentId)
    {
        var document = await _db
                        .PDFs
                        .FindAsync(documentId);

        if (document is null)
            throw new ResourceNotFoundException(message: "The document you're looking for was not found");

        try
        {
            File.Delete(document.FilePath);

            var removalResult = _db.PDFs.Remove(document);

            if (removalResult.State == EntityState.Deleted)
            {
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (ArgumentNullException)
        {
            return false;
        }

        return false; // failure for uncaught reason
    }

    public async Task<bool> RemoveImageByRelativeUrlAsync(string relativeUrl)
    {
        var image = await _db
                        .Images
                        .AsTracking()
                        .FirstOrDefaultAsync(p => p.RelativeUrl == relativeUrl);

        if (image is null)
            throw new ResourceNotFoundException("The image was not found");


        try
        {
            File.Delete(image.FilePath);

            var removalResult = _db.Images.Remove(image);

            if (removalResult.State == EntityState.Deleted)
            {
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (ArgumentNullException)
        {
            return false;
        }

        return false;
    }
}
