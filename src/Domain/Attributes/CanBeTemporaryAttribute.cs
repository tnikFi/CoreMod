namespace Domain.Attributes;

/// <summary>
///     Specifies that a moderation type can have an expiration date
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class CanBeTemporaryAttribute : Attribute { }