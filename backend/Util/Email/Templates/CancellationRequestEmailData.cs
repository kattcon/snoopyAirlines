namespace SnoopyAirlines.Util.Email.Templates
{
    public sealed record CancellationRequestEmailData(
        string ConfirmationCode,
        string CancelUrl);
}