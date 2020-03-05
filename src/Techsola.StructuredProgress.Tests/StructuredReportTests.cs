using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Immutable;

namespace Techsola
{
    public static class StructuredReportTests
    {
        [Test]
        public static void ToString_shows_subtask_structure()
        {
            var report =
                new StructuredReport(0, "Root", ImmutableList.Create(
                    new StructuredReport(0, "A", ImmutableList.Create(
                        new StructuredReport(0, "A.A"),
                        new StructuredReport(0, "A.B"))),
                    new StructuredReport(0, "B", ImmutableList.Create(
                        new StructuredReport(0, "B.A"),
                        new StructuredReport(0, "B.B")))));

            report.ToString().ShouldBe(WithoutInitialEmptyLine(@"
0.0% – Root
 ├─ 0.0% – A
 │   ├─ 0.0% – A.A
 │   └─ 0.0% – A.B
 └─ 0.0% – B
     ├─ 0.0% – B.A
     └─ 0.0% – B.B"));
        }

        private static string WithoutInitialEmptyLine(string value)
        {
            if (value.StartsWith('\n')) return value.Substring(1);

            if (value.StartsWith("\r\n", StringComparison.Ordinal)) return value.Substring(2);

            throw new ArgumentException("The specified value must have an initial empty line.", nameof(value));
        }
    }
}
