using System;

namespace Escc.AZ
{
	/// <summary>
	/// A url with a title and description
	/// </summary>
	public class AZUrl
	{
		private Uri url;
		private string text = "";
		private string description = "";
		private int id;
		
		/// <summary>
		/// Gets or sets the unique database id of the URL
		/// </summary>
		public int Id
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}
		
		/// <summary>
		/// The description of the link
		/// </summary>
		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}
		
		/// <summary>
		/// The clickable text of the link
		/// </summary>
		public string Text
		{
			get
			{
				return this.text;
			}
			set
			{
				this.text = value;
			}
		}
		
		/// <summary>
		/// The URI to link to
		/// </summary>
		public Uri Url
		{
			get
			{
				return this.url;
			}
			set
			{
				this.url = value;
			}
		}

		/// <summary>
		/// Creates a a url with a title and description
		/// </summary>
		public AZUrl()
		{
		}
	}
}
