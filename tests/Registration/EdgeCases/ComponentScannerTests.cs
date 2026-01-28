using System.Reflection;
using Autofac;
using NUnit.Framework;

namespace Csanno.Tests
{

    [TestFixture]
    public class ComponentScannerTests
    {
        [Test]
        public void ComponentScanner_Should_Handle_ReflectionTypeLoadException()
        {
            // Arrange
            var scannerType = typeof(RegistrationExtensions).Assembly.GetType("Csanno.Internal.ComponentScanner");
            var scanMethod = scannerType?.GetMethod("Scan", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(scanMethod, Is.Not.Null, "ComponentScanner.Scan 方法应存在");

            var assemblies = new[] { new ThrowingAssembly() };

            // Act
            var result = scanMethod!.Invoke(null, new object[] { assemblies });

            // Assert
            Assert.That(result, Is.Not.Null);
            var componentTypes = new List<Type>();
            foreach (var item in (System.Collections.IEnumerable)result!)
            {
                var componentTypeProp = item.GetType().GetProperty("ComponentType");
                var componentType = (Type?)componentTypeProp?.GetValue(item);
                if (componentType != null)
                {
                    componentTypes.Add(componentType);
                }
            }

            Assert.That(componentTypes, Does.Contain(typeof(Csanno.Tests.Registration.Lifetime.SimpleComponent)));
        }

        private sealed class ThrowingAssembly : Assembly
        {
            public override string FullName => "ThrowingAssembly";

            public override Type[] GetTypes()
            {
                var types = new Type?[]
                {
                    typeof(Csanno.Tests.Registration.Lifetime.SimpleComponent),
                    null
                };
                var exceptions = new Exception[]
                {
                    new TypeLoadException("Type load failed"),
                    new TypeLoadException("Type load failed")
                };
                throw new ReflectionTypeLoadException(types, exceptions);
            }
        }
    }
}
