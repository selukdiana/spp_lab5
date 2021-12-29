namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class ThirdClass
    {
        public FirstClass FirstClass { get; }

        public ThirdClass(FirstClass firstClass)
        {
            FirstClass = firstClass;
        }
    }
}