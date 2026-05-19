namespace API.InternalClasses;

public static class Transliteration
{
private static readonly Dictionary<char, string> Map = new()
{
{'а', "A"}, {'б', "B"}, {'в', "V"}, {'г', "G"}, {'д', "D"},
{'е', "E"}, {'ё', "YO"}, {'ж', "ZH"}, {'з', "Z"}, {'и', "I"},
{'й', "Y"}, {'к', "K"}, {'л', "L"}, {'м', "M"}, {'н', "N"},
{'о', "O"}, {'п', "P"}, {'р', "R"}, {'с', "S"}, {'т', "T"},
{'у', "U"}, {'ф', "F"}, {'х', "KH"}, {'ц', "TS"}, {'ч', "CH"},
{'ш', "SH"}, {'щ', "SHCH"}, {'ъ', ""}, {'ы', "Y"}, {'ь', ""},
{'э', "E"}, {'ю', "YU"}, {'я', "YA"},
{'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"},
{'Е', "E"}, {'Ё', "YO"}, {'Ж', "ZH"}, {'З', "Z"}, {'И', "I"},
{'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"}, {'Н', "N"},
{'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"},
{'У', "U"}, {'Ф', "F"}, {'Х', "KH"}, {'Ц', "TS"}, {'Ч', "CH"},
{'Ш', "SH"}, {'Щ', "SHCH"}, {'Ъ', ""}, {'Ы', "Y"}, {'Ь', ""},
{'Э', "E"}, {'Ю', "YU"}, {'Я', "YA"},
};

public static string ToLatin(string cyrillic)
{
if (string.IsNullOrWhiteSpace(cyrillic)) return cyrillic;
var result = new System.Text.StringBuilder();
foreach (char c in cyrillic)
{
if (Map.TryGetValue(c, out var latin))
result.Append(latin);
else if (c == ' ')
result.Append(' ');
else
result.Append(c);
}
return result.ToString();
}
}
