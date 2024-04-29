namespace Chinook.Services.Interfaces
{
    public interface IPlaylistService
    {
        public Task<ClientModels.Playlist> GetPlaylist(long playlistId, string currentUserId);
        public Task<bool> AddTrackToFavoritePlaylist(long trackId, string currentUserId);
        public Task<bool> RemoveTrackFromMyFavoritePlaylist(long trackId, string currentUserId);
        public Task<long> GetCurrentUserFavoritePlaylistId(string currentUserId);

    }
}
