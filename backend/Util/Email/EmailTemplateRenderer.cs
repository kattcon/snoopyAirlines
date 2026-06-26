using System.Reflection;
using System.Text;

namespace SnoopyAirlines.Util.Email
{
    internal static class EmailTemplateRenderer
    {
        public static RenderedEmailTemplate Render<T>(IEmailTemplate<T> template, T data)
        {
            var parameters = template.GetParameters(data);
            var body = LoadTemplate(template.TemplateName);
            var parameterValues = BuildParameterDictionary(parameters);

            return new RenderedEmailTemplate(
                ReplaceParameters(template.Subject, parameterValues),
                ReplaceParameters(body, parameterValues));
        }

        private static Dictionary<string, string> BuildParameterDictionary(
            IReadOnlyList<EmailTemplateParameter> parameters)
        {
            var parameterValues = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var parameter in parameters)
            {
                parameterValues.TryAdd(parameter.Name, parameter.Value);
            }

            return parameterValues;
        }

        private static string ReplaceParameters(
            string body,
            IReadOnlyDictionary<string, string> parameterValues)
        {
            if (parameterValues.Count == 0)
            {
                return body;
            }

            const string ParameterStart = "{{";
            const string ParameterEnd = "}}";

            var result = new StringBuilder(body.Length);
            var currentIndex = 0;

            while (currentIndex < body.Length)
            {
                var parameterStart = body.IndexOf(
                    ParameterStart,
                    currentIndex,
                    StringComparison.Ordinal);

                if (parameterStart < 0)
                {
                    result.Append(body, currentIndex, body.Length - currentIndex);
                    break;
                }

                var parameterEnd = body.IndexOf(
                    ParameterEnd,
                    parameterStart + ParameterStart.Length,
                    StringComparison.Ordinal);

                if (parameterEnd < 0)
                {
                    result.Append(body, currentIndex, body.Length - currentIndex);
                    break;
                }

                result.Append(body, currentIndex, parameterStart - currentIndex);

                var parameterNameStart = parameterStart + ParameterStart.Length;
                var parameterNameLength = parameterEnd - parameterNameStart;
                var parameterName = body.Substring(parameterNameStart, parameterNameLength);

                if (parameterValues.TryGetValue(parameterName, out var parameterValue))
                {
                    result.Append(parameterValue);
                }
                else
                {
                    var parameterLength = parameterEnd + ParameterEnd.Length - parameterStart;
                    result.Append(body, parameterStart, parameterLength);
                }

                currentIndex = parameterEnd + ParameterEnd.Length;
            }

            return result.ToString();
        }

        private static string LoadTemplate(string templateName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(templateName)
                ?? throw new InvalidOperationException(
                    $"Email template not found as embedded resource: '{templateName}'. " +
                    $"Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
