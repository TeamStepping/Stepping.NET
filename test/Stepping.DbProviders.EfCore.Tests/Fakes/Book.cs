namespace Stepping.DbProviders.EfCore.Tests.Fakes;

public record Book(string Name)
{
    public int Id { get; set; }
    
    public string Name { get; set; } = Name;
}