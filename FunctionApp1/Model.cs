using Bogus;

namespace FunctionApp1;

public class EdmEvent(string name, string message)
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = name;
    public string Message { get; set; } = message;
}

internal static class EdmEventGenerator
{
    public static Faker<EdmEvent> Faker { get; } = new Faker<EdmEvent>()
        .CustomInstantiator(f => new EdmEvent(
            f.Person.FullName,
            f.Lorem.Sentence()
        ));
}

