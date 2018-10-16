namespace SerilogPlay.Models
{
	public class ErrorViewModel
	{
		public string RouteOfException { get; set; }

		public string ErrorMessage { get; set; }

		public string ErrorSource { get; set; }

		public string ErrorTargetSiteName { get; set; }

		public string ErrorStackTrace { get; set; }

		public string OriginalPathBase { get; set; }

		public string OriginalQueryString { get; set; }

		public string RequestId { get; set; }

		public bool ShowRouteOfException => !string.IsNullOrWhiteSpace(RouteOfException);

		public bool ShowErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

		public bool ShowErrorSource => !string.IsNullOrWhiteSpace(ErrorSource);

		public bool ShowErrorTargetSiteName => !string.IsNullOrWhiteSpace(ErrorTargetSiteName);

		public bool ShowErrorStackTrace => !string.IsNullOrWhiteSpace(ErrorStackTrace);

		public bool ShowOriginalPathBase => !string.IsNullOrWhiteSpace(OriginalPathBase);

		public bool ShowOriginalQueryString => !string.IsNullOrWhiteSpace(OriginalQueryString);

		public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
	}
}