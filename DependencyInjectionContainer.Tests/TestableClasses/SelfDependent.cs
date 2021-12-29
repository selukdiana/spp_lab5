using DependencyInjectionContainer.Attribute;

namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class SelfDependent
    {
        public SelfDependent(SelfDependent self) {}
    }
}