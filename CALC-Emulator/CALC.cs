namespace CALC;

class Tools
{
    static Dictionary<string, string> codeFormat { get; } = new Dictionary<string, string>() {
        {"NOP", "0000"},
        {"HLT", "0100"},
        {"DLY", "02 num"},
        {"JMP", "10 num"},
        {"BRH", "2 flag num"},
        {"CAL", "30 funcID"},
        {"RTN", "4000"},
        {"LDI", "5 reg num"},
        {"LDM", "6 reg num"},
        {"STR", "7 reg num"},
        {"ADD", "8 type sType clr"},
        {"SUB", "9 type sType clr"},
        {"SFT", "A type sType reg"},
        {"GTI", "B reg type"},
        {"DSO", "C reg type"}
    };

    static public string Format(string opcode, string[] args)
    {
        if (string.IsNullOrEmpty(opcode)) { return ""; }
        string formattedCode = codeFormat[opcode].Replace(" ","");
        int opcodeID = Convert.ToInt32(formattedCode.Substring(0,1), 16);
        string structuredCode = formattedCode;

        switch (opcodeID)
        {
            default: case 0:
            structuredCode = formattedCode.Substring(1,1) != "2" ? formattedCode : formattedCode
            .Replace("num", args[0]);
            break;
            case 1:
            structuredCode = formattedCode
            .Replace("num", args[0]);
            break;
            case 2:
            structuredCode = formattedCode
            .Replace("flag", args[0])
            .Replace("num", args[1]);
            break;
            case 3:
            structuredCode = formattedCode
            .Replace("funcID", args[0]);
            break;
            case 5: case 6: case 7: 
            structuredCode = formattedCode
            .Replace("reg", args[0])
            .Replace("num", args[1]);
            break;
            case 8: case 9:
            structuredCode = formattedCode
            .Replace("type", args[0])
            .Replace("sType", args[1])
            .Replace("clr", args[2]);
            break;
            case 10:
            structuredCode = formattedCode
            .Replace("type", args[0])
            .Replace("sType", args[1])
            .Replace("reg", args[2]);
            break;
            case 11: case 12:
            structuredCode = formattedCode
            .Replace("reg", args[0])
            .Replace("type", args[1]);
            break;
        }

        return "0x"+structuredCode;
    }

    static public string TwoComp(int num)
    {
        return num < 0 || num >= 0x8000 ? $"-{(0xFFFF-num+1)%0x10000:X}" : $"{num:X}";
    }
}

class Assembler
{
    static public string[][] Assemble(string[] asm)
    {
        string[] code = new string[asm.Length];
        List<string> p = new List<string>();
        char[] split = {',',' ','r','c'};

        for (int i = 0; i < asm.Length; i++)
        {
            string[] args = asm[i].Split(split);
            args = args.Where(a => !string.IsNullOrEmpty(a)).ToArray();
            string opcode = args[0];
            if (opcode == "def")
            {
                p.Add($"{i-p.Count}");
                goto end;
            }
            if (opcode == "Main:")
            {
                p.Insert(0, $"{i-p.Count}");
                goto end;
            }
            args = args.Skip(1).ToArray();
            code[i] = Tools.Format(opcode, args);
            end:;
        }
        code = code.Where(a => !string.IsNullOrEmpty(a)).ToArray();
        string[] pointers = p.ToArray();

        return new [] {code, pointers};
    }
}

class Executor
{
    static Random rnd = new Random();
    static int[] RAM = new int[0xFF];
    static int[] reg = new int[8] {
        0, 0, 0, 0, 0, 0, 0, 0
    };
    static bool[] regF = new bool[6];
    static int[] pointers = new int[8];
    static int lastAddress = 0;
    static Dictionary<int, Delegate> instructions = new Dictionary<int, Delegate>() {
        {0x0, MSC}, // NOP, HLT, DLY
        {0x1, JMP},
        {0x2, BRH},
        {0x3, CAL},
        {0x4, RTN},
        {0x5, LDI},
        {0x6, LDM},
        {0x7, STR},
        {0x8, ADD},
        {0x9, SUB},
        {0xA, SFT},
        {0xB, GTI},
        {0xC, DSO}
    };

    static public void Execute(string[] code, string[] pointersStr, bool debug = false)
    {
        RAM = new int[0xFF];
        reg = new int[8];
        lastAddress = 0;

        code = Array.ConvertAll(code, l => l.Replace("0x",""));
        int[] _code = Array.ConvertAll(code, s => Convert.ToInt32(s, 16));
        int[] pointers = Array.ConvertAll(pointersStr, s => Convert.ToInt32(s));
        UpdateFlags(0);

        for (int i = pointers[0]; i < code.Length; i++)
        {
            int[] line = code[i].Select(d => Convert.ToInt32(new string(d,1), 16)).ToArray();
            int opcode = line[0];
            List<int> args = line.Skip(1).ToList();
            args.Add(i);

            if (debug) { Debug(_code,i); }
            if (opcode == 0 && args[0] == 1) { return; }
            if ((opcode == 12 || opcode == 11) && debug) {
                Console.WriteLine("\nProgram:\n");
            }
            int? output = (int?)instructions[opcode].DynamicInvoke(args.ToArray());
            output = opcode == 3 && output != null ? pointers[(int)output+1] : output;
            i = output != null ? (int)output-1 : i;
        }
    }

