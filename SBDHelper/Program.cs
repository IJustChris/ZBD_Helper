using Newtonsoft.Json;
using SBDHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SBDHelper
{
    class Program
    {
        static string _token = File.ReadAllText("OAuthToken.txt");

        static void Main(string[] args)
        {
            try
            {
                ApiHelper.Initialize();
                ApiHelper.SetBrearer(_token);

                var input = GetInput();
                int totalAlbums = 0;

                List<Tuple<AlbumResponseModel, IEnumerable<TrackModel>>> fullAlbumsData = new List<Tuple<AlbumResponseModel, IEnumerable<TrackModel>>>(); 
                foreach (var model in input)
                {
                    Console.WriteLine($"Obtainign data for artist with id: {model.ArtistId}");
                    var albums = ApiHelper.GetArtistAlbumsIds(model.SpotifyArtistId);
                    var currentArtistAlbums  = new List<Tuple<AlbumResponseModel, IEnumerable<TrackModel>>>();

                    foreach (var albumId in albums)
                    {
                        totalAlbums++;
                        var album = ApiHelper.GetAlbum(albumId, totalAlbums, model.ArtistId, model.Genre);

                        if (!currentArtistAlbums.Any(x => x.Item1.tytul == album.Item1.tytul))
                        {
                            currentArtistAlbums.Add(album);
                        }

                        Thread.Sleep(500);
                    }
                    
                    fullAlbumsData.AddRange(currentArtistAlbums);
                }

                string MongoObjects = MapToMongoObject(fullAlbumsData);
                File.WriteAllText("Output.txt", MongoObjects);
                Console.WriteLine("Job done");

            }
            catch (Exception e)
            {

                Console.WriteLine($"Wystapil nieoczekiwany blad: {e.Message}");
            }

            Console.WriteLine("Press Any key to exit");
            Console.ReadKey();

        }

        private static string MapToMongoObject(List<Tuple<AlbumResponseModel, IEnumerable<TrackModel>>> albums)
        {
            List<string> mongoAlbums = new List<string>();
            List<string> mongoTracks = new List<string>();

            foreach (var album in albums)
            {

                string albumStr =
$@"{{
    _id: {album.Item1._id},
    tytul: ""{album.Item1.tytul}"",
    artists_id: {album.Item1.arists_id},
    rok:""{album.Item1.rok}"",
    gatunek: ""{album.Item1.gatunek}"",
    nosnik:""{album.Item1.nosnik}""
}}";

                mongoAlbums.Add(albumStr);

                foreach (var track in album.Item2)
                {
                    StringBuilder tracksStrBuilder = new StringBuilder();

                    string trackStr =
$@"{{
    albums_id: {album.Item1._id},
    pozycja: {track.pozycja},
    tytul: ""{track.nazwa}"",
    dlugosc: {track.czas_trwania}
}}";

                    mongoTracks.Add(trackStr);
                }

            }

            string Albums = string.Join(",\n", mongoAlbums);
            string Tracks = string.Join(",\n", mongoTracks);

            return $"-- ALBUMS -- \n\n\n{Albums} \n\n\n -- TRACKS -- \n\n\n{Tracks}";
        }

        static List<InputDataModel> GetInput()
        {

            var json = File.ReadAllText("input.json");
            var x = JsonConvert.DeserializeObject<List<InputDataModel>>(json);
            return x;
        }   
    }
}
