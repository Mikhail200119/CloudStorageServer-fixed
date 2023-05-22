using AutoMapper;
using CloudStorage.Api.Dtos.Request;
using CloudStorage.Api.Dtos.Response;
using CloudStorage.BLL.Models;

namespace CloudStorage.Api.Mapping;

public class FilesProfile : Profile
{
    public FilesProfile()
    {
        CreateMap<FileCreateRequest, FileCreateData>();
        CreateMap<FileDescription, FileGetResponse>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.ProvidedName));

        CreateMap<IFormFile, FileCreateData>()
            .ForMember(dest => dest.Content,
                opt => opt.MapFrom(src => src.OpenReadStream()))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.FileName));

        CreateMap<FileSearchRequest, FileSearchData>();
    }
}