using dotnet_test_rerun.Common.Utilities;
using FluentAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Utilities;

public class TestUtilitiesTests
{
    [Fact]
    public void GetTmpDirectory_ShouldCreateAndReturnTempDirectory()
    {
        // Act
        var tmpDir = TestUtilities.GetTmpDirectory();

        // Assert
        tmpDir.Should().NotBeNullOrEmpty();
        Directory.Exists(tmpDir).Should().BeTrue();

        // Cleanup
        Directory.Delete(tmpDir);
    }

    [Fact]
    public void CopyAll_WithNestedDirectories_ShouldCopyRecursively()
    {
        // Arrange
        var sourceTmp = TestUtilities.GetTmpDirectory();
        var targetTmp = TestUtilities.GetTmpDirectory();
        
        try
        {
            var sourceDir = new DirectoryInfo(sourceTmp);
            var targetDir = new DirectoryInfo(targetTmp);

            // Create a nested directory structure
            var subDir = sourceDir.CreateSubdirectory("SubDirectory");
            var nestedSubDir = subDir.CreateSubdirectory("NestedSubDirectory");

            // Create files at different levels
            File.WriteAllText(Path.Combine(sourceDir.FullName, "root.txt"), "Root file");
            File.WriteAllText(Path.Combine(subDir.FullName, "sub.txt"), "Sub file");
            File.WriteAllText(Path.Combine(nestedSubDir.FullName, "nested.txt"), "Nested file");

            // Act
            TestUtilities.CopyAll(sourceDir, targetDir);

            // Assert
            File.Exists(Path.Combine(targetDir.FullName, "root.txt")).Should().BeTrue();
            File.Exists(Path.Combine(targetDir.FullName, "SubDirectory", "sub.txt")).Should().BeTrue();
            File.Exists(Path.Combine(targetDir.FullName, "SubDirectory", "NestedSubDirectory", "nested.txt")).Should().BeTrue();
            
            File.ReadAllText(Path.Combine(targetDir.FullName, "root.txt")).Should().Be("Root file");
            File.ReadAllText(Path.Combine(targetDir.FullName, "SubDirectory", "sub.txt")).Should().Be("Sub file");
            File.ReadAllText(Path.Combine(targetDir.FullName, "SubDirectory", "NestedSubDirectory", "nested.txt")).Should().Be("Nested file");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(sourceTmp))
                Directory.Delete(sourceTmp, true);
            if (Directory.Exists(targetTmp))
                Directory.Delete(targetTmp, true);
        }
    }

    [Fact]
    public void CopyAll_WithFilesOnly_ShouldCopyFiles()
    {
        // Arrange
        var sourceTmp = TestUtilities.GetTmpDirectory();
        var targetTmp = TestUtilities.GetTmpDirectory();
        
        try
        {
            var sourceDir = new DirectoryInfo(sourceTmp);
            var targetDir = new DirectoryInfo(targetTmp);

            // Create files without subdirectories
            File.WriteAllText(Path.Combine(sourceDir.FullName, "file1.txt"), "Content 1");
            File.WriteAllText(Path.Combine(sourceDir.FullName, "file2.txt"), "Content 2");

            // Act
            TestUtilities.CopyAll(sourceDir, targetDir);

            // Assert
            File.Exists(Path.Combine(targetDir.FullName, "file1.txt")).Should().BeTrue();
            File.Exists(Path.Combine(targetDir.FullName, "file2.txt")).Should().BeTrue();
            File.ReadAllText(Path.Combine(targetDir.FullName, "file1.txt")).Should().Be("Content 1");
            File.ReadAllText(Path.Combine(targetDir.FullName, "file2.txt")).Should().Be("Content 2");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(sourceTmp))
                Directory.Delete(sourceTmp, true);
            if (Directory.Exists(targetTmp))
                Directory.Delete(targetTmp, true);
        }
    }
}
