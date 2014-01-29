namespace Escc.AZ
{
	/// <summary>
	/// Enumerated value specify whether we're editing or displaying the A-Z
	/// </summary>
	public enum AZMode
	{
		/// <summary>
		/// Currently viewing the A-Z as the public would see it
		/// </summary>
		Published,

		/// <summary>
		/// Currently editing the A-Z
		/// </summary>
		Edit,

		/// <summary>
		/// Currently editing District or Borough information in the A-Z
		/// </summary>
		EditRestricted
	}
}
