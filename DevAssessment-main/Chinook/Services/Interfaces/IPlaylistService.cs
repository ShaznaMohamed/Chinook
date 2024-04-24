namespace Chinook.Services.Interfaces
{
    public interface IPlaylistService
    {
        public Task<ClientModels.Playlist> GetPlaylist(long playlistId, string currentUserId);
    }
}
