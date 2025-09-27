namespace MyTestLibrary;

public class MyPublicClass
{
    // Public Instance Property (Read/Write)
    public string InstanceProperty { get; set; } = "Default Instance Value";

    // Public Static Property (Read/Write)
    public static int StaticProperty { get; set; } = 100;

    // Public Instance Method
    public string GetInstanceMessage(string name)
    {
        return $"Hello, {name}! This is an instance method. InstanceProperty: {InstanceProperty}";
    }

    // Public Static Method
    public static int AddNumbers(int a, int b)
    {
        return a + b;
    }

    // Public Instance Method with side effect
    public void SetInstanceProperty(string newValue)
    {
        InstanceProperty = newValue;
    }

    // Public Static Method with side effect
    public static void SetStaticProperty(int newValue)
    {
        StaticProperty = newValue;
    }

    // Public Instance Method (no parameters, returns void)
    public void PrintMessage()
    {
        Console.WriteLine($"Printing from Instance. InstanceProperty: {InstanceProperty}");
    }

    // Public Static Method (no parameters, returns void)
    public static void PrintStaticMessage()
    {
        Console.WriteLine($"Printing from Static. StaticProperty: {StaticProperty}");
    }
}