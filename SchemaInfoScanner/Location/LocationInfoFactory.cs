namespace SchemaInfoScanner.Location;

public class LocationInfoFactory
{
    private readonly List<RawLocationNode> rawNodes = [];

    public void AddNamespace(RawLocationNode node)
    {
        if (rawNodes.Any())
        {
            throw new InvalidOperationException("Namespace must be the first location node.");
        }

        if (node.Category is not Category.Namespace)
        {
            throw new InvalidOperationException("Namespace node must be a namespace category.");
        }

        rawNodes.Add(node);
    }

    public void AddNode(RawLocationNode node)
    {
        rawNodes.Add(node);
    }

    public void AddNearestOpenContainerIndex()
    {
        var node = rawNodes.LastOrDefault(x => x is { IsContainer: true, IsClosed: false });
        if (node is null)
        {
            throw new InvalidOperationException("There is no container to close.");
        }

        var indexCount = 0;
        for (var i = rawNodes.IndexOf(node) + 1; i < rawNodes.Count; i++)
        {
            if (rawNodes[i].Category == Category.Index)
            {
                indexCount++;
            }
        }

        rawNodes.Add(RawLocationNode.CreateIndex(indexCount));
    }

    public void CloseNearestOpenContainer()
    {
        var node = rawNodes.LastOrDefault(x => x is { IsContainer: true, IsClosed: false });
        if (node is null)
        {
            throw new InvalidOperationException("There is no container to close.");
        }

        node.Close();
    }

    public LocationInfo Build()
    {
        var nodes = new List<ILocationNode>(rawNodes.Count);
        foreach(var rawNode in rawNodes)
        {
            nodes.Add(rawNode.ToLocationNode());
        }

        return new(nodes);
    }

    /*
    AddNamespace(LocationNode.CreateNamespace("Excel4"));
    AddNode(LocationNode.CreateRecord("School", "StaticDataRecord"));
    AddNode(LocationNode.CreateRecordContainer("ClassA", "Student"));

    AddNearestOpenContainerIndex(); // ClassA[0]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateRecord("Grades", "SubjectGrade"));
    AddNearestOpenContainerIndex(); // Grades[0]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[1]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[2]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    CloseNearestOpenContainer(); // Grades

    AddNearestOpenContainerIndex(); // ClassA[1]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateRecord("Grades", "SubjectGrade"));
    AddNearestOpenContainerIndex(); // Grades[0]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[1]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[2]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    CloseNearestOpenContainer(); // Grades

    AddNearestOpenContainerIndex(); // ClassA[2]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateRecord("Grades", "SubjectGrade"));
    AddNearestOpenContainerIndex(); // Grades[0]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[1]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[2]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    CloseNearestOpenContainer(); // Grades
    CloseNearestOpenContainer(); // ClassA

    AddNode(LocationNode.CreateRecordContainer("ClassB", "Student"));

    AddNearestOpenContainerIndex(); // ClassB[0]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateRecord("Grades", "SubjectGrade"));
    AddNearestOpenContainerIndex(); // Grades[0]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[1]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[2]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    CloseNearestOpenContainer(); // Grades

    AddNearestOpenContainerIndex(); // ClassB[1]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateRecord("Grades", "SubjectGrade"));
    AddNearestOpenContainerIndex(); // Grades[0]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[1]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    AddNearestOpenContainerIndex(); // Grades[2]
    AddNode(LocationNode.CreateParameter("Name", "string"));
    AddNode(LocationNode.CreateEnum("Grade", "Grades"));
    CloseNearestOpenContainer(); // Grades
    */
}
