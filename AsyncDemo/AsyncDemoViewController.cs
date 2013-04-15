using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace AsyncDemo
{
	public partial class AsyncDemoViewController : UITableViewController
	{
		public AsyncDemoViewController () : base ("AsyncDemoViewController", null)
		{
			Title = "Loading...";

			Data = new List<SongInfo> ();
			TableView.Source = new RootDataSource (this);
		}

		public List<SongInfo> Data { get; private set; }

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Fetch some trending songs
			await FetchSongsAsync (300);

			Title = string.Format ("{0} Trending Songs", Data.Count);

			// Fetch images of trending songs artists
			await FetchImages ();
		}

		async Task FetchSongsAsync (int maxCount)
		{
			var distinct_data = new HashSet<SongInfo> ();

			HttpClient client = new HttpClient ();

			int start = 0;
			while (Data.Count < maxCount) {
				JsonValue data;
				try {
					//
					// Get a stream with data bucket using http client async API
					//
					var stream = await client.GetStreamAsync (EchoNest.TrendingSongs + "&start=" + start);

					//
					// JSonObject does not have async API yet but we can easily simulate it
					// by using Task.Run
					//
					data = await Task.Run (() => JsonObject.Load (stream));
				} catch {
					return;
				}

				var songs = data ["response"] ["songs"];

				foreach (JsonObject song in songs) {
					var song_info = new SongInfo () {
						Title = song ["title"],
						ArtistID = song ["artist_id"]
					};

					if (distinct_data.Contains (song_info))
						continue;

					distinct_data.Add (song_info);

					Data.Add (song_info);
				}

				Title =  string.Format ("Fetched {0} songs", Data.Count);
				TableView.ReloadData ();

				start += 50;
			}
		}

		async Task FetchImages ()
		{
			foreach (var entry in Data) {
				await FetchArtistImage (entry);
				TableView.ReloadData ();
			}
		}
/*
		async Task FetchImagesFaster ()
		{
			foreach (var slice in Data.Partition (5)) {
				var tasks = from d in slice select FetchArtistImage (d);
				await Task.WhenAll (tasks);
				TableView.ReloadData ();
			}
		}
*/
		async Task FetchArtistImage (SongInfo song)
		{
			HttpClient client = new HttpClient ();
			Stream stream;
			try {
				stream = await client.GetStreamAsync (EchoNest.ArtistImages + "&id=" + song.ArtistID);
			} catch {
				return;
			}

			var data = await Task.Run (() => JsonObject.Load (stream));
			var images = data ["response"] ["images"];
			foreach (JsonObject image in images) {
				var img_url = image ["url"];
				try {
					var img_data = await Task.Run (() => NSData.FromUrl (new NSUrl (img_url)));
					var img = new UIImage (img_data);
					song.ImageData = img.Scale (new SizeF (150, 150));
					return;
				} catch {
				}
			}
		}
	}
}

