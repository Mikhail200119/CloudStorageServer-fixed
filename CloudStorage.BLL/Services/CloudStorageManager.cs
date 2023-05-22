using System.IO.Compression;
using System.Net;
using System.Text;
using AutoMapper;
using CloudStorage.BLL.Exceptions;
using CloudStorage.BLL.Helpers;
using CloudStorage.BLL.Models;
using CloudStorage.BLL.Options;
using CloudStorage.BLL.Services.Interfaces;
using CloudStorage.DAL;
using CloudStorage.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CloudStorage.BLL.Services;

public class CloudStorageManager : ICloudStorageManager
{
    private readonly ICloudStorageUnitOfWork _cloudStorageUnitOfWork;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDataHasher _dataHasher;
    private readonly IDeduplicationService _deduplicationService;
    private readonly ArchiveOptions _archiveOptions;

    public CloudStorageManager(ICloudStorageUnitOfWork cloudStorageUnitOfWork, IMapper mapper, IUserService userService, IFileStorageService fileStorageService, IDataHasher dataHasher, IOptions<ArchiveOptions> archiveOptions, IDeduplicationService deduplicationService)
    {
        _cloudStorageUnitOfWork = cloudStorageUnitOfWork;
        _mapper = mapper;
        _userService = userService;
        _fileStorageService = fileStorageService;
        _dataHasher = dataHasher;
        _deduplicationService = deduplicationService;
        _archiveOptions = archiveOptions.Value;
    }

    public async Task<IEnumerable<FileDescription>> CreateAsync(IEnumerable<FileCreateData> files)
    {
        var filesArray = files.ToArray();
        var filesDbModels = _mapper.Map<IEnumerable<FileCreateData>, IEnumerable<FileDescriptionDbModel>>(filesArray);
        var f = filesArray.DistinctBy(data => data.Name);

        if (f.Count() < filesArray.Length)
        {
            throw new FileNameDuplicationException("Some files have the same name.");
        }

        var finalDbModelsToUpload = filesDbModels.Select(model =>
        {
            var content = filesArray.Single(file => file.Name == model.ProvidedName).Content;
            model.ContentHash = _dataHasher.HashStreamData(content);
            model.UploadedBy = _userService.Current.Email;
            model.Extension = Path.GetExtension(filesArray.Single(file => file.Name == model.ProvidedName).Name)[1..];

            return model;
        }).ToArray();

        var namesWithContents = new List<(string FileName, Stream Content)>();

        foreach (var fileCreateData in filesArray)
        {
            var uniqueName = finalDbModelsToUpload.Single(file => file.ProvidedName == fileCreateData.Name).UniqueName;

            namesWithContents.Add((uniqueName, fileCreateData.Content));
        }

        await _fileStorageService.UploadRangeAsync(namesWithContents);

        var dbModelsWithContent = finalDbModelsToUpload.Select(model =>
        {
            var content = filesArray.Single(file => file.Name == model.ProvidedName).Content;

            return (model, content);
        }).ToArray();

        //await ValidateCreatedFile(finalDbModelsToUpload);
        await SetFilesThumbnail(dbModelsWithContent);

        try
        {
            await _cloudStorageUnitOfWork.FileDescription.CreateRangeAsync(finalDbModelsToUpload);
            await _cloudStorageUnitOfWork.SaveChangesAsync();
        }
        catch
        {
            var uniqueNames = finalDbModelsToUpload
                .Select(file => file.UniqueName)
                .ToArray();

            await _fileStorageService.DeleteRangeAsync(uniqueNames);

            throw;
        }

        var fileDescriptions = _mapper.Map<IEnumerable<FileDescriptionDbModel>, IEnumerable<FileDescription>>(finalDbModelsToUpload);
        
        return fileDescriptions;
    }

    public async Task<(Stream Data, string ContentType, string DownloadName)> GetFileStreamAsync(int fileId)
    {
        var item = await _cloudStorageUnitOfWork.FileDescription.GetByIdAsync(fileId);

        if (item is null)
        {
            return await Task.FromResult<(Stream, string, string)>((Stream.Null, string.Empty, string.Empty)!);
        }
                
        ValidateUserPermissions(item);

        var content = await _fileStorageService.GetStreamAsync(item.UniqueName) as FileStream;

        var downloadName = $"{Path.GetFileNameWithoutExtension(item.ProvidedName)}.{item.Extension}";

        return (content, item.ContentType, downloadName)!;
    }

    public async Task<(Stream Data, string ContentType)> GetThumbnailStreamAndContentTypeAsync(int thumbId)
    {
        var thumbDbModel = await _cloudStorageUnitOfWork.ThumbnailInfo.GetByIdAsync(thumbId);

        if (thumbDbModel is null)
        {
            return (Stream.Null, string.Empty);
        }
        
        var data = await _fileStorageService.GetStreamAsync(thumbDbModel.UniqueName!);

        return (data, "image/png");
    }

