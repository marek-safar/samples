using System;
using System.Collections.Generic;
using System.Drawing;
using System.Json;
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

			await FetchSongsAsync (500);

			Title = string.Format ("{0} Trending Songs", Data.Count);
		}

		async Task FetchSongsAsync (int maxCount)
		{
			const string url = "http://developer.echonest.com/api/v4/song/search?api_key=NVBXAS6ETZE2UHCGJ&sort=song_hotttnesss-desc&bucket=song_hotttnesss&results=50";

			var distinct_data = new HashSet<SongInfo> ();

			HttpClient client = new HttpClient ();

			int start = 0;
			while (Data.Count < maxCount) {
				JsonValue data;
				try {
					data = JsonObject.Load (await client.GetStreamAsync (url + "&start=" + start));
				} catch (Exception e) {
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

		class RootDataSource : UITableViewSource
		{
			AsyncDemoViewController controller;

			public RootDataSource (AsyncDemoViewController controller)
			{
				this.controller = controller;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return controller.Data.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				string cellIdentifier = "Cell";
				var cell = tableView.DequeueReusableCell (cellIdentifier);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				}

				cell.TextLabel.Text = controller.Data [indexPath.Item].Title;
				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				// Make the DetailView the active View.
				controller.NavigationController.PushViewController (new DetailViewController () {
					Song = controller.Data [indexPath.Item]
				}, true);
			}
		}
	}
}

