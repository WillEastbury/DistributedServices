using System.Text;
using System.Collections.Generic;
namespace AppFrame.Interfaces;

public interface IForm<T>
{
    ITable<T> Table { get; }
    string RenderView(string tableName, string itemId);
    string RenderEdit(string tableName, string itemId);
    string RenderDelete(string tableName, string itemId);
    string RenderSearch(string tableName);
    string RenderListAsGrid(string tableName);
}

public static class FormTemplates
{
    public static string RenderViewTemplate(string tableName, string itemId, IDictionary<string, string> fieldValues)
    {
        // Generate HTML form for view
        var formHtml = new StringBuilder();
        formHtml.AppendLine($"<h2>View {tableName} Item</h2>");

        foreach (var field in fieldValues)
        {
            formHtml.AppendLine($"<p><strong>{field.Key}:</strong> {field.Value}</p>");
        }

        return formHtml.ToString();
    }

    public static string RenderEditTemplate(string tableName, string itemId, IDictionary<string, string> fieldValues)
    {
        // Generate HTML form for edit
        var formHtml = new StringBuilder();
        formHtml.AppendLine($"<h2>Edit {tableName} Item</h2>");
        formHtml.AppendLine("<form>");

        foreach (var field in fieldValues)
        {
            formHtml.AppendLine($"<div>");
            formHtml.AppendLine($"<label for=\"{field.Key}\">{field.Key}:</label>");
            formHtml.AppendLine($"<input type=\"text\" id=\"{field.Key}\" name=\"{field.Key}\" value=\"{field.Value}\">");
            formHtml.AppendLine($"</div>");
        }

        formHtml.AppendLine("<button type=\"submit\">Save</button>");
        formHtml.AppendLine("</form>");

        return formHtml.ToString();
    }

    public static string RenderDeleteTemplate(string tableName, string itemId)
    {
        // Generate HTML form for delete
        var formHtml = new StringBuilder();
        formHtml.AppendLine($"<h2>Delete {tableName} Item</h2>");
        formHtml.AppendLine("<form>");
        formHtml.AppendLine($"<p>Are you sure you want to delete the {tableName} item with ID: {itemId}?</p>");
        formHtml.AppendLine("<button type=\"submit\">Delete</button>");
        formHtml.AppendLine("</form>");

        return formHtml.ToString();
    }

    public static string RenderSearchTemplate(string tableName)
    {
        // Generate HTML form for search
        var formHtml = new StringBuilder();
        formHtml.AppendLine($"<h2>Search {tableName}</h2>");
        formHtml.AppendLine("<form>");

        // ToDoRender search fields based on table metadata
        // var tableMetadata = GetTableMetadata(tableName);
        // foreach (var field in tableMetadata.Fields)
        // {
        //     formHtml.AppendLine($"<div>");
        //     formHtml.AppendLine($"<label for=\"{field}\">{field}:</label>");
        //     formHtml.AppendLine($"<input type=\"text\" id=\"{field}\" name=\"{field}\">");
        //     formHtml.AppendLine($"</div>");
        // }

        formHtml.AppendLine("<button type=\"submit\">Search</button>");
        formHtml.AppendLine("</form>");

        return formHtml.ToString();
    }

    public static string RenderListAsGridTemplate(string tableName, IList<IDictionary<string, string>> items)
    {
        // Generate HTML grid for item list
        var gridHtml = new StringBuilder();
        gridHtml.AppendLine($"<h2>{tableName} List</h2>");
        gridHtml.AppendLine("<table>");
        gridHtml.AppendLine("<thead>");
        gridHtml.AppendLine("<tr>");

        // Render table headers based on field names
        // var tableMetadata = GetTableMetadata(tableName);
        // foreach (var field in tableMetadata.Fields)
        // {
        //     gridHtml.AppendLine($"<th>{field}</th>");
        // }

        gridHtml.AppendLine("</tr>");
        gridHtml.AppendLine("</thead>");
        gridHtml.AppendLine("<tbody>");

        // Render table rows with item data
        // foreach (var item in items)
        // {
        //     gridHtml.AppendLine("<tr>");

        //     foreach (var field in tableMetadata.Fields)
        //     {
        //         gridHtml.AppendLine($"<td>{item[field]}</td>");
        //     }

        //     gridHtml.AppendLine("</tr>");
        // }

        gridHtml.AppendLine("</tbody>");
        gridHtml.AppendLine("</table>");

        return gridHtml.ToString();
    }
}