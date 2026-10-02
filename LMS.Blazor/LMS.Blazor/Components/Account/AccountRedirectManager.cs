using Microsoft.AspNetCore.Components;

namespace LMS.Blazor.Components.Account;

internal sealed class AccountRedirectManager(NavigationManager navigationManager)
{
    public void RedirectTo(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            navigationManager.NavigateTo("");
            return;
        }

        if (Uri.IsWellFormedUriString(uri, UriKind.Relative) &&
            !uri.StartsWith("//", StringComparison.Ordinal) &&
            !uri.StartsWith('\\'))
        {
            navigationManager.NavigateTo(uri);
            return;
        }

        var baseUri = new Uri(navigationManager.BaseUri);

        if (Uri.TryCreate(uri, UriKind.Absolute, out var absoluteUri) &&
            baseUri.IsBaseOf(absoluteUri))
        {
            navigationManager.NavigateTo(
                navigationManager.ToBaseRelativePath(absoluteUri.ToString()));
            return;
        }

        navigationManager.NavigateTo("");
    }
}
