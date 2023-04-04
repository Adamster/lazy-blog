using SlugGenerator;

namespace Lazy.Domain.Extensions;

public static class StringExtensions
{
    /// <summary>  
    /// Turn a string into a slug by encoding string to url acceptable format
    /// </summary>  
    /// <param name="phrase">The string to turn into a slug.</param>  
    /// <returns></returns>  
    public static string Slugify(this string phrase)
    {
        // Remove all accents and make the string lower case.  
        string output = phrase.GenerateSlug();

        return output;
    }
}