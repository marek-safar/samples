using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace AsyncDemo
{
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

			var entry = controller.Data [indexPath.Item];
			cell.TextLabel.Text = entry.Title;

			cell.ImageView.Frame = new RectangleF (0, 0, 50, 50);
			cell.ImageView.ContentMode = UIViewContentMode.ScaleToFill;

			if (entry.ImageData != null) {
				cell.ImageView.Image = entry.ImageData;

			}

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

