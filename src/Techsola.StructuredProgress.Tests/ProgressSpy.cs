using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Techsola
{
    internal sealed class ProgressSpy : IProgress<StructuredReport>
    {
        private readonly ImmutableArray<StructuredReport>.Builder actual = ImmutableArray.CreateBuilder<StructuredReport>();

        void IProgress<StructuredReport>.Report(StructuredReport value)
        {
            lock (actual)
            {
                actual.Add(value);
            }
        }

        public ImmutableArray<StructuredReport> GetUpdatesAndClear()
        {
            lock (actual)
            {
                var updates = actual.ToImmutable();
                actual.Clear();
                return updates;
            }
        }

        public void AssertNoUpdates()
        {
            var actual = GetUpdatesAndClear();

            Assert.That(actual, Is.Empty);
        }

        public void Clear()
        {
            actual.Clear();
        }

        public void AssertSingleAndClear(StructuredReport expected)
        {
            var actual = GetUpdatesAndClear();

            Assert.That(actual, Has.One.Items);
            Assert.That(actual.Single(), Is.EqualTo(expected).Using(StructuredReportRoundedEqualityComparer.Instance));
        }

        public void AssertLastAndClear(StructuredReport expected)
        {
            var actual = GetUpdatesAndClear().TakeLast(1);

            Assert.That(actual, Has.Count.EqualTo(1));
            Assert.That(actual, Is.EqualTo(new[] { expected }).Using(StructuredReportRoundedEqualityComparer.Instance));
        }
    }
}