    public async Task<FileDescription> UpdateAsync(FileUpdateData existingFile)
    {
        var fileDbModel = _mapper.Map<FileUpdateData, FileDescriptionDbModel>(existingFile);

        _cloudStorageUnitOfWork.FileDescription.Update(fileDbModel);
        await _cloudStorageUnitOfWork.SaveChangesAsync();

        var file = _mapper.Map<FileDescriptionDbModel, FileDescription>(fileDbModel);

        return file;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _cloudStorageUnitOfWork.FileDescription.GetByIdAsync(id);
        ValidateUserPermissions(item);

        if (item is null)
        {
            return;
        }

        _cloudStorageUnitOfWork.FileDescription.Delete(item.Id);

        await _cloudStorageUnitOfWork.SaveChangesAsync();

        _fileStorageService.Delete(item.UniqueName);

        if (item.ThumbnailInfo is not null)
        {
            _fileStorageService.Delete(item.ThumbnailInfo.UniqueName);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<int> ids)
    {
        var fileIds = ids.ToArray();
        _cloudStorageUnitOfWork.FileDescription.DeleteRange(fileIds);
        
        var filesToDelete = _cloudStorageUnitOfWork.FileDescription
            .GetAllFilesAsQueryable(_userService.Current.Email)
            .AsNoTracking()
            .Where(file => ids.Contains(file.Id))
            .ToArray();

        ValidateUserPermissions(filesToDelete);

        await _fileStorageService.DeleteRangeAsync(filesToDelete.Select(file => file.UniqueName).ToArray());

        var thumbs = filesToDelete
            .Where(file => file.ThumbnailInfo is not null)
            .Select(file => file.ThumbnailInfo!.UniqueName)
            .ToArray();

        await _fileStorageService.DeleteRangeAsync(thumbs);
        await _cloudStorageUnitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<FileDescription>> GetAllFilesAsync()
    {
        var filesDbModel = await _cloudStorageUnitOfWork.FileDescription.GetAllFilesAsync(_userService.Current.Email);
        ValidateUserPermissions(filesDbModel.ToArray());

        var files = _mapper.Map<IEnumerable<FileDescriptionDbModel>, IEnumerable<FileDescription>>(filesDbModel);

        var allFilesAsync = files as FileDescription[] ?? files.ToArray();

        return allFilesAsync;
    }

    public async Task<FileDescription?> GetFileDescriptionByIdAsync(int id)
    {
        var fileDbModel = await _cloudStorageUnitOfWork.FileDescription.GetByIdAsync(id);

        return _mapper.Map<FileDescription>(fileDbModel);
    }

    public async Task<IEnumerable<FileDescription>> SearchFilesAsync(FileSearchData fileSearchData)
    {
        var allFilesAsQueryable = _cloudStorageUnitOfWork.FileDescription.GetAllFilesAsQueryable(_userService.Current.Email);

        IQueryable<FileDescriptionDbModel> searchedFiles;
        
        if (string.IsNullOrEmpty(fileSearchData.Name) &&
            string.IsNullOrEmpty(fileSearchData.Extension) &&
            fileSearchData.SizeInBytes is null &&
            fileSearchData.CreationDate is null)
        {
            searchedFiles = allFilesAsQueryable;
        }
        else
        {
            searchedFiles = allFilesAsQueryable
                .Where(file => string.IsNullOrEmpty(fileSearchData.Name) || file.ProvidedName.ToLower().Contains(fileSearchData.Name.ToLower()))
                .Where(file => string.IsNullOrEmpty(fileSearchData.Extension) || fileSearchData.Extension == file.Extension)
                .Where(file => fileSearchData.SizeInBytes == null || file.SizeInBytes == fileSearchData.SizeInBytes)
                .Where(file => fileSearchData.CreationDate == null || file.CreatedDate == fileSearchData.CreationDate);
        }   

        var fileDescription = _mapper.Map<IEnumerable<FileDescription>>(await searchedFiles.ToListAsync());

        return fileDescription;
    }

    public async Task<IEnumerable<string>> GetArchiveFileNamesAsync(int fileId)
    {
        var archive = await _cloudStorageUnitOfWork.FileDescription.GetByIdAsync(fileId);

        ValidateUserPermissions(archive);

        if (archive is null)
        {
            return Enumerable.Empty<string>();
        }

        if (archive.Extension != "zip")
        {
            throw new BadHttpRequestException("Represented file is not an archive.", (int)HttpStatusCode.BadRequest);
        }
        
        var stream = await _fileStorageService.GetStreamAsync(archive.UniqueName);

        ZipArchive zipArchive = null;
        
        try
        {
            zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false, Encoding.GetEncoding(_archiveOptions.EntryNameEncoding));
        }
        catch (Exception ex)
        {
            var message = ex.Message;
        } 
        
        //var names = zipArchive.Entries.Select(n=>new Encoding)

        return zipArchive.Entries.Select(entry => entry.FullName).Where(name => name[^1] != '/');
    }

    public async Task<(string? name, string? contentType, Stream? data)> GetArchiveFileContent(int fileId, string archiveFilePath)
    {
        var archive = await _cloudStorageUnitOfWork.FileDescription.GetByIdAsync(fileId);

        ValidateUserPermissions(archive);

        if (archive is null)
        {
            return (string.Empty, string.Empty, Stream.Null);
        }
        
        if (archive.Extension != "zip" && archive.Extension != "rar")
        {
            throw new BadHttpRequestException("Represented file is not an archive.", (int)HttpStatusCode.BadRequest);
        }
        
        var stream = await _fileStorageService.GetStreamAsync(archive.UniqueName);

        var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false, Encoding.GetEncoding("cp866"));

        var entry = zipArchive.Entries.SingleOrDefault(entry => entry.FullName == archiveFilePath);

        var contentType = new FileExtensionContentTypeProvider().TryGetContentType(archiveFilePath, out var type) ? type : "image/png";
        
        return (entry.Name, contentType, entry.Open());
    }

