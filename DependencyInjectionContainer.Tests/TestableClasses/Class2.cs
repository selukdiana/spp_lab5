using DependencyInjectionContainer.Attribute;

namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class Class2
    {
       public Class1 class1 { get; }
        public Class2(Class1 class1) {
            this.class1 = class1;
        }
    }
}