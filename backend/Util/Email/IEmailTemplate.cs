namespace SnoopyAirlines.Util.Email
{
    public interface IEmailTemplate<in T>
    {
        string TemplateName { get; }
        string Subject { get; }
        IReadOnlyList<EmailTemplateParameter> GetParameters(T data);
    }
}
