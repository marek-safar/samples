using System;
using System.Collections.Generic;
using System.Drawing;
using System.Json;
using System.Net.Http;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;

namespace AsyncDemo
{
	public partial class DetailViewController : UITableViewController
	{	
		DataSource view;

		public DetailViewController () 
			: base (UITableViewStyle.Grouped)
		{
			TableView.Source = view = new DataSource (this);
		}

		public SongInfo Song { get; set; }

		public ArtistInfo ArtistInfo { get; private set; }

		public override async void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			TableView.ContentOffset = new PointF (0, 0);
			Title = Song.Title;

			// Populate artist details in async way. The view will appear and details will be updated
			// when server give us the data
			await FetchArtistDetails ();
		}

		async Task FetchArtistDetails ()
		{
			const string url = "http://developer.echonest.com/api/v4/artist/profile?api_key=NVBXAS6ETZE2UHCGJ&bucket=artist_location";

			var client = new HttpClient ();

			// Slow things down little bit
			await Task.Delay (1000);

			JsonValue data = null;

			// Simple retry routine for slow/flaky server
			while (true) {
				try {
					data = JsonObject.Load (await client.GetStreamAsync (url + "&id=" + Song.ArtistID));
					break;
				} catch (Exception e) {
					Console.WriteLine (e);
				}

				await Task.Delay (500);
			}

			var location = data ["response"] ["artist"] ["artist_location"];

			ArtistInfo = new ArtistInfo {
				Name = data ["response"] ["artist"] ["name"],
				City = location ["city"],
				Country = location ["country"],
				Location = location ["location"]
			};

			UpdateCell (ArtistInfo.Name, 0);

			UpdateCell (ArtistInfo.City, 1);
			UpdateCell (ArtistInfo.Country, 1, 1);

			if (ArtistInfo.Location != null)
				view.CalculateButton.Enabled = true;
		}

		public void UpdateCell (string text, int index, int cellIndex = 0, UIColor color = null)
		{
			var cell = TableView.CellAt (NSIndexPath.Create (index, cellIndex));
			cell.TextLabel.Text = text;
			cell.TextLabel.TextColor = color ?? UIColor.Black;
		}

		class DataSource : UITableViewSource
		{
			DetailViewController controller;
			CLLocationManager location_manager;
			CLLocation start_location;

			public DataSource (DetailViewController controller)
			{
				this.controller = controller;
			}

			public UIButton CalculateButton { get; private set; }

			public override int NumberOfSections (UITableView tableView)
			{
				return 3;
			}

			public override string TitleForHeader (UITableView tableView, int section)
			{
				switch (section) {
				case 0:
					return "Artist";
				case 1:
					return "Location";
				case 2:
					return "Distance";
				}

				throw new NotImplementedException ();
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				if (section == 0)
					return 1;

				return 2;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				string cellIdentifier = "Cell";
				var cell = tableView.DequeueReusableCell (cellIdentifier);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
					cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				}

				var info = controller.ArtistInfo;

				if (info != null)
					return cell;

				cell.TextLabel.TextColor = UIColor.LightGray;

				if (indexPath.Section == 2) {
					if (indexPath.Row == 1) {
						CalculateButton = new UIButton (UIButtonType.RoundedRect) {
							Frame = new RectangleF (80, 5, 140, 35),
							Enabled = false
						};

						CalculateButton.SetTitle ("Start Tracking", UIControlState.Normal);
						CalculateButton.TouchDown += HandleTouchDown;

						cell.ContentView.AddSubview (CalculateButton);
					} else {
						cell.TextLabel.Text = "Unknown";
					}
				} else {
					cell.TextLabel.Text = "Retrieving...";
				}

				return cell;
			}

			async void HandleTouchDown (object sender, EventArgs e)
			{
				if (location_manager != null) {
					location_manager.StopUpdatingLocation ();
					location_manager.LocationsUpdated -= HandleLocationsUpdated;
					location_manager = null;
					CalculateButton.SetTitle ("Start Tracking", UIControlState.Normal);

					controller.UpdateCell ("Unknown", 2, color: UIColor.LightGray);
					return;
				}

				// Disable the button because the handler can take a while
				CalculateButton.Enabled = false;

				try {
					var geocoder = new CLGeocoder ();

					// Convert textual artist location to longitude/latitude coordinates
					var art_loc = await geocoder.GeocodeAddressAsync (controller.ArtistInfo.Location);
					if (art_loc == null)
						return;

					start_location = art_loc [0].Location;

					location_manager = new CLLocationManager ();
					location_manager.DesiredAccuracy = CLLocation.AccuracyBest;
					CalculateButton.SetTitle ("Stop Tracking", UIControlState.Normal);
					location_manager.LocationsUpdated += HandleLocationsUpdated;

					if (CLLocationManager.LocationServicesEnabled)
						location_manager.StartUpdatingLocation ();
				} catch {
					// TODO: Handle exceptions
				} finally {
					// Re-enable the button when everything is ready
					CalculateButton.Enabled = true;
				}
			}

			void HandleLocationsUpdated (object sender, CLLocationsUpdatedEventArgs e)
			{
				var location = e.Locations [0];
				var dist = HaversineInKM (start_location, location);
				controller.UpdateCell (string.Format ("{0:f3} km", dist), 2);
			}

			static double HaversineInKM (CLLocation from, CLLocation to)
			{
				const double d2r = Math.PI / 180;
				const double eQuatorialEarthRadius = 6378.1370;

				double dlong = (to.Coordinate.Longitude - from.Coordinate.Longitude) * d2r;

				var lat2 = to.Coordinate.Latitude;
				var lat1 = from.Coordinate.Latitude;

				double dlat = (lat2 - lat1) * d2r;
				double a = Math.Pow (Math.Sin (dlat / 2), 2) + Math.Cos (lat1 * d2r) * Math.Cos (lat2 * d2r) * Math.Pow (Math.Sin (dlong / 2), 2);
				double c = 2 * Math.Atan2 (Math.Sqrt (a), Math.Sqrt (1 - a));
				double d = eQuatorialEarthRadius * c;

				return d;
			}
		}
	}
}
