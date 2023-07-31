using System.Reflection;
using System.Text.RegularExpressions;

namespace AppFrame.Expressions;
public partial class WhereExpressionComparison
{
    public string FieldName { get; set; }
    public QueryOperator Operator { get; set; }
    public byte[] Value { get; set; }

    public static IEnumerable<WhereExpressionComparison> ParseExpressionText<T>(string text)
    {
        // PARSE THIS TEXT into the list of Fields and operators and values 
        // "Name equals 'will', Address notequals '16 brook meadow', balance greaterthan 0"
        // All fields must exist as a valid property in the list of properties and the type must be valid for the property type listed
        List<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Public).ToList();
        List<WhereExpressionComparison> outlist = new();

        // Split the input text using the regular expression to handle commas 
        // inside quoted strings or numbers
        string[] expressions = MyRegex().Split(text);

        foreach(string expression in expressions)
        {
            // find the location of the first space in the string and see if it's a valid property name
            int space = expression.IndexOf(' ');
            if (space == -1) throw new ArgumentException($"Invalid expression: {expression}");
            string fieldname = expression[..space];
            PropertyInfo property = properties.FirstOrDefault(p => p.Name.Equals(fieldname, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException($"Invalid field name: {fieldname}");

            // Find the operator
            int space2 = expression.IndexOf(' ', space + 1);
            if (space2 == -1) throw new ArgumentException($"Invalid expression: {expression}");
            string operatorstring = expression.Substring(space + 1, space2 - space - 1);
            QueryOperator queryOperator = Enum.Parse<QueryOperator>(operatorstring);

            // Get the value
            string value = expression[(space2 + 1)..];
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            outlist.Add(new WhereExpressionComparison { FieldName = fieldname, Operator = queryOperator, Value = bytes });
        }

        return outlist; 

    }

    [GeneratedRegex(",(?=(?:[^']*'[^']*')*[^']*$)")]
    private static partial Regex MyRegex();
}
