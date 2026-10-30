using dotnet.test.rerun.Execution;
using AwesomeAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Execution;

public class FilterBatchesTests
{
    [Fact]
    public void Split_ShortFilter_ReturnsSingleBatch()
        => FilterBatches.Split(new[] { "A", "B", "C" }).Should().Equal("A | B | C");

    [Fact]
    public void Split_LongFilter_ReturnsBatchesWithinMaxLength()
    {
        // Arrange
        var clauses = Enumerable.Range(1, 2000).Select(i => $"FullyQualifiedName=My.Namespace.MyClass.Test{i}").ToList();

        // Act
        var batches = FilterBatches.Split(clauses);

        // Assert
        batches.Should().HaveCountGreaterThan(1);
        batches.Should().OnlyContain(batch => batch.Length <= FilterBatches.MaxFilterLength);
        batches.SelectMany(batch => batch.Split(" | ")).Should().Equal(clauses);
    }

    [Fact]
    public void Split_ClauseLongerThanMax_GetsItsOwnBatch()
        => FilterBatches.Split(new[] { "A", new string('x', 20), "B" }, maxLength: 10)
            .Should().Equal("A", new string('x', 20), "B");

    [Fact]
    public void Split_ExactlyAtMax_StaysInOneBatch()
        => FilterBatches.Split(new[] { "AAA", "BBB" }, maxLength: 9).Should().Equal("AAA | BBB");
}
