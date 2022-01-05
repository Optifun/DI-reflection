using System;
using System.Linq;
using System.Reflection;

namespace DIReflection.Runtime
{
  internal static class ReflectionExtensions
  {
    public static bool HasAttributeInMethod(this Type type, Type attribute) =>
      type
        .GetMethods()
        .Any(method => method.HasAttribute(attribute));

    public static bool HasAttributeInMethod(this Type type, Type attribute, BindingFlags flags) =>
      type
        .GetMethods(flags)
        .Any(method => method.HasAttribute(attribute));

    public static bool HasAttributeInConstructor(this Type type, Type attribute) =>
      type
        .GetConstructors()
        .Any(ctor => HasAttribute(ctor, attribute));

    public static bool HasAttributeInConstructor(this Type type, Type attribute, BindingFlags flags) =>
      type
        .GetConstructors(flags)
        .Any(ctor => HasAttribute(ctor, attribute));

    public static bool HasAttribute(this MemberInfo member, Type attribute)
    {
      var attributes = member.GetCustomAttributes().ToList();
      if (!attributes.Any())
        return false;

      return attributes.Any(att => att.GetType() == attribute);
    }
  }
}