using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Immutable;

namespace Techsola
{
    public static class StructuredProgressTests
    {
        private static StructuredReport Report(double fraction, string message, params StructuredReport[] subtasks)
        {
            return new StructuredReport(fraction, message, ImmutableList.CreateRange(subtasks));
        }

        private static StructuredReport Report(double numerator, double denominator, string message, params StructuredReport[] subtasks)
        {
            return Report(numerator / denominator, message, subtasks);
        }

        [Test]
        public static void Start_sends_zero_progress_and_initial_message()
        {
            var progress = new ProgressSpy();

            progress.Start("Gathering info");

            progress.AssertSingleAndClear(Report(0, "Gathering info"));
        }

        [Test]
        public static void Initial_job_size_must_not_be_negative()
        {
            var progress = new ProgressSpy();

            Should.Throw<ArgumentOutOfRangeException>(() => progress.Start("Gathering info", initialJobSize: -double.Epsilon))
                .ParamName.ShouldBe("initialJobSize");
        }

        [Test]
        public static void Initial_job_size_must_not_be_infinite()
        {
            var progress = new ProgressSpy();

            Should.Throw<ArgumentOutOfRangeException>(() => progress.Start("Gathering info", initialJobSize: double.PositiveInfinity))
                .ParamName.ShouldBe("initialJobSize");
        }

        [Test]
        public static void Initial_job_size_must_be_a_number()
        {
            var progress = new ProgressSpy();

            Should.Throw<ArgumentOutOfRangeException>(() => progress.Start("Gathering info", initialJobSize: double.NaN))
                .ParamName.ShouldBe("initialJobSize");
        }

        [Test]
        public static void Progress_may_be_completed_with_initial_job_size_zero()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info", initialJobSize: 0);
            progress.AssertSingleAndClear(Report(0, "Gathering info"));

            structuredProgress.Complete();
            progress.AssertSingleAndClear(Report(1, "Successfully completed."));
        }

        [Test]
        public static void Complete_may_run_before_finishing()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            progress.AssertSingleAndClear(Report(0, "Gathering info"));

