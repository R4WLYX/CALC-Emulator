using CALC;

namespace CALC_Emulator;

internal class Program
{
    static void Main(string[] args)
    {
        // Set Up
        bool debug = false;
        Console.Title = "CALC Emulator";
        start: string? s = Start();
        int choice = !string.IsNullOrEmpty(s) ? int.Parse(s) : 3;
        if (choice == 3) { Exit(); }
        debug = choice == 2? true : false;

        // Handles File Read
        getFile: Console.Write("Please Enter File Name to Assemble.\n> ");
        string? p = Console.ReadLine();
        Console.Clear();
        string filePath = File.Exists(p) ? p : "";
        if (string.IsNullOrEmpty(filePath))
        {
            Console.Clear();
            goto getFile;
        }
        string compiledFile = filePath.Replace("asc","smc");

        // Handles Assembling
        string[] assembly = File.ReadAllLines(filePath);
        string[][] info = Assembler.Assemble(assembly);
        string[] code = info[0];
        string[] pointers = info[1];

        // Handles File Write
        string codeStr = "";
        for (int i = 0; i < code.Length; i++) {
            codeStr += i != code.Length-1 ? code[i]+"\n" : code[i];
        }
        File.WriteAllTextAsync(compiledFile, codeStr);
        if (choice == 0) { goto start; }
        
        // Execute Assembled Code
        Executor.Execute(code, pointers, debug);
        Wait();
        goto start;
    }

    static string? Start()
    {
        Console.Clear();
        Console.Write
(@"What would you like to do?

[0] Assemble
[1] Execute
[2] Debug
[3] Exit

> ");
        string? s = Console.ReadLine();
        Console.Clear();
        return s;
    }

    static void Wait()
    {
        Console.Write("\nPress Enter to Continue.");
        while(Console.ReadKey(true).Key != ConsoleKey.Enter) {};
        Console.Clear();
    }

    static void Exit()
    {
        Console.Clear();
        Console.Write("Press ESC to Exit.");
        while(Console.ReadKey(true).Key != ConsoleKey.Escape) {};
        Console.Clear();
        Environment.Exit(0);
    }
}