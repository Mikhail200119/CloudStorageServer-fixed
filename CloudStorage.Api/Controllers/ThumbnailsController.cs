using CloudStorage.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.Api.Controllers;

[ApiController]
[Route("api/thumbnails")]
public class ThumbnailsController : ControllerBase
{
    private readonly ICloudStorageManager _cloudStorageManager;

    public ThumbnailsController(ICloudStorageManager cloudStorageManager)
    {
        _cloudStorageManager = cloudStorageManager;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Stream>> GetThumbnail(int id)
    {
        var (data, contentType) = await _cloudStorageManager.GetThumbnailStreamAndContentTypeAsync(id);

        return new FileStreamResult(data, contentType);
    }
}