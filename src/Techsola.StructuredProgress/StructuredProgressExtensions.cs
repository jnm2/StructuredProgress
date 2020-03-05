using System;

namespace Techsola
{
    public static class StructuredProgressExtensions
    {
        public static StructuredProgress Start(this IProgress<StructuredReport> progress, string initialMessage, double initialJobSize = 1)
        {
            return new StructuredProgress(progress, initialMessage, initialJobSize);
        }
    }
}
