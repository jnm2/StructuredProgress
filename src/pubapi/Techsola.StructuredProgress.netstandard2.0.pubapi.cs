// Name:       Techsola.StructuredProgress
// Public key: ACQAAASAAACUAAAABgIAAAAkAABSU0ExAAQAAAEAAQAFBBEqA2UBdU94ikWT0bnvWDIjUzt/HJbKZTruN8XgSYYb5p3/BQmA0A4vlFhmdIWfKHHuppCpllnO0Ya1XiA9f/sMoqpWaJE3FwUXYDT6F4hhRO+eQxsQoB/E/RkGg/u2n9ft8eJ+Dqqxea44tlQA/SeW1R4TkEIvHt6K2trJ5g==

namespace Techsola
{
    [System.Runtime.CompilerServices.NullableContext(1)]
    [System.Runtime.CompilerServices.Nullable(0)]
    public sealed class StructuredProgress
    {
        public StructuredProgress(System.IProgress<StructuredReport> sink, string initialJobMessage, double initialJobSize = 1);

        public void AddJobSize(double additionalJobSize);

        public void Complete(string message = "Successfully completed.");

        public System.IProgress<StructuredReport> CreateSubprogress(double jobSize = 1);

        public void Next(string nextJobMessage, double nextJobSize = 1);
    }

    public static class StructuredProgressExtensions
    {
        [System.Runtime.CompilerServices.NullableContext(1)]
        public static StructuredProgress Start(this System.IProgress<StructuredReport> progress, string initialMessage, double initialJobSize = 1);
    }

    [System.Runtime.CompilerServices.NullableContext(1)]
    [System.Runtime.CompilerServices.Nullable(0)]
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public sealed class StructuredReport : System.IEquatable<StructuredReport>
    {
        public double Fraction { get; }

        public string Message { get; }

        public System.Collections.Immutable.ImmutableList<StructuredReport> Subtasks { get; }

        public StructuredReport(double fraction, string message, [System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })] System.Collections.Immutable.ImmutableList<StructuredReport> subtasks = default);

        [System.Runtime.CompilerServices.NullableContext(2)]
        public override bool Equals(object obj);

        [System.Runtime.CompilerServices.NullableContext(2)]
        public bool Equals(StructuredReport other);

        public override int GetHashCode();

        public override string ToString();
    }
}
