using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppFrame.Interfaces;
public interface ITrackedWorkflow
{
    ITable<IWorkflowDefinition> tableWorkflow {get;set;}
    Task<string> StartWorkflowAsync(string workflowName, string initialStep, IWorkflowState initialState);
    Task<bool> AdvanceWorkflowAsync(string workflowId);
    Task<bool> RollbackWorkflowAsync(string workflowId);
    Task<IWorkflowState> GetWorkflowStateAsync(string workflowId);
}
public interface IWorkflowStep
{
    string Name { get; }
    WorkflowStepType Type { get; }
    IActivity Activity { get; }
    Dictionary<string, string> ExternalRenderingData { get; }
}
public interface IWorkflowBranch
{
    string Condition { get; }
    string NextStep { get; }
}
public interface IWorkflowState
{
    string CurrentStep { get; set; }
    Dictionary<string, string> Data { get; }
}
public interface IWorkflowDefinition
{
    string Name { get; }
    List<IWorkflowStep> Steps { get; }
    List<IWorkflowBranch> Branches { get; }
}
public enum WorkflowStepType
{
    Activity,
    Branch
}
public interface IActivity
{
    Task ExecuteAsync(IWorkflowState state, Dictionary<string, string> externalData);
}
