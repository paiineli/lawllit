namespace Lawllit.Web.Areas.Finance.Models
{
    public sealed record ErrorViewModel(string? RequestId)
    {
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
