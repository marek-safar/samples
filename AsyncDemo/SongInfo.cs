using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace AsyncDemo
{
	public class SongInfo
	{
		public string Title { get; set; }
		public string ArtistID { get; set; }
		public UIImage ImageData { get; set; }

		public override int GetHashCode ()
		{
			return Title.ToLowerInvariant ().GetHashCode () ^ ArtistID.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			var other = obj as SongInfo;
			return other != null &&
				string.Equals (other.Title, Title, StringComparison.OrdinalIgnoreCase) &&
					other.ArtistID == ArtistID;
		}
	}

	public class ArtistInfo
	{
		public string Name { get; set; }
		public string City { get; set; }
		public string Country { get; set; }

		public string Location { get; set; }
	}
}

