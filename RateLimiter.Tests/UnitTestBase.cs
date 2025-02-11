using AutoFixture;

using Moq.AutoMock;

namespace RateLimiter.Tests
{
    public abstract class UnitTestBase<T>
    {
        public AutoMocker Mocker { get; }

        public Fixture Fixture { get; }

        internal UnitTestBase()
        {
            Mocker = new AutoMocker();
            Fixture = new Fixture();
        }
    }
}
