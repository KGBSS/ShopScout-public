namespace ShopScout.Data.EmailTemplates;

public static class EmailTemplates
{
    public static string GetPasswordReset(string resetLink)
    {
        var template = LoadTemplate("ForgotPassword.html");
        return template.Replace("{{RESET_LINK}}", resetLink);
    }

    public static string GetWelcomeWithEmailConfirmation(string confirmationLink)
    {
        string template = LoadTemplate("WelcomeWithEmailConfirmation.html");
        return template.Replace("{{CONFIRMATION_LINK}}", confirmationLink);
    }

    public static string GetWelcomeWithoutEmailConfirmation()
    {
        return LoadTemplate("WelcomeWithoutEmailConfirmation.html");
    }

    public static string GetEmailConfirmation(string confirmationLink)
    {
        var template = LoadTemplate("EmailConfirmation.html");
        return template.Replace("{{CONFIRMATION_LINK}}", confirmationLink);
    }

    private static string LoadTemplate(string fileName)
    {
        var assembly = typeof(EmailTemplates).Assembly;
        var resourceName = $"ShopScout.Data.EmailTemplates.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Email template '{fileName}' not found");

        using var reader = new StreamReader(stream);
        return MinifyHtml(reader.ReadToEnd());
    }

    private static string MinifyHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;

        // Remove comments
        html = System.Text.RegularExpressions.Regex.Replace(html, @"<!--.*?-->", "", System.Text.RegularExpressions.RegexOptions.Singleline);

        // Remove whitespace between tags
        html = System.Text.RegularExpressions.Regex.Replace(html, @">\s+<", "><");

        // Remove leading/trailing whitespace on each line
        html = System.Text.RegularExpressions.Regex.Replace(html, @"^\s+|\s+$", "", System.Text.RegularExpressions.RegexOptions.Multiline);

        // Replace multiple spaces with single space
        html = System.Text.RegularExpressions.Regex.Replace(html, @"\s{2,}", " ");

        return html.Trim();
    }
}