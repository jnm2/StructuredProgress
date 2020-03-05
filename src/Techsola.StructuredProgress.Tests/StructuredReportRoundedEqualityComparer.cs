using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Techsola
{
    internal sealed class StructuredReportRoundedEqualityComparer : IEqualityComparer<StructuredReport>
    {
        public static StructuredReportRoundedEqualityComparer Instance { get; } = new StructuredReportRoundedEqualityComparer();

        private StructuredReportRoundedEqualityComparer()
        {
        }

        private static double Round(double fraction)
        {
            return Math.Round(fraction, 14);
        }

        public bool Equals(StructuredReport? x, StructuredReport? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return Round(x.Fraction) == Round(y.Fraction)
                   && x.Message == y.Message
                   && x.Subtasks.SequenceEqual(y.Subtasks, this);
        }

        public int GetHashCode(StructuredReport obj)
        {
            return HashCode.Combine(
                Round(obj.Fraction),
                obj.Message,
                obj.Subtasks.Count);
        }
    }
}
