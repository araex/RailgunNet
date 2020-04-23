﻿using Moq;
using Railgun;
using Xunit;

namespace Tests
{
    public class RailPoolTest
    {
        public class A : IRailPoolable<A>
        {
            IRailPool<A> IRailPoolable<A>.Pool { get; set; }
            void IRailPoolable<A>.Reset() { }
        }

        public class B : A
        {

        }

        private readonly Mock<IRailFactory<A>> factoryMock;

        public RailPoolTest()
        {
            factoryMock = new Mock<IRailFactory<A>>();
        }

        [Fact]
        public void AllocateCallsFactory()
        {
            A instance = new A();
            factoryMock.Setup(f => f.Create()).Returns(instance);

            var pool = new RailPool<A>(factoryMock.Object);
            var allocatedObject = pool.Allocate();
            factoryMock.Verify(f => f.Create(), Times.Once);
            Assert.Same(instance, allocatedObject);
        }
        [Fact]
        public void PoolReusesInstances()
        {
            factoryMock.Setup(f => f.Create()).Returns(new A());
            var pool = new RailPool<A>(factoryMock.Object);
            var firstObject = pool.Allocate();
            factoryMock.Verify(f => f.Create(), Times.Once);

            pool.Deallocate(firstObject);
            var secondObject = pool.Allocate();

            factoryMock.Verify(f => f.Create(), Times.Once);
            Assert.Same(firstObject, secondObject);
        }
    }
}