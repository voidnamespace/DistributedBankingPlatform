using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Domain.Entities;

public class Segment
{
    private Segment()
    {
    }

    public Segment(
        Guid id,
        string name,
        SegmentRuleType ruleType,
        SegmentKind kind)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Segment id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Segment name cannot be empty.", nameof(name));

        Id = id;
        Name = name;
        RuleType = ruleType;
        Kind = kind;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public SegmentRuleType RuleType { get; private set; }

    public SegmentKind Kind { get; private set; }
}