            structuredProgress.Complete();
            progress.AssertSingleAndClear(Report(1, "Successfully completed."));
        }

        [Test]
        public static void Next_may_not_be_called_when_complete()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            structuredProgress.Complete();

            Should.Throw<InvalidOperationException>(() => structuredProgress.Next("Next"))
                .Message.ShouldBe("Progress has already been reported as complete.");
        }

        [Test]
        public static void AddJobSize_may_not_be_called_when_complete()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            structuredProgress.Complete();

            Should.Throw<InvalidOperationException>(() => structuredProgress.AddJobSize(0))
                .Message.ShouldBe("Progress has already been reported as complete.");
        }

        [Test]
        public static void CreateSubprogress_may_not_be_called_when_complete()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            structuredProgress.Complete();

            Should.Throw<InvalidOperationException>(() => structuredProgress.CreateSubprogress(1))
                .Message.ShouldBe("Progress has already been reported as complete.");
        }

        [Test]
        public static void Complete_may_not_be_called_when_complete()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            structuredProgress.Complete();

            Should.Throw<InvalidOperationException>(() => structuredProgress.Complete())
                .Message.ShouldBe("Progress has already been reported as complete.");
        }

        [Test]
        public static void Additional_job_size_must_not_be_negative()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");

            Should.Throw<ArgumentOutOfRangeException>(() => structuredProgress.AddJobSize(additionalJobSize: -double.Epsilon))
                .ParamName.ShouldBe("additionalJobSize");
        }

        [Test]
        public static void Additional_job_size_must_not_be_infinite()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");

            Should.Throw<ArgumentOutOfRangeException>(() => structuredProgress.AddJobSize(additionalJobSize: double.PositiveInfinity))
                .ParamName.ShouldBe("additionalJobSize");
        }

        [Test]
        public static void Additional_job_size_must_be_a_number()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");

            Should.Throw<ArgumentOutOfRangeException>(() => structuredProgress.AddJobSize(additionalJobSize: double.NaN))
                .ParamName.ShouldBe("additionalJobSize");
        }

        [Test]
        public static void AddJobSize_does_not_cause_report_when_fraction_is_zero()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            progress.AssertSingleAndClear(Report(0, "Gathering info"));

            structuredProgress.AddJobSize(1);
            progress.AssertNoUpdates();
        }

        [Test]
        public static void AddJobSize_causes_report_when_fraction_is_nonzero()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            structuredProgress.AddJobSize(1);
            structuredProgress.Next("Step 2");
            progress.AssertLastAndClear(Report(1, 2, "Step 2"));

            structuredProgress.AddJobSize(1);
            progress.AssertSingleAndClear(Report(1, 3, "Step 2"));
        }

        [Test]
        public static void Next_completes_initial_job_size([Values(0, 1, 10)] int initialJobSize)
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info", initialJobSize);
            structuredProgress.AddJobSize(1);
            progress.Clear();

            structuredProgress.Next("Step 2");
            progress.AssertSingleAndClear(Report(initialJobSize, initialJobSize + 1, "Step 2"));
        }

        [Test]
        public static void Next_completes_previous_job_size([Values(0, 1, 10)] int previousJobSize)
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            structuredProgress.AddJobSize(previousJobSize + 1);
            structuredProgress.Next("Step 2", nextJobSize: previousJobSize);
            progress.Clear();

            structuredProgress.Next("Step 3");
            progress.AssertSingleAndClear(Report(1 + previousJobSize, 1 + previousJobSize + 1, "Step 3"));
        }

        [Test]
        public static void CreateSubprogress_does_not_cause_report()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info");
            progress.AssertSingleAndClear(Report(0, "Gathering info"));

            structuredProgress.CreateSubprogress();
            progress.AssertNoUpdates();
        }

        [Test]
        public static void CreateSubprogress_may_be_called_before_previous_subtask_is_finished()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info", initialJobSize: 2);
            progress.AssertSingleAndClear(Report(0, "Gathering info"));

            structuredProgress.CreateSubprogress();
            structuredProgress.CreateSubprogress();
            progress.AssertNoUpdates();
        }

        [Test]
        public static void Subtasks_may_not_have_a_total_size_larger_than_the_current_job()
        {
            var progress = new ProgressSpy();
            var structuredProgress = progress.Start("Gathering info", initialJobSize: 10);
            progress.AssertSingleAndClear(Report(0, "Gathering info"));

            var ex = Should.Throw<ArgumentOutOfRangeException>(() => structuredProgress.CreateSubprogress(jobSize: 11));
            ex.ParamName.ShouldBe("jobSize");
            ex.ActualValue.ShouldBe(11);
            ex.Message.ShouldStartWith("The subprogress job size (11) is greater than the size remaining after the last call to Next (10).");
        }

        [Test]
        public static void Subtasks_appear_in_original_order()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks", initialJobSize: 3);
            var sub1 = structuredProgress.CreateSubprogress();
            var sub2 = structuredProgress.CreateSubprogress();
            var sub3 = structuredProgress.CreateSubprogress();
            progress.Clear();

            sub3.Report(Report(0, "Sub 3"));
            progress.AssertSingleAndClear(Report(0, "Tasks", Report(0, "Sub 3")));

            sub1.Report(Report(0, "Sub 1"));
            progress.AssertSingleAndClear(Report(0, "Tasks", Report(0, "Sub 1"), Report(0, "Sub 3")));

            sub2.Report(Report(0, "Sub 2"));
            progress.AssertSingleAndClear(Report(0, "Tasks", Report(0, "Sub 1"), Report(0, "Sub 2"), Report(0, "Sub 3")));
        }

        [Test]
        public static void Subtasks_disappear_when_complete()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks", initialJobSize: 3);
            var sub1 = structuredProgress.CreateSubprogress();
            var sub2 = structuredProgress.CreateSubprogress();
            var sub3 = structuredProgress.CreateSubprogress();
            sub1.Report(Report(0, "Sub 1"));
            sub2.Report(Report(0, "Sub 2"));
            sub3.Report(Report(0, "Sub 3"));
            progress.AssertLastAndClear(Report(0, "Tasks", Report(0, "Sub 1"), Report(0, "Sub 2"), Report(0, "Sub 3")));

            sub2.Report(Report(1, "Never seen"));
            progress.AssertSingleAndClear(Report(1, 3, "Tasks", Report(0, "Sub 1"), Report(0, "Sub 3")));
        }

        [Test]
        public static void Subtasks_are_never_seen_if_they_immediately_complete()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks", initialJobSize: 2);
            var sub1 = structuredProgress.CreateSubprogress();
            var sub2 = structuredProgress.CreateSubprogress();
            progress.AssertLastAndClear(Report(0, "Tasks"));

            sub1.Report(Report(1, "Never seen"));
            progress.AssertSingleAndClear(Report(1, 2, "Tasks"));

            sub2.Report(Report(1, "Never seen"));
            progress.AssertSingleAndClear(Report(2, 2, "Tasks"));
        }

        [Test]
        public static void Subtask_report_is_ignored_if_duplicate()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks");
            var sub1 = structuredProgress.CreateSubprogress();
            progress.Clear();

            sub1.Report(Report(0.5, "A", Report(0.5, "A.1")));
            progress.AssertSingleAndClear(Report(0.5, "Tasks", Report(0.5, "A", Report(0.5, "A.1"))));

            sub1.Report(Report(0.5, "A", Report(0.5, "A.1")));
            progress.AssertNoUpdates();
        }

        [Test]
        public static void Subtask_report_is_not_ignored_if_message_differs()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks");
            var sub1 = structuredProgress.CreateSubprogress();
            progress.Clear();

            sub1.Report(Report(0.5, "A", Report(0.5, "A.1")));
            progress.AssertSingleAndClear(Report(0.5, "Tasks", Report(0.5, "A", Report(0.5, "A.1"))));

            sub1.Report(Report(0.5, "B", Report(0.5, "A.1")));
            progress.AssertSingleAndClear(Report(0.5, "Tasks", Report(0.5, "B", Report(0.5, "A.1"))));
        }

        [Test]
        public static void Subtask_report_is_not_ignored_if_fraction_differs()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks");
            var sub1 = structuredProgress.CreateSubprogress();
            progress.Clear();

            sub1.Report(Report(0.5, "A", Report(0.5, "A.1")));
            progress.AssertSingleAndClear(Report(0.5, "Tasks", Report(0.5, "A", Report(0.5, "A.1"))));

            sub1.Report(Report(0.6, "A", Report(0.5, "A.1")));
            progress.AssertSingleAndClear(Report(0.6, "Tasks", Report(0.6, "A", Report(0.5, "A.1"))));
        }

        [Test]
        public static void Subtask_report_is_not_ignored_if_subtask_differs()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks");
            var sub1 = structuredProgress.CreateSubprogress();
            progress.Clear();

            sub1.Report(Report(0.5, "A", Report(0.5, "A.1")));
            progress.AssertSingleAndClear(Report(0.5, "Tasks", Report(0.5, "A", Report(0.5, "A.1"))));

            sub1.Report(Report(0.5, "A", Report(0.5, "A.2")));
            progress.AssertSingleAndClear(Report(0.5, "Tasks", Report(0.5, "A", Report(0.5, "A.2"))));
        }

        [Test]
        public static void Subtasks_add_scaled_fraction()
        {
            var progress = new ProgressSpy();

            var structuredProgress = progress.Start("Tasks", initialJobSize: 2.5);
            var sub1 = structuredProgress.CreateSubprogress(0.5);
            var sub2 = structuredProgress.CreateSubprogress(2);
            progress.AssertLastAndClear(Report(0, "Tasks"));

            sub1.Report(Report(0.1, "Sub 1"));
            progress.AssertSingleAndClear(Report(0.1 * 0.5, 2.5, "Tasks", Report(0.1, "Sub 1")));

            sub1.Report(Report(0.2, "Sub 1"));
            progress.AssertSingleAndClear(Report(0.2 * 0.5, 2.5, "Tasks", Report(0.2, "Sub 1")));

            sub1.Report(Report(0.3, "Sub 1"));
            progress.AssertSingleAndClear(Report(0.3 * 0.5, 2.5, "Tasks", Report(0.3, "Sub 1")));

            sub2.Report(Report(0.1, "Sub 2"));
            progress.AssertSingleAndClear(Report((0.3 * 0.5) + (0.1 * 2), 2.5, "Tasks", Report(0.3, "Sub 1"), Report(0.1, "Sub 2")));

            sub2.Report(Report(0.2, "Sub 2"));
            progress.AssertSingleAndClear(Report((0.3 * 0.5) + (0.2 * 2), 2.5, "Tasks", Report(0.3, "Sub 1"), Report(0.2, "Sub 2")));

            sub2.Report(Report(0.3, "Sub 2"));
            progress.AssertSingleAndClear(Report((0.3 * 0.5) + (0.3 * 2), 2.5, "Tasks", Report(0.3, "Sub 1"), Report(0.3, "Sub 2")));

            sub1.Report(Report(0.4, "Sub 1"));
            progress.AssertSingleAndClear(Report((0.4 * 0.5) + (0.3 * 2), 2.5, "Tasks", Report(0.4, "Sub 1"), Report(0.3, "Sub 2")));

            sub2.Report(Report(0.4, "Sub 2"));
            progress.AssertSingleAndClear(Report((0.4 * 0.5) + (0.4 * 2), 2.5, "Tasks", Report(0.4, "Sub 1"), Report(0.4, "Sub 2")));

            sub1.Report(Report(1, "Sub 1"));
            progress.AssertSingleAndClear(Report((1 * 0.5) + (0.4 * 2), 2.5, "Tasks", Report(0.4, "Sub 2")));

            sub2.Report(Report(1, "Sub 2"));
            progress.AssertSingleAndClear(Report((1 * 0.5) + (1 * 2), 2.5, "Tasks"));
        }

        [Test]
        public static void Nested_subtasks()
        {
            var progress = new ProgressSpy();

            var level1 = progress.Start("Level 1");
            var level2 = level1.CreateSubprogress(0.1).Start("Level 2");
            var level3 = level2.CreateSubprogress(0.1).Start("Level 3");
            var level4 = level3.CreateSubprogress(0.1).Start("Level 4");
            progress.AssertLastAndClear(
                Report(0, "Level 1",
                    Report(0, "Level 2",
                        Report(0, "Level 3",
                            Report(0, "Level 4")))));

            level4.AddJobSize(9);

            level4.Next("Level 4");
            progress.AssertLastAndClear(
                Report(0.0001, "Level 1",
                    Report(0.001, "Level 2",
                        Report(0.01, "Level 3",
                            Report(0.1, "Level 4")))));

            level4.Next("Level 4");
            progress.AssertLastAndClear(
                Report(0.0002, "Level 1",
                    Report(0.002, "Level 2",
                        Report(0.02, "Level 3",
                            Report(0.2, "Level 4")))));
        }

        // Test TODO:
        // Next before subtasks
        // Complete before subtasks
        // Subtask going backwards
        // Subtask reporting again after 100%
        // Subtask reporting again after Next
        // Subtask reporting again after Complete
        // AddSize before subtasks
        // Concurrency

        // TODO:
        // Doc comments
        // Readme
    }
}
