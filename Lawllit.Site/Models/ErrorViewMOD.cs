namespace Lawllit.Site.Models;

public sealed record ErrorViewMOD(string? RequestId)
{
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
