using System.Reflection;
using System.Reflection.Emit;
using Autofac;
using Csanno.Attributes;
using NUnit.Framework;

namespace Csanno.Tests
{

    [TestFixture]
    public class MixedRegistrationTests
    {
        [Test]
        public void RegisterComponents_Should_Fallback_To_Runtime_For_Assemblies_Without_Generated_Code()
        {
            // Arrange
            var dynamicType = CreateDynamicComponentType();
            var dynamicAssembly = dynamicType.Assembly;

            var builder = new ContainerBuilder();

            // Act
            builder.RegisterComponents(typeof(SourceGeneratorTests).Assembly, dynamicAssembly);
            var container = builder.Build();

            // Assert
            Assert.That(() => container.Resolve(dynamicType), Throws.Nothing,
                "未命中生成器的程序集应回退到运行时扫描并注册组件");
        }

        private static Type CreateDynamicComponentType()
        {
            var assemblyName = new AssemblyName("DynamicComponentsAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            var typeBuilder = moduleBuilder.DefineType(
                "DynamicComponents.DynamicComponent",
                TypeAttributes.Public | TypeAttributes.Class);

            var componentCtor = typeof(ComponentAttribute).GetConstructor(Type.EmptyTypes);
            var componentAttr = new CustomAttributeBuilder(componentCtor!, Array.Empty<object>());
            typeBuilder.SetCustomAttribute(componentAttr);

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            return typeBuilder.CreateTypeInfo()!.AsType();
        }
    }
}
