using AutoMapper;
using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.AutoMapper
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            // Map the Track model to PlaylistTrack model
            CreateMap<Track, PlaylistTrack>()
                .ForMember(dest => dest.TrackId, source => source.MapFrom(src => src.TrackId))
                .ForMember(dest => dest.TrackName, source => source.MapFrom(src => src.Name))
                .ForMember(dest => dest.AlbumTitle, source => source.MapFrom(src => src.Album != null ? src.Album.Title : string.Empty))
                .ForMember(dest => dest.ArtistName, source => source.MapFrom(src => src.Album != null && src.Album.Artist != null ? src.Album.Artist.Name : string.Empty))
                .ForMember(dest => dest.IsFavorite, source => source.Ignore());   // IsFavorite is not set here but the value is set at AfterMap as it needs to be passed with the currentUserId
        }
    }
}
