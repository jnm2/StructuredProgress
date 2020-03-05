using System;

namespace Techsola
{
    partial class StructuredProgress
    {
        private sealed class Subtask : IProgress<StructuredReport>
        {
            private StructuredProgress? parent;

            public Subtask(StructuredProgress parent, double jobSize)
            {
                this.parent = parent;
                JobSize = jobSize;
            }

            public double JobSize { get; }
            public StructuredReport? LastReport { get; set; }

            public void Disassociate() => parent = null;

            public void Report(StructuredReport value)
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                parent?.OnSubprogressReport(this, value);
            }
        }
    }
}
