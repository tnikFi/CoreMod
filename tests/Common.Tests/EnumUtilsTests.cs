using Common.Utils;

namespace Common.Tests;

[AttributeUsage(AttributeTargets.Field)]
internal class SampleAttribute : Attribute
{
    public int Value { get; set; }
    
    public SampleAttribute(int value)
    {
        Value = value;
    }
}

[AttributeUsage(AttributeTargets.Field)]
// ReSharper disable once ClassNeverInstantiated.Global
internal class UnusedAttribute : Attribute
{
}

[TestFixture]
public class EnumUtilsTests
{
    private enum TestEnum : byte
    {
        [Sample(1)]
        Option1,
        Option2,
        [Sample(3)]
        Option3
    }
    
    [Test]
    public void HasAttribute_ReturnsTrue_WhenAttributeExists()
    {
        var result = EnumUtils.HasAttribute<SampleAttribute>(TestEnum.Option1);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void HasAttribute_ReturnsFalse_WhenAttributeDoesNotExist()
    {
        var result = EnumUtils.HasAttribute<SampleAttribute>(TestEnum.Option2);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GetAttributeValue_ReturnsAttribute_WhenAttributeExists()
    {
        var result = EnumUtils.GetAttributeValue<SampleAttribute>(TestEnum.Option1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value, Is.EqualTo(1));
    }
    
    [Test]
    public void GetAttributeValue_ReturnsNull_WhenAttributeDoesNotExist()
    {
        var result = EnumUtils.GetAttributeValue<SampleAttribute>(TestEnum.Option2);
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public void GetValuesWithAttribute_ReturnsValues_WhenAttributeExists()
    {
        var result = EnumUtils.GetValuesWithAttribute<TestEnum, SampleAttribute>().ToArray();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EquivalentTo(new[] { TestEnum.Option1, TestEnum.Option3 }));
    }
    
    [Test]
    public void GetValuesWithAttribute_ReturnsEmpty_WhenAttributeDoesNotExist()
    {
        var result = EnumUtils.GetValuesWithAttribute<TestEnum, UnusedAttribute>().ToArray();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
}