    public async Task<FileDescription> LoadFileFromZip(int zipFileId, string archiveFileName)
    {
        var archiveDbModel = await _cloudStorageUnitOfWork.FileDescription.GetByIdAsync(zipFileId);

        ValidateUserPermissions(archiveDbModel);
        
        var uniqueName = Guid.NewGuid().ToString();
        var (data, entryName) = await _fileStorageService.ExtractZipEntry(archiveDbModel.UniqueName, uniqueName, archiveFileName);

        var contentType = new FileExtensionContentTypeProvider()
            .TryGetContentType(archiveFileName, out var type) ? type : "text/plain";
        
        var fileCreateData = new FileCreateData
        {
            Content = data,
            Name = entryName,
            ContentType = contentType
        };

        var newFileDbModel = _mapper.Map<FileDescriptionDbModel>(fileCreateData);
        newFileDbModel.UniqueName = uniqueName;
        newFileDbModel.ContentHash = _dataHasher.HashStreamData(data);
        newFileDbModel.Extension = Path.GetExtension(archiveFileName)[1..];
        newFileDbModel.UploadedBy = _userService.Current.Email;
        await _cloudStorageUnitOfWork.FileDescription.CreateAsync(newFileDbModel);
        await _cloudStorageUnitOfWork.SaveChangesAsync();
        
        var fileDescription = _mapper.Map<FileDescription>(newFileDbModel);

        return fileDescription;
    }

    public async Task RenameFileAsync(int id, string newName)
    {
        await _cloudStorageUnitOfWork.FileDescription.RenameFileAsync(id, newName);
        await _cloudStorageUnitOfWork.SaveChangesAsync();
    }

    public async Task<long> GetDiskUsageInBytes()
    {
        var fileNames = await _cloudStorageUnitOfWork.FileDescription
            .GetAllFilesAsQueryable(_userService.Current.Email)
            .Select(file => file.UniqueName)
            .ToListAsync();

        if (!fileNames.Any())
        {
            return 0;
        }
        
        var diskUsageInBytes = await _deduplicationService.GetFilesDiskUsage(fileNames);

        return diskUsageInBytes;
    }

    private async Task ValidateCreatedFile(params FileDescriptionDbModel[] fileDbModel)
    {
        var hashes = fileDbModel.Select(model => model.ContentHash);

        var contentHashesExist = await _cloudStorageUnitOfWork.FileDescription.ContentHashesExistAsync(_userService.Current.Email, hashes.ToArray());

        if (contentHashesExist)
        {
            throw new FileContentDuplicationException("A file with such content already exists.");
        }

        var fileNames = fileDbModel.Select(file => file.ProvidedName);

        var fileNamesExist = await _cloudStorageUnitOfWork.FileDescription.FileNamesExist(_userService.Current.Email, fileNames.ToArray());

        if (fileNamesExist)
        {
            throw new FileNameDuplicationException("A file with such name is already exist.");
        }
    }

    private async Task SetFilesThumbnail(params (FileDescriptionDbModel DbModel, Stream Content)[] filesInfo)
    {
        var createThumbTasks = filesInfo.Select(fileInfo =>
        {
            var (dbModel, content) = fileInfo;

            if (content.CanSeek)
            {
                content.Seek(0, SeekOrigin.Begin);
            }

            if (ContentTypeDeterminant.IsVideo(dbModel.ContentType) ||
                ContentTypeDeterminant.IsImage(dbModel.ContentType))
            {
                var thumbName = Guid.NewGuid().ToString();
                dbModel.ThumbnailInfo = new ThumbnailInfoDbModel
                {
                    UniqueName = thumbName
                };

                return _fileStorageService.CreateVideoThumbnailAsync(dbModel.UniqueName, thumbName);
            }

            return Task.CompletedTask;
        });

        await Task.WhenAll(createThumbTasks);
    }

    private void ValidateUserPermissions(params FileDescriptionDbModel[]? files)
    {
        var currentUser = _userService.Current.Email;

        if (files?.Any(file => file.UploadedBy != currentUser) == true)
        {
            throw new UnauthorizedUserException("Current user does not have access to some files.");
        }
    }
}