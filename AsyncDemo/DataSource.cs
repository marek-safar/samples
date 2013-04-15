using MonoTouch.UIKit;
using System.Drawing;


using System;


using MonoTouch.Foundation;

namespace AsyncDemo
{
	class DataSource : UITableViewSource
	{
		DetailViewController controller;

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
					CalculateButton.TouchDown += controller.HandleTouchDown;

					cell.ContentView.AddSubview (CalculateButton);
				} else {
					cell.TextLabel.Text = "Unknown";
				}
			} else {
				cell.TextLabel.Text = "Retrieving...";
			}

			return cell;
		}
	}
}
