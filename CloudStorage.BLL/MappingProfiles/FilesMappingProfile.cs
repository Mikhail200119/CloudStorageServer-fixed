using AutoMapper;
using CloudStorage.BLL.Models;
using CloudStorage.DAL.Entities;

namespace CloudStorage.BLL.MappingProfiles;

public class FilesMappingProfile : Profile
{
    public FilesMappingProfile()
    {
        CreateMap<FileCreateData, FileDescriptionDbModel>()
            .ForMember(dest => dest.ProvidedName,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.UniqueName,
                opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate,
                opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.SizeInBytes,
                opt => opt.MapFrom(src => src.Content.Length));

        CreateMap<FileUpdateData, FileDescriptionDbModel>();
        CreateMap<FileDescriptionDbModel, FileDescription>()
            .ForMember(dest => dest.ThumbnailId,
                opt => opt.MapFrom(src => src.ThumbnailInfo!.Id));
    }
}