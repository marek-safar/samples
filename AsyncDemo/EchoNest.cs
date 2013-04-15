using System;

namespace AsyncDemo
{
	public static class EchoNest
	{
		public const string TrendingSongs = "http://developer.echonest.com/api/v4/song/search?api_key=NVBXAS6ETZE2UHCGJ&sort=song_hotttnesss-desc&bucket=song_hotttnesss&results=50";
		public const string ArtistImages = "http://developer.echonest.com/api/v4/artist/images?api_key=NVBXAS6ETZE2UHCGJ&format=json&results=3&start=0";
		public const string ArtistDetails = "http://developer.echonest.com/api/v4/artist/profile?api_key=NVBXAS6ETZE2UHCGJ&bucket=artist_location";
	}
}

