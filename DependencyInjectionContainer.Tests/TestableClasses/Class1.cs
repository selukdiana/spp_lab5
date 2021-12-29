using DependencyInjectionContainer.Attribute;

namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class Class1
    {
        public Class2 class2 { get; }

        public Class1(Class2 class2) {
            this.class2 = class2;
        }
    }
}