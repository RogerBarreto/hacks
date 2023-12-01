namespace ConsoleApp1;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Start");

        var index = 0;
        await foreach (var item in Test())
        {
            index++;
            Console.WriteLine($"Item {index}");

            if (index == 1)
            {
                break;
            }
        }

        Console.WriteLine("End");
    }

    static IAsyncEnumerable<MyClass> Test()
    {
        try
        {
            return InnerTest();
        }
        finally
        {
            Console.WriteLine("Log");
        }
    }

    static async IAsyncEnumerable<MyClass> InnerTest()
    {
        yield return new MyClass();
    }
}

internal class MyClass
{
}