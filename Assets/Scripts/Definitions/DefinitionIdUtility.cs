using System.Text;

public static class DefinitionIdUtility
{
    public static string FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        StringBuilder builder = new();

        foreach (char character in name)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(
                    char.ToLowerInvariant(character)
                );
            }
            else if (builder.Length > 0 &&
                     builder[builder.Length - 1] != '_')
            {
                builder.Append('_');
            }
        }

        return builder.ToString().Trim('_');
    }
}
