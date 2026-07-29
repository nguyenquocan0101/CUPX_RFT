using System.Reflection;

namespace Services.Email;

public class EmailTemplateHandler
{
    private readonly string _templatesPath;

    public EmailTemplateHandler()
    {
        var projectPath = Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;
        var templateProject = Assembly.GetExecutingAssembly().GetName().Name;

        //_templatesPath = Path.Combine(projectPath, templateProject, "Email/Templates");
        _templatesPath = Path.Combine(AppContext.BaseDirectory, "Email/Templates");
    }

    public async Task<string> GetTemplateAsync(string templateName)
    {
        using var reader = new StreamReader(Path.Combine(_templatesPath, templateName));

        return await reader.ReadToEndAsync();
    }

    public string ReplaceInTemplate(string input, IDictionary<string, string> replaceWords)
    {
        var response = input;

        foreach (var temp in replaceWords)
        {
            response = response.Replace(temp.Key, temp.Value);
        }

        return response;
    }
}