using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Csanno.Generator.Models;

namespace Csanno.Generator;

internal static class SymbolAnalysis
{
    private static readonly string[] LifetimeAttributeFullNames =
    [
        "Csanno.Attributes.SingletonAttribute",
        "Csanno.Attributes.ScopedAttribute",
        "Csanno.Attributes.TransientAttribute",
        "Csanno.Attributes.PerRequestAttribute",
        "Csanno.Attributes.PerMatchingLifetimeScopeAttribute",
        "Csanno.Attributes.OwnedAttribute"
    ];

    public static InstanceLifetime ResolveLifetime(
        INamedTypeSymbol classSymbol,
        out string[]? tags,
        out string? ownedType)
    {
        tags = null;
        ownedType = null;

        var attributes = classSymbol.GetAttributes();
        var attrFullNames = new HashSet<string>(
            attributes
                .Select(a => a.AttributeClass?.ToDisplayString())
                .Where(n => n != null)!);

        if (attrFullNames.Contains("Csanno.Attributes.SingletonAttribute"))
        {
            return InstanceLifetime.Singleton;
        }

        var perMatching = attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == "Csanno.Attributes.PerMatchingLifetimeScopeAttribute");
        if (perMatching != null)
        {
            tags = perMatching.ConstructorArguments
                .SelectMany(a => a.Values)
                .Select(v => v.Value?.ToString())
                .OfType<string>()
                .ToArray();
            return InstanceLifetime.PerMatchingLifetimeScope;
        }

        if (attrFullNames.Contains("Csanno.Attributes.ScopedAttribute"))
        {
            return InstanceLifetime.Scoped;
        }

        if (attrFullNames.Contains("Csanno.Attributes.PerRequestAttribute"))
        {
            return InstanceLifetime.PerRequest;
        }

        var ownedAttr = attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == "Csanno.Attributes.OwnedAttribute");
        if (ownedAttr != null)
        {
            var ownedTypeProp = ownedAttr.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "OwnedType");
            if (ownedTypeProp.Value.Value is INamedTypeSymbol typeSymbol)
            {
                ownedType = typeSymbol.ToDisplayString();
            }
            return InstanceLifetime.Owned;
        }

        if (attrFullNames.Contains("Csanno.Attributes.TransientAttribute"))
        {
            return InstanceLifetime.Transient;
        }

        return InstanceLifetime.Transient;
    }

    public static List<ServiceInfo> ResolveServiceTypes(INamedTypeSymbol classSymbol)
    {
        var services = new List<ServiceInfo>();

        var asServiceAttrs = classSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == "Csanno.Attributes.AsServiceAttribute");

        foreach (var attr in asServiceAttrs)
        {
            var serviceType = attr.ConstructorArguments.FirstOrDefault().Value;
            if (serviceType is INamedTypeSymbol typeSymbol)
            {
                services.Add(new ServiceInfo
                {
                    ServiceType = typeSymbol.ToDisplayString(),
                    IsSelf = SymbolEqualityComparer.Default.Equals(typeSymbol, classSymbol)
                });
            }
        }

        var componentServiceTypes = GetComponentAttributes(classSymbol)
            .SelectMany(attr => attr.NamedArguments)
            .Where(kvp => kvp.Key == "ServiceType")
            .Select(kvp => kvp.Value.Value)
            .OfType<INamedTypeSymbol>()
            .Distinct(SymbolEqualityComparer.Default)
            .ToList();

        if (componentServiceTypes.Count > 0)
        {
            services.Clear();
            foreach (var typeSymbol in componentServiceTypes)
            {
                var serviceTypeName = typeSymbol?.ToDisplayString();
                if (serviceTypeName is null)
                {
                    continue;
                }

                services.Add(new ServiceInfo
                {
                    ServiceType = serviceTypeName,
                    IsSelf = SymbolEqualityComparer.Default.Equals(typeSymbol, classSymbol)
                });
            }
            return services;
        }

        if (services.Count == 0)
        {
            services.Add(new ServiceInfo
            {
                ServiceType = classSymbol.ToDisplayString(),
                IsSelf = true
            });
        }

        return services;
    }

    public static List<MetadataInfo> ResolveMetadata(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == "Csanno.Attributes.WithMetadataAttribute")
            .Select(attr =>
            {
                var key = attr.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
                var value = attr.ConstructorArguments[1];

                return new MetadataInfo
                {
                    Key = key,
                    ValueExpression = ConvertToAotFriendlyExpression(value)
                };
            })
            .ToList();
    }

    public static bool HasComponentAttribute(INamedTypeSymbol classSymbol)
    {
        if (HasComponentAttributeDirect(classSymbol))
        {
            return true;
        }

        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            if (HasComponentAttributeDirect(baseType))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }

    public static bool HasComponentAttributeDirect(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(a => IsComponentAttribute(a.AttributeClass));
    }

    public static IEnumerable<AttributeData> GetComponentAttributes(INamedTypeSymbol classSymbol)
    {
        foreach (var attr in classSymbol.GetAttributes())
        {
            if (IsComponentAttribute(attr.AttributeClass))
            {
                yield return attr;
            }
        }

        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            foreach (var attr in baseType.GetAttributes())
            {
                if (IsComponentAttribute(attr.AttributeClass))
                {
                    yield return attr;
                }
            }
            baseType = baseType.BaseType;
        }
    }

    public static bool IsComponentAttribute(INamedTypeSymbol? attributeType)
    {
        var current = attributeType;
        while (current is not null)
        {
            if (current.Name == "ComponentAttribute" &&
                (current.ToDisplayString() == "Csanno.Attributes.ComponentAttribute" ||
                 current.ContainingNamespace.ToDisplayString() == "Csanno.Attributes"))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }

    public static string ConvertToAotFriendlyExpression(TypedConstant value)
    {
        if (value.Value is null)
        {
            return "null";
        }

        if (value.Type?.TypeKind == TypeKind.Enum)
        {
            return $"({value.Type.ToDisplayString()}){Convert.ToString(value.Value, CultureInfo.InvariantCulture)}";
        }

        if (value.Value is string s)
        {
            return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(s, true);
        }

        if (value.Value is char c)
        {
            return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(c, true);
        }

        if (value.Value is bool b)
        {
            return b ? "true" : "false";
        }

        if (value.Value is int i)
        {
            return i.ToString();
        }

        if (value.Value is long l)
        {
            return l.ToString(CultureInfo.InvariantCulture) + "L";
        }

        if (value.Value is double d)
        {
            return d.ToString("R", CultureInfo.InvariantCulture) + "D";
        }

        if (value.Value is float f)
        {
            return f.ToString("R", CultureInfo.InvariantCulture) + "F";
        }

        if (value.Value is decimal m)
        {
            return m.ToString(CultureInfo.InvariantCulture) + "m";
        }

        if (value.Value is INamedTypeSymbol type)
        {
            return $"typeof({type.ToDisplayString()})";
        }

        return "null";
    }
}
