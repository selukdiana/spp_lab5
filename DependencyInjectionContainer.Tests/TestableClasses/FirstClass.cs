namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class FirstClass
    {
        public SecondClass SecondClass { get; }

        public FirstClass(SecondClass secondClass)
        {
            SecondClass = secondClass;
        }
    }
}