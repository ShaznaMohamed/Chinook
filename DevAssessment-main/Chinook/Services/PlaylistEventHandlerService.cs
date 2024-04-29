using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Chinook.Services
{
    public class PlaylistEventHandlerService
    {
        // Behavior subject is used this behavoir needs to be identified at the initial app load as well
        private BehaviorSubject<bool> favoritePlaylist = new BehaviorSubject<bool>(false);

        public void PublishFavoritePlaylist(bool isFavorite)
        {
            favoritePlaylist.OnNext(isFavorite);
        }

        public IObservable<bool> SubscribeFavoritePlaylist()
        {
            return favoritePlaylist.AsObservable();
        }
    }
}
