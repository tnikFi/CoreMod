using Domain.Enums;

namespace Domain.Attributes;

/// <summary>
///     Specifies that a moderation type is a pardon of some other moderation type
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PardonAttribute : Attribute
{
    public PardonAttribute(ModerationType pardonedType)
    {
        PardonedType = pardonedType;
    }

    public ModerationType PardonedType { get; }
}