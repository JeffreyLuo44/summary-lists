using System.Reflection;
using System.Runtime.CompilerServices;
using SummaryLists.Mobile.Extensions;

namespace SummaryLists.Mobile.Tests.Extensions;

public class MauiPropertyExtensionsTests
{
    [Theory]
    [InlineData("Text", typeof(Label), typeof(string))]
    [InlineData("FontSize", typeof(Label), typeof(double))]
    [InlineData("Bold", typeof(Label))]
    [InlineData("TextColor", typeof(Label), typeof(Color))]
    [InlineData("CenterVertical", typeof(Label))]
    [InlineData("Text", typeof(Button), typeof(string))]
    [InlineData("BackgroundColor", typeof(Button), typeof(Color))]
    [InlineData("TextColor", typeof(Button), typeof(Color))]
    [InlineData("Placeholder", typeof(Entry), typeof(string))]
    [InlineData("Keyboard", typeof(Entry), typeof(Keyboard))]
    [InlineData("Password", typeof(Entry), typeof(bool))]
    [InlineData("Padding", typeof(VerticalStackLayout), typeof(double))]
    [InlineData("Spacing", typeof(VerticalStackLayout), typeof(double))]
    [InlineData("ColumnSpacing", typeof(Grid), typeof(double))]
    public void FluentExtension_ExistsWithExpectedSignature(
        string methodName,
        Type firstParameterType,
        Type? secondParameterType = null)
    {
        var method = FindExtensionMethod(methodName, firstParameterType, secondParameterType);

        Assert.NotNull(method);
        Assert.True(method!.IsDefined(typeof(ExtensionAttribute), inherit: false));
        Assert.Equal(firstParameterType, method.GetParameters()[0].ParameterType);
        Assert.Equal(firstParameterType, method.ReturnType);
    }

    [Fact]
    public void BindExtension_IsGenericAndReturnsInputType()
    {
        var bindMethod = typeof(MauiPropertyExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(method => method.Name == "Bind" && method.IsGenericMethodDefinition);

        Assert.NotNull(bindMethod);
        Assert.True(bindMethod!.IsDefined(typeof(ExtensionAttribute), inherit: false));

        var genericType = bindMethod.GetGenericArguments().Single();
        Assert.Equal(genericType, bindMethod.ReturnType);

        var parameters = bindMethod.GetParameters();
        Assert.Equal(5, parameters.Length);
        Assert.Equal(typeof(BindableProperty), parameters[1].ParameterType);
        Assert.Equal(typeof(string), parameters[2].ParameterType);
        Assert.Equal(typeof(BindingMode), parameters[3].ParameterType);
        Assert.Equal(typeof(object), parameters[4].ParameterType);
    }

    [Fact]
    public void ColumnExtension_IsGenericForViewTypes()
    {
        var columnMethod = typeof(MauiPropertyExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(method => method.Name == "Column" && method.IsGenericMethodDefinition);

        Assert.NotNull(columnMethod);
        Assert.True(columnMethod!.IsDefined(typeof(ExtensionAttribute), inherit: false));

        var genericType = columnMethod.GetGenericArguments().Single();
        Assert.Equal(genericType, columnMethod.ReturnType);

        var genericConstraints = genericType.GetGenericParameterConstraints();
        Assert.Contains(typeof(View), genericConstraints);

        var parameters = columnMethod.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(genericType, parameters[0].ParameterType);
        Assert.Equal(typeof(int), parameters[1].ParameterType);
    }

    private static MethodInfo? FindExtensionMethod(string name, Type firstParameterType, Type? secondParameterType)
    {
        var candidates = typeof(MauiPropertyExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == name)
            .Where(method =>
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0 || parameters[0].ParameterType != firstParameterType)
                {
                    return false;
                }

                if (secondParameterType is null)
                {
                    return parameters.Length == 1;
                }

                return parameters.Length == 2 && parameters[1].ParameterType == secondParameterType;
            });

        return candidates.SingleOrDefault();
    }
}
