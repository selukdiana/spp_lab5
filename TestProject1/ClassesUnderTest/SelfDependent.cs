using DependencyInjectionContainer.CustomAttributes;

namespace TestDemo.ClassesUnderTest
{
    public class SelfDependent
    {
        public SelfDependent sd;
        public SelfDependent(SelfDependent self) { this.sd = self; }
    }
}