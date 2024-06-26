﻿using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public interface IFilesRepository
{
    /// <summary>
    /// Saves an uploaded image to the database (Id, URL, and UserId) after the image gets uploaded to
    /// the server, and the uploaded image is returned in case of success
    /// </summary>
    /// <param name="image">The uploaded image</param>
    /// <param name="imageType">Determine whether to link the app user id if the image is a profile picture</param>
    /// <param name="threadId">The ID of the thread (Used for creating group chats) to associate the image with</param>
    /// <returns>Image details (ID, URL)</returns>
    Task<Image> PersistImageAsync(Image image, ImageType imageType, Guid? threadId = null);


    /// <summary>
    /// Removes the image record from the database, and from the file system using the Image URL
    /// </summary>
    /// <param name="pictureId">The id of the image to remove</param>
    /// <returns>A boolean indicating whether the operation succeeded or not</returns>
    Task<bool> RemoveImageAsync(Guid imageId);

    /// <summary>
    /// Removes the image record from the db and file system using the relative path
    /// </summary>
    /// <param name="relativeUrl"></param>
    /// <returns></returns>
    Task<bool> RemoveImageByRelativeUrlAsync(string relativeUrl);

    /// <summary>
    /// Gets the image from the database using its id, regardless if the image is a profile picture or
    /// an image sent in a chat thread
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns></returns>
    Task<Image> GetImageAsync(Guid imageId);

    Task<string> SavePDF(IFormFile file, string filePath);
    Task<bool> RemovePDFAsync(Guid documentId);


    //TODO: Add a method to get all the media sent in a chat thread (Requires a nullable ThreadId proeprty be added to the Image class)
}
