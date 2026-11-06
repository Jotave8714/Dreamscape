using Dreamscape.Routes;
using NUnit.Framework;
using Node = Dreamscape.Routes.PowerRouteSearch.Node;
using Edge = Dreamscape.Routes.PowerRouteSearch.Connection;

namespace Dreamscape.Tests
{
    public sealed class PowerRouteSearchTests
    {
        [Test]
        public void ReachingTheJumpWithoutThePickupDoesNotUnlockIt()
        {
            var nodes = new[] { new Node("start"), new Node("pickup", grantsDoubleJump: true), new Node("jump"), new Node("goal") };
            var edges = new[] { new Edge(0, 2), new Edge(0, 1), new Edge(1, 2), new Edge(2, 3, true) };
            var result = PowerRouteSearch.FindPath(nodes, edges, 0, 3);
            Assert.That(result.Found, Is.True);
            Assert.That(result.Path, Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void SearchCanRevisitTheSamePointAfterCollectingPower()
        {
            var nodes = new[] { new Node("start"), new Node("pickup", grantsDoubleJump: true), new Node("goal") };
            var edges = new[] { new Edge(0, 2, true), new Edge(0, 1), new Edge(1, 0) };
            var result = PowerRouteSearch.FindPath(nodes, edges, 0, 2);
            Assert.That(result.Path, Is.EqualTo(new[] { 0, 1, 0, 2 }));
            Assert.That(result.VisitedStates, Is.LessThanOrEqualTo(nodes.Length * 2));
        }

        [Test]
        public void UnreachablePickupDoesNotGrantPower()
        {
            var nodes = new[] { new Node("start"), new Node("pickup", grantsDoubleJump: true), new Node("goal") };
            var result = PowerRouteSearch.FindPath(nodes, new[] { new Edge(0, 2, true) }, 0, 2);
            Assert.That(result.Found, Is.False);
            Assert.That(result.Error, Is.Null);
        }

        [Test]
        public void UnsafeFloorCannotBecomeAShortcutToTheGoal()
        {
            var nodes = new[] { new Node("start", grantsDoubleJump: true), new Node("trap", available: false), new Node("goal") };
            var result = PowerRouteSearch.FindPath(nodes, new[] { new Edge(0, 1), new Edge(1, 2) }, 0, 2);
            Assert.That(result.Found, Is.False);
        }

        [Test]
        public void BlockedConnectionRemainsBlockedWithPower()
        {
            var nodes = new[] { new Node("start", grantsDoubleJump: true), new Node("goal") };
            var result = PowerRouteSearch.FindPath(nodes, new[] { new Edge(0, 1, true, blocked: true) }, 0, 1);
            Assert.That(result.Found, Is.False);
        }

        [Test]
        public void CycleWithoutAnExitTerminatesWithNoRoute()
        {
            var nodes = new[] { new Node("start"), new Node("pickup", grantsDoubleJump: true), new Node("goal") };
            var edges = new[] { new Edge(0, 0), new Edge(0, 1), new Edge(1, 0), new Edge(1, 1) };
            var result = PowerRouteSearch.FindPath(nodes, edges, 0, 2);
            Assert.That(result.Found, Is.False);
            Assert.That(result.VisitedStates, Is.LessThanOrEqualTo(nodes.Length * 2));
        }

        [Test]
        public void BreadthFirstSearchReturnsTheShortestAvailableRoute()
        {
            var nodes = new[] { new Node("start"), new Node("detour"), new Node("goal") };
            var result = PowerRouteSearch.FindPath(nodes, new[] { new Edge(0, 1), new Edge(1, 2), new Edge(0, 2) }, 0, 2);
            Assert.That(result.Path, Is.EqualTo(new[] { 0, 2 }));
        }

        [Test]
        public void EntryAlreadyAtTheGoalHasASinglePointRoute()
        {
            var result = PowerRouteSearch.FindPath(new[] { new Node("start") }, new Edge[0], 0, 0);
            Assert.That(result.Path, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void UnavailableGoalHasNoRoute()
        {
            var nodes = new[] { new Node("start"), new Node("goal", available: false) };
            Assert.That(PowerRouteSearch.FindPath(nodes, new[] { new Edge(0, 1) }, 0, 1).Found, Is.False);
        }

        [TestCase(-1, 1)]
        [TestCase(0, 2)]
        public void InvalidEntryOrGoalIsReported(int start, int goal)
        {
            var result = PowerRouteSearch.FindPath(new[] { new Node("start"), new Node("goal") }, new Edge[0], start, goal);
            Assert.That(result.Found, Is.False);
            Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ConnectionToAnUnknownPointIsReported()
        {
            var result = PowerRouteSearch.FindPath(new[] { new Node("start") }, new[] { new Edge(0, 9) }, 0, 0);
            Assert.That(result.Found, Is.False);
            Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void DuplicatePointIdsAreReported()
        {
            var result = PowerRouteSearch.FindPath(new[] { new Node("same"), new Node("same") }, new[] { new Edge(0, 1) }, 0, 1);
            Assert.That(result.Found, Is.False);
            Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
        }
    }
}
