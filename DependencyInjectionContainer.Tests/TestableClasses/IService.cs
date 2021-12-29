namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public interface IService<out T> : IBaseService where T: IRepository
    {

    }
}