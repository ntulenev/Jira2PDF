using FluentAssertions;

using JiraReport.Logic;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Logic;

public sealed class JiraLogicServiceTests
{
    [Fact(DisplayName = "ResolveOutputColumns returns default columns when config is null")]
    [Trait("Category", "Unit")]
    public void ResolveOutputColumnsWhenConfigIsNullReturnsDefaultColumns()
    {
        // Arrange
        var service = new JiraLogicService();
        var issue = new JiraIssue(
            new IssueKey("APP-1"),
            new Dictionary<IssueKey, FieldValue>
            {
                [new IssueKey("summary")] = new FieldValue("Implement report"),
                [new IssueKey("issuetype")] = new FieldValue("Story"),
                [new IssueKey("status")] = new FieldValue("Open"),
                [new IssueKey("assignee")] = new FieldValue("Jane Doe"),
                [new IssueKey("created")] = new FieldValue("2026-02-28"),
                [new IssueKey("updated")] = new FieldValue("2026-03-01")
            });

        // Act
        var columns = service.ResolveOutputColumns(configuredFields: null);

        // Assert
        columns.Select(static column => column.Key.Value).Should().ContainInOrder(
            "key",
            "issuetype",
            "status",
            "assignee",
            "created",
            "updated",
            "summary");
        columns[0].Selector(issue).Value.Should().Be("APP-1");
        columns[^1].Selector(issue).Value.Should().Be("Implement report");
    }

    [Fact(DisplayName = "ResolveOutputColumns applies configured aliases")]
    [Trait("Category", "Unit")]
    public void ResolveOutputColumnsWhenAliasesAreConfiguredUsesAliasHeaders()
    {
        // Arrange
        var service = new JiraLogicService();

        // Act
        var columns = service.ResolveOutputColumns(
            [new IssueFieldName("customfield_11868"), new IssueFieldName("summary")],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["customfield_11868"] = "Sport"
            });

        // Assert
        columns.Select(static column => column.Header.Value).Should().ContainInOrder("Sport", "Summary");
    }

    [Fact(DisplayName = "ResolveRequestedIssueFields merges output and count fields without duplicates")]
    [Trait("Category", "Unit")]
    public void ResolveRequestedIssueFieldsWhenConfiguredMergesOutputAndCountFieldsWithoutDuplicates()
    {
        // Arrange
        var service = new JiraLogicService();

        // Act
        var fields = service.ResolveRequestedIssueFields(
            [new IssueFieldName("summary"), new IssueFieldName(" status "), new IssueFieldName("summary")],
            [new IssueFieldName("assignee"), new IssueFieldName("Status")]);

        // Assert
        fields.Select(static field => field.Value).Should().ContainInOrder("summary", "status", "assignee");
    }

    [Fact(DisplayName = "BuildDefaultPdfPath creates timestamped report path")]
    [Trait("Category", "Unit")]
    public void BuildDefaultPdfPathCreatesTimestampedReportPath()
    {
        // Arrange
        var service = new JiraLogicService();
        var generatedAt = new DateTimeOffset(2026, 2, 28, 18, 10, 30, TimeSpan.Zero);

        // Act
        var path = service.BuildDefaultPdfPath(new PdfReportName("Sprint report"), generatedAt);

        // Assert
        Path.GetFileName(path.Value).Should().Be("Sprint_report_20260228_181030.pdf");
    }

    [Fact(DisplayName = "BuildReport throws when issues are null")]
    [Trait("Category", "Unit")]
    public void BuildReportWhenIssuesAreNullThrowsArgumentNullException()
    {
        // Arrange
        var service = new JiraLogicService();
        IReadOnlyList<JiraIssue> issues = null!;

        // Act
        Action act = () => _ = service.BuildReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            issues,
            [new IssueFieldName("status")]);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "BuildReport groups configured count fields and uses unknown fallback")]
    [Trait("Category", "Unit")]
    public void BuildReportWhenCountFieldsAreConfiguredBuildsGroupedTables()
    {
        // Arrange
        var service = new JiraLogicService();
        var before = DateTimeOffset.Now;
        var issues = new[]
        {
            new JiraIssue(
                new IssueKey("APP-1"),
                new Dictionary<IssueKey, FieldValue>
                {
                    [new IssueKey("status")] = new FieldValue("Open")
                },
                new Dictionary<IssueKey, IReadOnlyList<FieldValue>>
                {
                    [new IssueKey("labels")] = [new FieldValue("Backend"), new FieldValue("API"), new FieldValue("backend")]
                }),
            new JiraIssue(
                new IssueKey("APP-2"),
                new Dictionary<IssueKey, FieldValue>
                {
                    [new IssueKey("status")] = FieldValue.Missing
                })
        };

        // Act
        var report = service.BuildReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            issues,
            [new IssueFieldName("status"), new IssueFieldName("labels")]);
        var after = DateTimeOffset.Now;

        // Assert
        report.CountTables.Should().HaveCount(2);
        report.CountTables[0].Title.Should().Be("By Status");
        report.CountTables[0].Rows.Select(static row => row.Name).Should().ContainInOrder("Open", "Unknown");
        report.CountTables[1].Title.Should().Be("By Labels");
        report.CountTables[1].Rows.Select(static row => row.Name).Should().ContainInOrder("API", "Backend", "Unknown");
        report.GeneratedAt.Should().BeOnOrAfter(before);
        report.GeneratedAt.Should().BeOnOrBefore(after);
    }

    [Fact(DisplayName = "BuildReport applies configured count field aliases")]
    [Trait("Category", "Unit")]
    public void BuildReportWhenCountAliasesAreConfiguredUsesAliasTitles()
    {
        // Arrange
        var service = new JiraLogicService();
        var issues = new[]
        {
            new JiraIssue(
                new IssueKey("APP-1"),
                new Dictionary<IssueKey, FieldValue>
                {
                    [new IssueKey("customfield_11868")] = new FieldValue("Football")
                })
        };

        // Act
        var report = service.BuildReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            issues,
            [new IssueFieldName("customfield_11868")],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["customfield_11868"] = "Sport"
            });

        // Assert
        report.CountTables.Should().ContainSingle();
        report.CountTables[0].Title.Should().Be("By Sport");
    }

    [Fact(DisplayName = "BuildFlowPathGroups groups matching paths and calculates median status duration")]
    [Trait("Category", "Unit")]
    public void BuildFlowPathGroupsWhenPathsMatchCalculatesMedianDuration()
    {
        // Arrange
        var service = new JiraLogicService();
        var at = new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero);
        var flows = new[]
        {
            new IssueFlow(
                new IssueKey("QA-1"),
                [
                    new FlowTransition("To Do", "In Progress", at, TimeSpan.FromHours(2)),
                    new FlowTransition("In Progress", "Done", at.AddHours(8), TimeSpan.FromHours(6))
                ]),
            new IssueFlow(
                new IssueKey("QA-2"),
                [
                    new FlowTransition("To Do", "In Progress", at, TimeSpan.FromHours(4)),
                    new FlowTransition("In Progress", "Done", at.AddHours(12), TimeSpan.FromHours(8))
                ])
        };

        // Act
        var groups = service.BuildFlowPathGroups(flows);

        // Assert
        groups.Should().ContainSingle();
        groups[0].Path.Should().Be("To Do -> In Progress -> Done");
        groups[0].Issues.Select(static key => key.Value).Should().ContainInOrder("QA-1", "QA-2");
        groups[0].Stages[0].MedianDuration.Should().Be(TimeSpan.FromHours(3));
        groups[0].Stages[1].MedianDuration.Should().Be(TimeSpan.FromHours(7));
    }
}
