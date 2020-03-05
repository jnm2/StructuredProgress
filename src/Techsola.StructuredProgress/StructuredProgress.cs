using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Techsola
{
    public sealed partial class StructuredProgress
    {
        private readonly IProgress<StructuredReport> sink;

        // Because I'd like the calls to IProgress<>.Report to happen synchronously, ensuring that updates are sent to
        // it in order requires the use of a lock. A lock-free implementation is possible but requires updates to be
        // queued and IProgress<>.Report to be called asynchronously.
        private readonly object reportLock = new object();

        private string message;
        private double completedSize;
        private double completedSizeAfterCurrentJob;
        private double totalSize;
        private readonly List<Subtask> subtasks = new List<Subtask>();
        private ImmutableList<StructuredReport> subtaskReports = ImmutableList<StructuredReport>.Empty;

        public StructuredProgress(IProgress<StructuredReport> sink, string initialJobMessage, double initialJobSize = 1)
        {
            if (string.IsNullOrWhiteSpace(initialJobMessage))
                throw new ArgumentException("A message must be specified.", nameof(initialJobMessage));

            ValidateJobSize(initialJobSize, nameof(initialJobSize), "Initial job size");

            this.sink = sink ?? throw new ArgumentNullException(nameof(sink));
            message = initialJobMessage;
            completedSizeAfterCurrentJob = initialJobSize;
            totalSize = initialJobSize;

            Report();
        }

        public void AddJobSize(double additionalJobSize)
        {
            ValidateJobSize(additionalJobSize, nameof(additionalJobSize), "Additional job size");

            lock (reportLock)
            {
                CheckCompletion();

                totalSize += additionalJobSize;

                if (completedSize != 0) Report();
            }
        }

        public void Next(string nextJobMessage, double nextJobSize = 1)
        {
            if (string.IsNullOrWhiteSpace(nextJobMessage))
                throw new ArgumentException("A message must be specified.", nameof(nextJobMessage));

            ValidateJobSize(nextJobSize, nameof(nextJobSize), "Job size");

            lock (reportLock)
            {
                CheckCompletion();

                var amountRemaining = totalSize - completedSizeAfterCurrentJob;
                if (amountRemaining < nextJobSize)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(nextJobSize),
                        nextJobSize,
                        $"The next job size ({nextJobSize}) is greater than the remaining size ({amountRemaining}). Use {nameof(AddJobSize)} to increase the remaining size.");
                }

                completedSize = completedSizeAfterCurrentJob;
                completedSizeAfterCurrentJob += nextJobSize;
                message = nextJobMessage;

                Report();
            }
        }

        public IProgress<StructuredReport> CreateSubprogress(double jobSize = 1)
        {
            ValidateJobSize(jobSize, nameof(jobSize), "Job size");

            lock (reportLock)
            {
                CheckCompletion();

                var amountRemaining = completedSizeAfterCurrentJob - completedSize - subtasks.Sum(t => t.JobSize);
                if (amountRemaining < jobSize)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(jobSize),
                        jobSize,
                        $"The subprogress job size ({jobSize}) is greater than the size remaining after the last call to {nameof(Next)} ({amountRemaining}).");
                }

                var subtask = new Subtask(this, jobSize);
                subtasks.Add(subtask);
                return subtask;
            }
        }

        private void OnSubprogressReport(Subtask subtask, StructuredReport value)
        {
            lock (reportLock)
            {
                CheckCompletion();

                var index = subtasks.TakeWhile(t => t != subtask).Count(t => t.LastReport is { });

                if (value.Fraction < 1)
                {
                    if (value.Equals(subtask.LastReport)) return;

                    subtaskReports = subtask.LastReport is null
                        ? subtaskReports.Insert(index, value)
                        : subtaskReports.SetItem(index, value);

                    subtask.LastReport = value;
                }
                else
                {
                    subtask.Disassociate();
                    subtasks.Remove(subtask);
                    completedSize += subtask.JobSize;

                    if (subtask.LastReport is { })
                        subtaskReports = subtaskReports.RemoveAt(index);
                }

                Report();
            }
        }

        public void Complete(string message = "Successfully completed.")
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("A message must be specified.", nameof(message));

            lock (reportLock)
            {
                CheckCompletion();

                if (totalSize == 0) totalSize = 1;
                completedSize = totalSize;

                this.message = message;

                Report();
            }
        }

        private void Report()
        {
            var completedIncludingSubtasks = completedSize + (subtasks.Sum(t => t.LastReport?.Fraction * t.JobSize) ?? 0);

            sink?.Report(new StructuredReport(
                totalSize > 0 ? completedIncludingSubtasks / totalSize : 0,
                message,
                subtaskReports));
        }

        private void CheckCompletion()
        {
            if (0 < totalSize && totalSize <= completedSize)
                throw new InvalidOperationException("Progress has already been reported as complete.");
        }

        private static void ValidateJobSize(double jobSize, string paramName, string subject)
        {
            if (jobSize < 0)
                throw new ArgumentOutOfRangeException(paramName, jobSize, subject + " must not be negative.");

            if (double.IsInfinity(jobSize))
                throw new ArgumentOutOfRangeException(paramName, jobSize, subject + " must not be infinite.");

            if (double.IsNaN(jobSize))
                throw new ArgumentOutOfRangeException(paramName, jobSize, subject + " must be a number.");
        }
    }
}
