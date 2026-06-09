using JiraReport.Models.ValueObjects;

namespace JiraReport.Models;

internal sealed record IssueFlow(IssueKey Key, IReadOnlyList<FlowTransition> Transitions);

internal sealed record FlowTransition(string From, string To, DateTimeOffset At, TimeSpan TimeInFromStatus);

internal sealed record FlowPathGroup(
    string Path,
    IReadOnlyList<IssueKey> Issues,
    IReadOnlyList<FlowStageSummary> Stages);

internal sealed record FlowStageSummary(string Status, string NextStatus, TimeSpan MedianDuration);
