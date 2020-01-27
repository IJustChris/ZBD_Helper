using Newtonsoft.Json;
using SBDHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace SBDHelper
{
    public static class ApiHelper
    {
        public static HttpClient Client { get; set; }
        public static void Initialize()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static void SetBrearer(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static Tuple<AlbumResponseModel,IEnumerable<TrackModel>> GetAlbum(string albumId, int outputId, int artistId, string genre)
        {
            string url = $"https://api.spotify.com/v1/albums/{albumId}?market=ES";

            var result = Client.GetAsync(url).GetAwaiter().GetResult();

            if (result.IsSuccessStatusCode)
            {
                var response = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return DeserializeResponse(response, outputId, artistId, genre);
            }

            throw new Exception("Request failed");
        }

        public static IEnumerable<string> GetArtistAlbumsIds(string artistId)
        {
            string url = $"https://api.spotify.com/v1/artists/{artistId}/albums";

            var result = Client.GetAsync(url).GetAwaiter().GetResult();

            if (result.IsSuccessStatusCode)
            {
                var response = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return DeserializeAndExtractAlbumsIds(response);
            }

            throw new Exception("Request failed");
        }

        private static IEnumerable<string> DeserializeAndExtractAlbumsIds(string json)
        {
            List<string> albumsIds = new List<string>();
            dynamic obj = Json.Decode(json);
            foreach (var item in obj.items)
            {
                if (item.album_type == "album" && item.album_group == "album" && item.type == "album" && item.total_tracks > 5)
                {
                    string uri = item.uri;
                    string albumid = uri.Split(':')[2];
                    albumsIds.Add(albumid);
                }
            }

            return albumsIds;
        }

        private static Tuple<AlbumResponseModel, IEnumerable<TrackModel>> DeserializeResponse(string json, int outputId, int artistId, string genre)
        {
            dynamic obj = Json.Decode(json);

            string Artist = obj.artists[0].name;
            string albumName = obj.name;
            string release = obj.release_date;
            int releaseYear = int.Parse(release.Split('-')[0]);

            AlbumResponseModel album = new AlbumResponseModel 
            { 
                tytul = albumName, 
                rok = releaseYear, 
                _id = outputId, 
                arists_id = artistId,
                gatunek = genre
            };

            List<TrackModel> tracks = new List<TrackModel>();

            foreach (var track in obj.tracks.items)
            {
                var model = new TrackModel
                {
                    czas_trwania = track.duration_ms,
                    pozycja = track.track_number,
                    nazwa = track.name,
                    albums_id = album._id
                };

                tracks.Add(model);
            }

            return new Tuple<AlbumResponseModel, IEnumerable<TrackModel>>(album, tracks);
        }


    }
}
