using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Location;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class LocationInfoFactoryTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<LocationInfoFactoryTest>() is not TestOutputLogger<LocationInfoFactoryTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var locationInfoFactory = new LocationInfoFactory();
        locationInfoFactory.AddNamespace(RawLocationNode.CreateNamespace("Excel4"));
        locationInfoFactory.AddNode(RawLocationNode.CreateRecord("School", "StaticDataRecord"));
        locationInfoFactory.AddNode(RawLocationNode.CreateRecordContainer("ClassA", "Student"));

        locationInfoFactory.AddNearestOpenContainerIndex(); // ClassA[0]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateRecordContainer("Grades", "SubjectGrade"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[0]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[1]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[2]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.CloseNearestOpenContainer(); // Grades

        locationInfoFactory.AddNearestOpenContainerIndex(); // ClassA[1]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateRecordContainer("Grades", "SubjectGrade"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[0]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[1]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[2]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.CloseNearestOpenContainer(); // Grades

        locationInfoFactory.AddNearestOpenContainerIndex(); // ClassA[2]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateRecordContainer("Grades", "SubjectGrade"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[0]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[1]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[2]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.CloseNearestOpenContainer(); // Grades
        locationInfoFactory.CloseNearestOpenContainer(); // ClassA

        locationInfoFactory.AddNode(RawLocationNode.CreateRecordContainer("ClassB", "Student"));

        locationInfoFactory.AddNearestOpenContainerIndex(); // ClassB[0]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateRecordContainer("Grades", "SubjectGrade"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[0]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[1]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[2]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.CloseNearestOpenContainer(); // Grades

        locationInfoFactory.AddNearestOpenContainerIndex(); // ClassB[1]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateRecordContainer("Grades", "SubjectGrade"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[0]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[1]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.AddNearestOpenContainerIndex(); // Grades[2]
        locationInfoFactory.AddNode(RawLocationNode.CreateParameter("Name", "string"));
        locationInfoFactory.AddNode(RawLocationNode.CreateEnum("Grade", "Grades"));
        locationInfoFactory.CloseNearestOpenContainer(); // Grades
        locationInfoFactory.CloseNearestOpenContainer(); // ClassB

        var locationInfo = locationInfoFactory.Build();

        Assert.Empty(logger.Logs);
    }
}
