using System.Text;
using AutoMapper;
using CloudStorage.Api.Dtos.Request;
using CloudStorage.Api.Dtos.Response;
using CloudStorage.Api.Filters;
using CloudStorage.Api.Helpers;
using CloudStorage.Api.Services;
using CloudStorage.BLL.Models;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CloudStorage.Api.Controllers;

[ApiController]
[Route("api/files/")]
[Authorize]
public class FilesController : ControllerBase
{
    private const long MaxFileSize = 10_000_000_000;

    private readonly ICloudStorageManager _cloudStorageManager;
    private readonly IDisplayContentTypeMapper _contentTypeMapper;
    private readonly IMapper _mapper;
    private readonly IWordToPdfConverter _wordToPdfConverter;

    public FilesController(ICloudStorageManager cloudStorageManager, IMapper mapper, IDisplayContentTypeMapper contentTypeMapper, IWordToPdfConverter wordToPdfConverter)
    {
        _cloudStorageManager = cloudStorageManager;
        _mapper = mapper;
        _contentTypeMapper = contentTypeMapper;
        _wordToPdfConverter = wordToPdfConverter;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileGetResponse>>> GetAllFiles()
    {
        var fileDescriptions = await _cloudStorageManager.GetAllFilesAsync();
        var fileGetResponses = _mapper.Map<IEnumerable<FileGetResponse>>(fileDescriptions);

        SetContentUrl(fileGetResponses);

        return Ok(fileGetResponses);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<FileGetResponse>>> SearchFiles([FromQuery] FileSearchRequest fileSearchRequest)
    {
        var fileSearchData = _mapper.Map<FileSearchData>(fileSearchRequest);
        var fileDescriptions = await _cloudStorageManager.SearchFilesAsync(fileSearchData);
        var fileGetResponses = _mapper.Map<IEnumerable<FileGetResponse>>(fileDescriptions);

        SetContentUrl(fileGetResponses);

        return Ok(fileGetResponses);
    }

    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    [RequestSizeLimit(MaxFileSize)]
    [DisableFormValueModelBinding]
    public async Task<ActionResult<IEnumerable<FileGetResponse>>> UploadFiles([FromForm] IEnumerable<IFormFile> files)
    {
        var fileCreateDatas = _mapper.Map<IEnumerable<FileCreateData>>(files);
        var fileDescriptions = await _cloudStorageManager.CreateAsync(fileCreateDatas);
        var fileResponses = _mapper.Map<IEnumerable<FileGetResponse>>(fileDescriptions);

        SetContentUrl(fileResponses);

        return Ok(fileResponses);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] IEnumerable<int> ids)
    {
        await _cloudStorageManager.DeleteRangeAsync(ids);

        return Ok();
    }

    [HttpGet("archive/file-list/{fileId}")]
    public async Task<ActionResult<IEnumerable<string>>> GetArchiveFileNames(int fileId)
    {
        var names = await _cloudStorageManager.GetArchiveFileNamesAsync(fileId);
        
        return Ok(names);
    }

    [HttpGet("archive/unzip-file/{fileId}/{archiveFilePath}")]
    public async Task<ActionResult<Stream>> UnzipArchiveFile(int fileId, string archiveFilePath)
    {
        var decodedPath = archiveFilePath.Replace("%2F", "/");

        var extension = Path.GetExtension(archiveFilePath)[1..];
        var name = Path.GetFileName(archiveFilePath);
        
        var (_, contentType, data) = await _cloudStorageManager.GetArchiveFileContent(fileId, decodedPath);

        Stream fileStream;
        
        if (extension is "doc" or "docx")
        {
            contentType = "application/pdf";
            fileStream = await _wordToPdfConverter.GetPdfFromWordAsync(data, name, extension);
        }
        else
        {
            fileStream = data;
        }
        
        return new FileStreamResult(fileStream ?? Stream.Null, $"{contentType}; charset=utf-8");
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Stream>> GetFileContent([FromRoute] int id)
    {
        var fileDescription = await _cloudStorageManager.GetFileDescriptionByIdAsync(id);
        
        //var (stream, contentType, downloadName) = await _cloudStorageManager.GetFileStreamAsync(id);

        var extensions = new FileExtensionContentTypeProvider().Mappings
            .Where(m => m.Value == fileDescription.ContentType)
            .Select(m => m.Key);

        Stream stream;
        string contentType;
        
        if (extensions.Contains(".doc") || extensions.Contains(".docx"))
        {
            stream = await _wordToPdfConverter.GetPdfFromWordAsync(id);
            Response.Headers.ContentType = contentType = "application/pdf";
        }
        else
        {
            var (data, type, _) = await _cloudStorageManager.GetFileStreamAsync(id);
            stream = data;
            Response.Headers.ContentType = contentType = type;
        }

        var base64FileName = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileDescription.ProvidedName));
        Response.Headers.ContentDisposition = $"inline; filename={base64FileName}";

        return new FileStreamResult(stream, $"{contentType}; charset=utf-8");
    }

    [HttpGet("is-displayable-file")]
    public bool IsDisplayableFile([FromQuery] string contentType)
    {
        return DisplayableContentType.IsDisplayable(contentType);
    }

    [HttpGet("is-displayable-archive-file")]
    public bool IsArchiveFileDisplayable([FromQuery] string archiveFilePath)
    {
        var extension = Path.GetExtension(archiveFilePath);
        var contentType = new FileExtensionContentTypeProvider()
            .TryGetContentType(extension, out var type)
            ? type
            : string.Empty;

        return contentType != string.Empty && DisplayableContentType.IsDisplayable(contentType);
    }

    [HttpGet("download/{fileId:int}")]
    public async Task<ActionResult<Stream?>> DownloadFile(int fileId)
    {
        var (data, contentType, downloadName) = await _cloudStorageManager.GetFileStreamAsync(fileId);

        return File(data, contentType, downloadName);
    }

    [HttpGet("download/archive-file/{fileId:int}/{archiveFilePath}")]
    public async Task<ActionResult<Stream?>> DownloadArchiveFile(int fileId, string archiveFilePath)
    {
        var decodedPath = archiveFilePath.Replace("%2F", "/");

        var (name, contentType, data) = await _cloudStorageManager.GetArchiveFileContent(fileId, decodedPath);

        return File(data, contentType, name);
    }

    [HttpPost("archive/upload-file/{archiveId:int}/{archiveFilePath}")]
    public async Task<ActionResult<FileGetResponse>> UploadArchiveFile(int archiveId, string archiveFilePath)
    {
        var decodedPath = archiveFilePath.Replace("%2F", "/");
        var fileDescription = await _cloudStorageManager.LoadFileFromZip(archiveId, decodedPath);

        var response = _mapper.Map<FileGetResponse>(fileDescription);
        SetContentUrl(new List<FileGetResponse> { response });

        return Ok(response);
    }

    [HttpGet("disk-usage")]
    public async Task<long> GetDiskUsage()
    {
        return await _cloudStorageManager.GetDiskUsageInBytes();
    }
    
    private void SetContentUrl(IEnumerable<FileGetResponse> fileGetResponses)
    {
        foreach (var file in fileGetResponses)
        {
            file.FileSrc = Url.Action("GetFileContent", "Files", new { id = file.Id })!;
            file.ThumbnailSrc = Url.Action("GetThumbnail", "Thumbnails", new { id = file.ThumbnailId })!;
        }
    }
}