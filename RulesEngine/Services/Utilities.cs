namespace RulesService.Services;

public class CustomTypes
{
    public static bool Contains(string value, string stringList)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(stringList))
            return false;

        var list = stringList.Split(',').ToList();
        return list.Contains(value);
    }
}
