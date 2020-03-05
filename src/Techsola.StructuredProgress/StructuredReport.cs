using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Techsola
{
    [DebuggerDisplay("{ToString(),nq}")]
    public sealed class StructuredReport : IEquatable<StructuredReport?>
    {
        public StructuredReport(double fraction, string message, ImmutableList<StructuredReport>? subtasks = null)
        {
            if (fraction < 0 || 1 < fraction)
                throw new ArgumentOutOfRangeException(nameof(fraction), fraction, "Fraction must be between 0 and 1, inclusive.");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("A message must be specified.", nameof(message));

            Fraction = fraction;
            Message = message;
            Subtasks = subtasks ?? ImmutableList<StructuredReport>.Empty;
        }

        public double Fraction { get; }
        public string Message { get; }
        public ImmutableList<StructuredReport> Subtasks { get; }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as StructuredReport);
        }

        /// <inheritdoc/>
        public bool Equals(StructuredReport? other)
        {
            return other != null &&
                   Fraction == other.Fraction &&
                   Message == other.Message &&
                   Subtasks.SequenceEqual(other.Subtasks);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 735917300;
            hashCode = hashCode * -1521134295 + Fraction.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
            hashCode = hashCode * -1521134295 + Subtasks.Count.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            WriteTo(builder, childLinePrefix: string.Empty);
            return builder.ToString();
        }

        private void WriteTo(StringBuilder builder, string childLinePrefix)
        {
            builder.Append($"{Fraction:p1} – {Message}");

            foreach (var (index, task) in Subtasks.AsIndexed())
            {
                var isLast = index == Subtasks.Count - 1;

                builder.AppendLine();
                builder.Append(childLinePrefix);
                builder.Append(isLast ? " └─ " : " ├─ ");

                task.WriteTo(builder, childLinePrefix + (isLast ? "    " : " │  "));
            }
        }
    }
}
