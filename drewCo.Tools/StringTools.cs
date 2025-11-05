//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2024 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using SysMath = System.Math;
using System.Diagnostics.SymbolStore;


#if !NETFX_CORE
using System.Security.Cryptography;
#else

using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;

#endif

namespace drewCo.Tools
{
  // ============================================================================================================================
  public partial class StringTools
  {
    public const string ELLIPSIS = "...";

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get the first word from the given input.
    /// </summary>
    public static string GetFirstWord(string input)
    {
      if (input == null) { throw new ArgumentNullException(nameof(input)); }

      string res = input;
      int firstSpace = input.IndexOf(' ');
      if (firstSpace != -1)
      {
        res = input.Substring(0, firstSpace);
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// An easy, non-stupid way to reverse a string.
    /// </summary>
    /// <remarks>
    /// Probably doesn't work well with stuff that isn't UTF-8 / ASCII.
    /// </remarks>
    public static string Reverse(string s)
    {
      // Thanks internet!
      //https://stackoverflow.com/questions/228038/best-way-to-reverse-a-string
      // Looks expensive!
      char[] charArray = s.ToCharArray();
      Array.Reverse(charArray);
      return new string(charArray);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Convert the first character of the input string to lowercase.
    /// </summary>
    public static string LowerFirst(string input)
    {
      if (input.Length == 0) { return input; }

      uint val = (uint)input[0];
      if (val >= 65 & val <= 90)
      {
        val += 32;
      }

      // Too bad we can't just set the stupid character.  Might be useful to do so in an
      // unsafe context tho!
      string res = (char)val + input.Substring(1);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Remove all newline characters from the given input string.
    /// Newlines can optionaly be replaced with space characters via <paramref name="replaceWithSpace"/>.
    /// </summary>
    public static string StripNewlines(string input, bool replaceWithSpace = false)
    {
      string replaceWith = replaceWithSpace ? " " : string.Empty;;

      string res = input.Replace("\r\n", replaceWith).Replace("\r", replaceWith).Replace("\n", replaceWith);
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    // Use the form: *d *h *m *s for days, hours, minutes, seconds."
    /// <summary>
    /// Parses the input string and return a timespan instance.
    /// </summary>
    /// <param name="cacheAge">Use the form: *d *h *m *s for days, hours, minutes, seconds."</param>
    public static TimeSpan ParseTimespanString(string cacheAge)
    {
      // TimeSpan res = new TimeSpan();
      int days = 0;
      int hours = 0;
      int minutes = 0;
      int seconds = 0;

      cacheAge.Replace(":", " ");

      string[] parts = cacheAge.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string part in parts)
      {
        string p = part.Trim();
        char specifier = p[p.Length - 1];
        switch (specifier)
        {
          case 'd':
            days = int.Parse(p.Substring(0, p.Length - 1));
            break;

          case 'h':
            hours = int.Parse(p.Substring(0, p.Length - 1));
            break;

          case 'm':
            minutes = int.Parse(p.Substring(0, p.Length - 1));
            break;

          case 's':
            seconds = int.Parse(p.Substring(0, p.Length - 1));
            break;
          default:
            throw new NotSupportedException($"Format specifier: {specifier} is not supported!");
        }

      }

      var res = new TimeSpan(days, hours, minutes, seconds);
      return res;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string PadString(string input, int paddedLength)
    {
      int padSize = paddedLength - input.Length;
      if (padSize <= 0) { return input; }

      string padWith = new string(' ', padSize);

      var side = EPadSide.Right;
      switch (side)
      {
        case EPadSide.Left:
          return padWith + input;

        case EPadSide.Right:
          return input + padWith;

        default:
          throw new NotSupportedException();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Remove <paramref name="toTrim"/> from the end of the <paramref name="input"/> string.
    /// Multiple instances of <paramref name="toTrim"/> will be removed from the input string.
    /// </summary>
    public static string TrimEnd(string input, string toTrim)
    {
      while (input.EndsWith(toTrim))
      {
        input = input.Substring(0, input.Length - toTrim.Length);
      }
      return input;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if the given email address is valid or not.
    /// </summary>
    /// <remarks>
    /// Email addres validation is difficult.  This function may not cover all cases.
    /// Please report any valid email address that causes this function to return false.
    /// </remarks>
    public static bool IsValidEmail(string email)
    {
      // Thanks Internet!
      // Original version from:
      // https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address

      var trimmedEmail = email.Trim();

      if (trimmedEmail.EndsWith("."))
      {
        return false; // suggested by @TK-421
      }
      try
      {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == trimmedEmail;
      }
      catch
      {
        return false;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This computes a valid filename from an input URI.
    /// The computed filename is safe on Windows, Linux, and MacOS filesystems.
    /// </summary>
    /// <param name="preserveScheme">When false, this will remove scheme information (http/https/etc) from the url.</param>
    /// <param name="preserveFolders">When true, slashes ('/') in the url will be represented as folders.  If false, the url will be translated to a single file name.</param>
    public static string TranslateUrlToFilename(Uri uri, bool preserveScheme = false, bool preserveFolders = true)
    {
      string useUrl = uri.Scheme + "//" + uri.AbsolutePath;
      if (!string.IsNullOrWhiteSpace(uri.Query))
      {
        useUrl += "?" + uri.Query;
      }
      string res = TranslateUrlToFilename(useUrl, preserveScheme, preserveFolders);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This computes a valid filename from an input URL.
    /// The computed filename is safe on Windows, Linux, and MacOS filesystems.
    /// </summary>
    /// <remarks>
    /// This function assumes that you are passing a valid url.
    /// This function will remove any scheme information from the url
    /// </remarks>
    /// <param name="preserveScheme">When false, this will remove scheme information (http/https/etc) from the url.</param>
    /// <param name="preserveFolders">When true, slashes ('/') in the url will be represented as folders.  If false, the url will be translated to a single file name.</param>
    public static string TranslateUrlToFilename(string url, bool preserveScheme = false, bool preserveFolders = true)
    {
      string res = url;
      if (!preserveScheme)
      {
        res = Regex.Replace(url, "http(s)?://", "");
      }
      else
      {
        res = Regex.Replace(url, "(http[s])?://", "$1__");
      }

      if (preserveFolders)
      {
        res = res.Replace('/', Path.DirectorySeparatorChar);
      }
      else
      {
        res = res.Replace("/", "_");
      }

      res = res.Replace("?", "qs_");
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static int StrCompare(string x, string y)
    {

      int xLen = x.Length;
      int yLen = y.Length;
      int minLen = Math.Min(xLen, yLen);

      int comp = 0;
      int i = 0;
      while (comp == 0 & i < minLen)
      {
        int xc = x[i];
        int yc = y[i];
        if ((uint)(xc - 'a') <= (uint)('z' - 'a')) xc -= 0x20;
        if ((uint)(yc - 'a') <= (uint)('z' - 'a')) yc -= 0x20;


        comp = xc - yc;
        ++i;
      }
      if (comp == 0) { comp = xLen - yLen; }
      return Math.Sign(comp);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Breaks a camel cased string into its constituent words.
    /// </summary>
    public static string DeCamelCase(string input)
    {
      // We need at least three characters to detect a case change.
      int len = input.Length;
      if (len < 3) { return input; }

      int start = 0;
      int end = 0;
      bool lastUpper = IsUppercase(input[0]);

      StringBuilder sb = new StringBuilder(len * 2);

      // Scan each character....
      for (int i = 1; i < len; i++)
      {
        int cVal = (int)(input[i]);
        bool isUpper = (cVal >= 65 && cVal <= 90);

        // If we have an uppercase, then we might be at a new word.
        if (isUpper && !lastUpper)
        {
          end = i;
          int wordLen = end - start;
          // Extract the word!
          string word = input.Substring(start, wordLen);
          if (sb.Length > 0) { sb.Append(" "); }
          sb.Append(word);

          start = end;
          end = 0;
        }

        lastUpper = isUpper;
      }

      // Extract the last word.
      if (end == 0)
      {
        string word = input.Substring(start);
        if (sb.Length > 0) { sb.Append(" "); }
        sb.Append(word);
      }

      string res = sb.ToString();
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool IsUppercase(char c)
    {
      int cVal = (int)(c);
      bool isUpper = (cVal >= 65 && cVal <= 90);
      return isUpper;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Truncate the input to the given length if its length exceeds it.  An ellipsis can optionally be
    /// used at the end of the string.
    /// </summary>
    /// <param name="useEllipsis">If set, and ellipsis '...' will be the last characters of the truncated string.
    /// If <paramref name="length"/> is less than the length of the ellipsis, 'useEllipsis' will be ignored.</param>
    /// <returns></returns>
    public static string Truncate(string input, int length, bool useEllipsis = false)
    {
      if (input == null) { return input; }
      if (input.Length > length)
      {
        if (useEllipsis && length > ELLIPSIS.Length)
        {
          length = (length - ELLIPSIS.Length);
          string res = input.Substring(0, (int)length) + ELLIPSIS;
          return res;
        }
        else
        {
          string res = input.Substring(0, (int)length);
          return res;
        }

      }
      return input;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string TruncateParagraph(string input, int charLimit)
    {
      if (input == null) { return null; }
      if (input.Length <= charLimit) { return input; }

      // Let's truncate, back to the last word.
      string res = input.Substring(0, charLimit);
      char lastChar = res[charLimit - 1];
      if (lastChar != ' ')
      {
        int lastSpace = res.LastIndexOf(' ');
        if (lastSpace != -1)
        {
          res = res.Substring(0, lastSpace);
        }
      }
      res += "...";
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Generates a unique string, based on the list of existing strings.
    /// </summary>
    /// <remarks>This will use a counter to create a unique string.  It may have poor performance depending on the list
    /// of existing strings, and how their contents.</remarks>
    public static string GetUniqueString(string baseString, IEnumerable<string> existingStrings)
    {
      int count = 0;
      string res = baseString;
      int len = existingStrings.Count();
      while (existingStrings.Contains(res))
      {
        count++;
        res = baseString + "_" + count;
      }

      return res;
    }


    // ------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Compute a hash for the given string.  This should be compatible with the C++ version. (LPCWSTR)
    /// </summary>
    /// <remarks>
    /// Null strings will return 0, and an empty string will return 5381.
    /// </remarks>
    // http://www.cse.yorku.ca/~oz/hash.html
    public static UInt64 GetHash(string str)
    {
      if (str == null) { return 0; }

      UInt64 hash = 5381;

      foreach (var c in str)
      {
        hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
      }

      return hash;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Case insensitive string comparison.  May not work for all cultures.  Suitable for fast sorting of alphanumeric strings
    /// only.
    /// </summary>
    public static int AlphaNumericStrCompare_NoCase(string x, string y)
    {

      int xLen = x.Length;
      int yLen = y.Length;
      int minLen = SysMath.Min(xLen, yLen);

      int comp = 0;
      int i = 0;
      while (comp == 0 & i < minLen)
      {
        int xc = x[i];
        int yc = y[i];
        if ((uint)(xc - 'a') <= (uint)('z' - 'a')) xc -= 0x20;
        if ((uint)(yc - 'a') <= (uint)('z' - 'a')) yc -= 0x20;


        comp = xc - yc;
        ++i;
      }
      if (comp == 0) { comp = xLen - yLen; }
      return SysMath.Sign(comp);

    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static string ToHexString(string input)
    {
      if (input == null) { return null; }

      char[] charValues = input.ToCharArray();

      // NOTE: We could use string builder to save on some allocations....
      string res = "";
      foreach (char _eachChar in charValues)
      {
        int value = Convert.ToInt32(_eachChar);
        res += String.Format("{0:X}", value);
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string RemoveExtraSpaces(string input)
    {
      // NOTE:  This is a really slow way to do it!
      while (input.IndexOf("  ") != -1)
      {
        input = input.Replace("  ", " ");
      }
      return input;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string UppercaseFirstLetterOfEachWord(string input)
    {
      string[] wordParts = input.Split(' ');

      for (int windex = 0; windex < wordParts.Length; windex++)
      {
        var word = wordParts[windex];
        char[] newWord = new char[word.Length];
        for (int i = 0; i < word.Length; i++)
        {
          char c = word[i];
          if (i == 0 && c >= 97 && c <= 122)
          {
            c = (char)((byte)c - 32);
          }
          newWord[i] = c;
        }

        wordParts[windex] = new string(newWord);
      }


      string res = string.Join(" ", wordParts);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Formats the input string with the given mask.
    /// </summary>
    public static string ApplyMask(string input, string mask)
    {
      const char COPY_CHAR = '#';
      StringBuilder sb = new StringBuilder();

      int maskLen = mask.Length;
      int inputLen = input.Length;

      int maskIndex = 0;
      int inputIndex = 0;

      while (maskIndex < maskLen & inputIndex < inputLen)
      {
        char mc = mask[maskIndex];
        if (mc == COPY_CHAR)
        {
          sb.Append(input[inputIndex]);
          ++maskIndex;
          ++inputIndex;
        }
        else
        {
          sb.Append(mc);
          ++maskIndex;
        }
      }

      string res = sb.ToString();
      return res;

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Converts the given string to base64 format.
    /// </summary>
    /// <remarks>
    /// Assumes UTF8 encoding.
    /// </remarks>
    public static string ToBase64String(string input)
    {
      Encoding enc = Encoding.UTF8;

      byte[] strBytes = enc.GetBytes(input);
      string res = Convert.ToBase64String(strBytes);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Converts the given string from the base64 format.
    /// </summary>
    /// <remarks>
    /// Assumes UTF8 encoding for the output string.
    /// </remarks>
    public static string FromBase64String(string input)
    {
      Encoding enc = Encoding.UTF8;

      byte[] strBytes = Convert.FromBase64String(input);

#if NETFX_CORE
      string res = enc.GetString(strBytes, 0, strBytes.Length);
#else
      string res = enc.GetString(strBytes);
#endif

      return res;
    }

#if !NETFX_CORE

    // --------------------------------------------------------------------------------------------------------------------------
    public static string ComputeSHA1(string input)
    {
      using (SHA1Managed sha1 = new SHA1Managed())
      {
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(hash.Length * 2);

        foreach (byte b in hash)
        {
          // can be "x2" if you want lowercase
          sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string ComputeSHA256(string text)
    {
      // Thanks to: https://stackoverflow.com/questions/12416249/hashing-a-string-with-sha256#12416380
      byte[] bytes = Encoding.UTF8.GetBytes(text);
      SHA256Managed hashstring = new SHA256Managed();
      byte[] hash = hashstring.ComputeHash(bytes);
      string res = string.Empty;
      foreach (byte x in hash)
      {
        res += String.Format("{0:x2}", x);
      }
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string ComputeMD5(string text)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(text);
      var hashstring = MD5.Create();
      byte[] hash = hashstring.ComputeHash(bytes);

      StringBuilder sb = new StringBuilder(32);
      foreach (byte x in hash)
      {
        sb.AppendFormat("{0:x2}", x);
      }
      return sb.ToString();
    }

#else

    public static string GetMd5Hash(string input)
    {
      var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
      IBuffer buff = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
      IBuffer hashed = alg.HashData(buff);
      var res = CryptographicBuffer.EncodeToHexString(hashed);
      return res;
    }

#endif


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Unlike ToString(), this allows for null objects.
    /// </summary>
    /// <param name="target"></param>
    /// <remarks>
    /// With newer versions of .NET object?.ToString() can be used instead of this function.
    /// </remarks>
    public static string AsString(object target)
    {
      return target == null ? null : target.ToString();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns a document as a string that follows the given encoding rules.
    /// </summary>
    /// <param name="encoding">Optional.  If omitted, the system will default to UTF-8</param>
    public static string ConvertToString(XDocument doc, Encoding encoding = null)
    {
      if (encoding == null)
      {
        encoding = System.Text.Encoding.UTF8;
      }

      using (CustomStringWriter writer = new CustomStringWriter(encoding))
      {
        // The encoding that is listed in the XML declaration will be adjusted to 'encoding'
        doc.Save(writer);
        return writer.GetStringBuilder().ToString();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Takes a quoted string, and returns the data that is inside of the quotes.
    /// </summary>
    /// <remarks>
    /// This function assumes that there is only one set of quotes.  It won't do anything fancy.
    /// </remarks>
    public static string Unquote(string input)
    {
      return GetInnerString(input, '"', '"');
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add quotes to the input string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Quote(string input)
    {
      string res = "\"" + input + "\"";
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Looks for a closed set of parenthesis in a string, and will extract the data between them.
    /// Returns null if parens are not present or mis-matched.  WILL NOT take nesting into account.
    /// </summary>
    public static string ExtractParenthesisData(string s)
    {
      return GetInnerString(s, '(', ')');
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Extracts the inner string from one that is enclosed in the given open/close pair.
    /// </summary>
    public static string GetInnerString(string input, char openChar, char closeChar)
    {

      int start = input.IndexOf(openChar) + 1;
      int stop = input.LastIndexOf(closeChar);

      if (start == -1 | stop == -1)
      {
        throw new InvalidOperationException("Please provide a quoted string!");
      }
      return input.Substring(start, stop - start);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Splits an equality expression into left and right parts.
    /// An equality expressions is something like ' x = y '
    /// </summary>
    /// <returns>
    /// An array of strings with index 0 representing the left side, and index 1 representing the right.
    /// Will return null if the expression has no equality character.
    /// </returns>
    /// <remarks>
    /// This will successively split expressions with multiple left / right components.
    /// x = y = z --> 0 = x, 1 = y = z
    /// </remarks>
    public static string[] SplitEqualityExpression(string expression, bool trimEach = false)
    {
      const string SPLIT_STR = "=";

      string[] exParts = expression.Split(new[] { SPLIT_STR }, StringSplitOptions.None);
      if (exParts.Length == 1) { return null; }

      string[] res = new string[2];
      res[0] = exParts[0];
      res[1] = string.Join(SPLIT_STR, exParts, 1, exParts.Length - 1);

      if (trimEach)
      {
        for (int i = 0; i < res.Length; i++)
        {
          res[i] = res[i].Trim();
        }
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static Dictionary<string, string> EscapeMappings = new Dictionary<string, string>()
    {
        {"\"", @"\\"},
        {"\'", @"'"},
        {"\\\\", @"\\"},
        {"\a", @"\a"},
        {"\b", @"\b"},
        {"\f", @"\f"},
        {"\n", @"\n"},
        {"\r", @"\r"},
        {"\t", @"\t"},
        {"\v", @"\v"},
        {"\0", @"\0"},
    };


    // --------------------------------------------------------------------------------------------------------------------------
    // NOTE: There is no unicode support at this time!
    public static string EscapeString(string s)
    {
      foreach (var key in EscapeMappings.Keys)
      {
        s = s.Replace(key, EscapeMappings[key]);
      }
      return s;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // NOTE: There is no unicode support at this time!
    public static string UnescapeString(string s)
    {
      foreach (var key in EscapeMappings.Keys)
      {
        s = s.Replace(EscapeMappings[key], key);
      }
      return s;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Escapes all special XML entities in a string.
    /// </summary>
    public static string EscapeXml(string s)
    {
      string xml = s;
      if (!string.IsNullOrEmpty(xml))
      {
        // replace literal values with entities
        xml = xml.Replace("&", "&amp;");
        xml = xml.Replace("<", "&lt;");
        xml = xml.Replace(">", "&gt;");
        xml = xml.Replace("\"", "&quot;");
        xml = xml.Replace("'", "&apos;");
      }
      return xml;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Unescapes all special XML entities in a string.
    /// </summary>
    public static string UnescapeXml(string s)
    {
      string unxml = s;
      if (!string.IsNullOrEmpty(unxml))
      {
        // replace entities with literal values
        unxml = unxml.Replace("&apos;", "'");
        unxml = unxml.Replace("&quot;", "\"");
        unxml = unxml.Replace("&gt;", ">");
        unxml = unxml.Replace("&lt;", "<");
        unxml = unxml.Replace("&amp;", "&");
      }
      return unxml;
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Adds a single line + formatting characters onto a string for you.  Useful for creating large chunks of text data.
    /// </summary>
    public static string AddLine(string s, string line)
    {
      return s + line + "\r\n";
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Given the string, this will attempt to give us a 'Type' representation of it.  Naturally it only works for the basic
    /// data types.  It will attempt to find type representations fro the SQL types too.
    /// </summary>
    /// <param name="dataLength">If this is a SQL type, this is the length, for example VARCHAR(32) --> length = 32</param>
    public static Type GetTypeFromString(string typeString, out int dataLength, bool checkForSQLType = true, bool throwOnUnrecognized = true)
    {
      typeString = typeString.Trim();
      dataLength = -1;
      string s = typeString.ToLower();

      Type res = null;

      // Try a simple conversion...
      if (typeString.StartsWith("system."))
      {
#if NETFX_CORE
        res = Type.GetType(s, false);
#else
        res = Type.GetType(s, false, true);
#endif
        if (res != null)
        {
          return res;
        }
      }

      // OK, let's try to tack a 'System." onto the front and then parse.
      string makeSystem = "System." + s;
#if NETFX_CORE
      res = Type.GetType(makeSystem, false);
#else
      res = Type.GetType(makeSystem, false, true);
#endif
      if (res != null)
      {
        return res;
      }


      // As a last result, we can do the brute-force synonym style search.
      switch (s)
      {
        case "int":
          return typeof(int);
        case "long":
          return typeof(long);

        // Unrecognized items should fall through.
        default:
          break;
      }


      // OK, none of that worked.  Let's see if this is a SQL type or something.
      Type sqlType = null;
      if (checkForSQLType)
      {
        sqlType = GetSQLTypeFromString(typeString, out dataLength);
        if (sqlType != null) { return sqlType; }
      }

      // To err, or not to err ?
      if (throwOnUnrecognized)
      {
        throw new Exception(string.Format("The data type string '{0}' is not valid!"));
      }
      return null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Determines if a string is a number, or not.
    /// TODO: Some kind of perf/value test on this would be cool!
    /// </summary>
    public static bool IsNumeric(string s)
    {
      return Regex.IsMatch(s.Trim(), "^-?[0-9]*[.]?[0-9]*$");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Determines if the string is an integer.
    /// TODO: Some kind of perf/value test on this would be cool!
    /// </summary>
    public static bool IsInteger(string s)
    {
      return Regex.IsMatch(s.Trim(), "^-?[0-9]+$");
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Handles conversion of a string to an int32.  Takes care of empty strings and null values, etc.  You may provide your
    /// own default value if the string cannot be converted.
    /// </summary>
    public static Int32 ToInt32(string s, int defaultValue = 0)
    {
      if (string.IsNullOrEmpty(s) || !StringTools.IsNumeric(s))
      {
        return defaultValue;
      }

      return Convert.ToInt32(s);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Used when we odont' care about the data length.
    /// </summary>
    public static Type GetTypeFromString(string typeString)
    {
      int dataLength = -1;
      return GetTypeFromString(typeString, out dataLength, true, true);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Attempts to interpret the input string as a type from SQL.
    /// Returns null if the input string is not recognized.
    /// </summary>
    public static Type GetSQLTypeFromString(string input, out int dataLength)
    {
      input = input.Trim();
      dataLength = -1;
      string s = input.ToLower();

      // Hmm.... still not working..  let's see if it is a (fixed size) SQL type of some sort....
      switch (s)
      {
        case "bigint":
          return typeof(Int64);
        case "text":
          return typeof(string);
        case "bit":
          return typeof(bool);
      }


      // There are some lenth associated SQL types too.....
      if (s.StartsWith("varchar"))
      {
        Type t = typeof(string);
        int.TryParse(ExtractParenthesisData(s), out dataLength);
        return t;
      }

      // Could not find anything....
      return null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// When we dont' care about the length of the data, use this...
    /// </summary>
    public static Type GetSQLTypeFromString(string input)
    {
      int dataLength = -1;
      return GetSQLTypeFromString(input, out dataLength);
    }

#if !NETFX_CORE

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a new secure string for us, without requiring that we do all of that goofy character appending and what-not.
    /// </summary>
    public static SecureString ToSecureString(string s)
    {
      if (s == null || string.IsNullOrWhiteSpace(s)) { return null; }

      SecureString secure = new SecureString();
      foreach (char c in s)
      {
        secure.AppendChar(c);
      }
      return secure;
    }

#endif

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Some characters can't be represented in XML documents, so they must be encoded.  (Spaces, hashes, etc.)
    /// Not all of the speical characters are supported at this time.
    /// </summary>
    public static string EncodeSpecialXMLCharacters(string s)
    {
      // hmmm.... bathIndex am seeing a pattern here....
      s = s.Replace(" ", "_0x20");
      s = s.Replace("#", "_0x23");
      s = s.Replace("$", "_0x24");
      return s;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Decode the special characters.
    /// </summary>
    public static string DecodeSpecialXMLCharacters(string s)
    {
      // ? Look at the similar functions.  Is an actual array a good idea?? Maybe in the future? 
      s = s.Replace("_0x20", " ");
      s = s.Replace("_0x23", "#");
      s = s.Replace("_0x24", "$");
      return s;
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if our string contains only alphabetic characters.  Case insensitive.
    /// </summary>
    public static bool IsAlphabetic(string s)
    {
      int strLen = s.Length;
      for (int i = 0; i < strLen; i++)
      {
        if (!IsAlphabetic(s[i]))
        {
          return false;
        }
      }
      return true;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static bool IsAlphabetic(char c)
    {
      byte val = (byte)c;
      if (!((val >= 65 && val <= 90) || (val >= 97 && val <= 122)))
      {
        return false;
      }
      return true;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if our strings contain alphanumeric characters, as well as the underscore character.
    /// </summary>
    public static bool IsAlphaNumeric(string s, bool includeSpace = false, bool includeUnderscore = false)
    {
      string pattern = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      if (includeSpace) { pattern += " "; }
      if (includeUnderscore) { pattern += "_"; }

      return AllCharactersMatch(s, pattern);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This is the fastest version of the character matching pattern, but it requires a non-regex string.
    /// All other versions are obsolete, since using a regex string is technically wrong + lead to misleading results.
    /// </summary>
    public static bool AllCharactersMatch(string s, string pattern)
    {
      for (int i = 0; i < s.Length; i++)
      {
        bool match = false;
        for (int j = 0; j < pattern.Length; j++)
        {
          if (s[i] == (pattern[j]))
          {
            match = true;
            break;
          }
        }
        if (!match) { return false; }
      }
      return true;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Removes any padding characters from the left side of the given string.
    /// </summary>
    public static string UnpadLeft(string s, char paddingChar, int minLength)
    {
      int start = 0;
      while (start < s.Length)
      {
        if (s.IndexOf(paddingChar, start) == -1)
        {
          // Do the trim...
          return s.Substring(start);
        }
        start++;
      }

      // If we got here, it is all padding characters.
      return Right(s, minLength);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns all of the characters from the right side of the string.
    /// </summary>
    public static string Right(string s, int length)
    {
      if (length > s.Length)
      {
        length = s.Length;
      }
      return s.Substring(s.Length - length, length);
    }



    // --------------------------------------------------------------------------------------------------------------------------
    public static int GetCharCount(string s, int startIndex, char c)
    {
      int count = 0;
      int sLen = s.Length;
      for (int i = startIndex; i < sLen; i++)
      {
        if (c == s[i]) { count++; }
      }

      return count;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    ///<remarks>
    /// Originally lifted from: http://stackoverflow.com/questions/4135317/make-first-letter-of-a-string-upper-case-for-maximum-performance#4135491
    /// </remarks>
    public static string FirstLetterToUpper(string str)
    {
      if (str == null) { return null; }
      if (str.Length > 1) { return char.ToUpper(str[0]) + str.Substring(1); }
      return str.ToUpper();
    }


  }



  // ============================================================================================================================
  /// <summary>
  /// Acts like a normal StringWriter, but allows you to provide your own encoding.
  /// </summary>
  public class CustomStringWriter : StringWriter
  {
    private Encoding _Encoding;

    // --------------------------------------------------------------------------------------------------------------------------
    public CustomStringWriter(Encoding encoding_)
    {
      _Encoding = encoding_;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override Encoding Encoding
    {
      get
      {
        return _Encoding;
      }
    }
  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Describes what side of a string padding may be applied to.
  /// </summary>
  public enum EPadSide { Invalid = 0, Left, Right }

}
