using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace SpotifyHelper
{
    internal static class Program
    {
        static async Task Main(string[] _)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Get token (e.g. at https://developer.spotify.com/console/get-playlist/) and type it:");
            var token = Console.ReadLine();

            try
            {
                var spotify = new SpotifyClient(token);
                Console.WriteLine("Getting user profile...");
                var currentUser = await spotify.UserProfile.Current();
                Console.WriteLine($"Logged in as {currentUser.DisplayName}.");

                Console.WriteLine("Get playlist Id (Open spotify -> playlist options -> Share -> Copy Spotify URI -> paste it in console):");
                var playlistId = Console.ReadLine();
                playlistId = playlistId.StartsWith("spotify:playlist:") ? playlistId.Replace("spotify:playlist:", string.Empty) : playlistId;

                Console.WriteLine("Getting playlist...");
                var playlist = await spotify.Playlists.Get(playlistId);

                Console.WriteLine($"Trying to find duplicates at '{playlist.Name}'...");

                var allPages = await spotify.PaginateAll(playlist.Tracks);
                var tracks = allPages.Select((x, index) =>
                {
                    var track = (FullTrack) x.Track;
                    return new
                    {
                        Artist = string.Join(", ", track.Artists.Select(artist => artist.Name)),
                        track.Name,
                        Index = index
                    };
                }).ToArray();

                var duplicates =
                    (from track in tracks
                        let name = $"{track.Artist} - {track.Name}"
                        let otherNames = tracks.Where(x => x.Index != track.Index).Select(x => $"{x.Artist} - {x.Name}").ToArray()
                        let similarNames = otherNames.Where(x => x.Contains(name) || x.StartsWith(name)).ToArray()
                        where similarNames.Any()
                        select $"{name}\r\n{string.Join("\r\n", similarNames)}").ToList();

                Console.WriteLine(duplicates.Any() ? string.Join(Environment.NewLine, duplicates) : "No duplicates found." );

                Console.ReadKey();
            }
            catch (APIException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}