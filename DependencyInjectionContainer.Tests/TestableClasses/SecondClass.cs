namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class SecondClass
    {
        public ThirdClass ThirdClass { get; }

        public SecondClass(ThirdClass thirdClass)
        {
            ThirdClass = thirdClass;
        }
    }
}