    static void Debug(int[] code, int current)
    {
        string str = "\nDebug Output:\n\n";
        for (int i = 0; i < code.Length; i++)
        {
            int line = code[i];
            string arrow = i == current ? " <-" : "";
            string n = i != code.Length-1 ? "\n" : "";
            str += $"{i:X4}: {line:X6}{arrow}{n}";
        }
        Console.WriteLine(str);
    }

    static void UpdateFlags(int num)
    {
        /*
        0: Zero               default: true
        1: Not Zero           default: false
        2: Negative           default: false
        3: Not Negative       default: true
        4: Carry/Overflow     default: false
        5: Not Carry/Overflow default: true
        */
        regF[0] = regF[3] = regF[5] = true;
        regF[1] = regF[2] = regF[4] = false;

        if (num != 0)
        {
            regF[0] = false;
            regF[1] = true;
        }

        if (num < 0 || num >= 0x8000)
        {
            regF[2] = true;
            regF[3] = false;
        }

        if (num > 0xFFFF)
        {
            regF[4] = true;
            regF[5] = false;
        }
    }

    static void MSC(params int[] args) {
        if (args[0] == 0 || args[0] == 1) { return; }
        int delay = args[1]*16 + args[2];
        for (int i = 0; i < delay; i++) { Task.Delay(1000); }
    }
    static int JMP(params int[] args) {
        return args[1]*16 + args[2];
    }
    static int? BRH(params int[] args) {
        if (!regF[args[0]]) { return null; }
        return args[1]*16 + args[2];
    }
    static int CAL(params int[] args) {
        lastAddress = args[3]+1;
        return args[1]*16 + args[2];
    }
    static int RTN(params int[] args) {
        return lastAddress;
    }
    static void LDI(params int[] args) {
        reg[args[0]] = args[1]*16 + args[2];
    }
    static void LDM(params int[] args) {
        reg[args[0]] = RAM[args[1]*16 + args[2]];
    }
    static void STR(params int[] args) {
        RAM[args[1]*16 + args[2]] = reg[args[0]];
    }
    static void ADD(params int[] args) {
        int sum = args[0] == 1 && regF[4] ? 
        reg[args[2]] + reg[args[2]+1] + 1 : reg[args[2]] + reg[args[2]+1];
        UpdateFlags(sum);
        reg[args[2]] = args[1] == 0 || args[1] == 2 ? sum : reg[args[2]];
        reg[args[2]+1] = args[1] == 1 || args[1] == 2 ? sum : reg[args[2]+1];
    }
    static void SUB(params int[] args) {
        int sum = args[0] == 1 && regF[4] ? 
        reg[args[2]] - reg[args[2]+1] + 1 : reg[args[2]] - reg[args[2]+1];
        UpdateFlags(sum);
        reg[args[2]] = args[1] == 0 || args[1] == 2 ? sum : reg[args[2]];
        reg[args[2]+1] = args[1] == 1 || args[1] == 2 ? sum : reg[args[2]+1];
    }
    static void SFT(params int[] args) {
        int dir; int nextReg; int num;
        dir = args[0]-args[0]*3+1;

        if (args[1] == 0) {
            nextReg = args[2]+dir;
            num = reg[args[2]];
            reg[args[2]] = 0;
            if (nextReg < 0 || nextReg >= reg.Length) { return; }
            reg[nextReg] = num;
            return;
        }
        
        // for (int i = args[0] == 0 ? 0 : reg.Length-1; i < reg.Length || i != 0; i += dir) {
        //     nextReg = i+dir;
        //     num = reg[i];
        //     reg[i] = 0;
        //     if (nextReg < 0 || nextReg >= reg.Length) { goto end; }
        //     reg[i] = num;
        //     end:;
        // }
    }
    static void GTI(params int[] args) {
        int port = args[1]*16 + args[2];
        if (port == 0 || port == 1) {
            string output = port == 0 ?
            $"Hexadecimal > " :
            $"Decimal > ";
            Console.Write(output);
            string? input = Console.ReadLine();
            reg[args[0]] = port == 0 ? Convert.ToInt32(input,16) : Convert.ToInt32(input);
            return;
        }
        reg[args[0]] = port == 2 ? rnd.Next(0xFFFF) : reg[args[0]];
    }
    static void DSO(params int[] args) {
        int port = args[1]*16 + args[2];
        string output = port == 0 ?
        $"Hexadecimal: {Tools.TwoComp(reg[args[0]])}" :
        $"Decimal: {reg[args[0]]}";
        Console.WriteLine(output);
    }
}