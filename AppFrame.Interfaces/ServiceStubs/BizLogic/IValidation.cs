namespace AppFrame.Interfaces;
public interface IValidation<T>
{
    ITable<T> Table { get; }
    Task<bool> ValidateObjectAsync(T obj);
    Task<IEnumerable<ValidationResult>> GetValidationResultsAsync(T obj);
    Task<bool> EvaluateBusinessRulesAsync(T obj);
    Task<IEnumerable<BusinessRuleResult>> GetBusinessRuleResultsAsync(T obj);
}
public class ValidationResult{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }

}
public class BusinessRuleResult{
    public string RuleName { get; set;}
    public string RuleId { get; set;}
    public string ErrorMessage { get; set; }
}