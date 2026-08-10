using System.Text;

namespace CarRent.Infrastructure.Services.Crm;

public static class TemplateRenderer
{
    public static string Render(string template, IReadOnlyDictionary<string, string> values)
    {
        var result = new StringBuilder(template);
        foreach (var (key, value) in values)
        {
            result.Replace($"{{{{{key}}}}}", value);
        }
        return result.ToString();
    }
}
