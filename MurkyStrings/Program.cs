using System;
using System.IO;
using AsmResolver.DotNet;

namespace MurkyStrings
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("MurkyStrings by drakonia | https://github.com/dr4k0nia");

            Console.WriteLine("Available arguments --mode=");
            Console.WriteLine("replace[glyph]\nreplace[simple]\nremove\ncombine[glyph]\ncombine[simple]\n");

            if (!File.Exists(args[0]))
                throw new FileNotFoundException(args[0]);

            var module = ModuleDefinition.FromFile(args[0]);

            if (args.Length == 1)
            {
                Console.WriteLine($"Mode was not specified using default option: replace[glyph]");
                var obfuscator = new ReplaceObfuscator(module);
                obfuscator.Execute();
            }
            else
            {
                if (!args[1].StartsWith("--mode="))
                    throw new Exception("Invalid arguments please use --mode=");

                if (!TryHandleMode(args[1].Remove(0, 7), ref module))
                {
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine($"Used mode: {args[1]}");
            }

            string path = module.FilePath!;
            string filePath = Path.Combine(Path.GetDirectoryName(path)!,
                $"{Path.GetFileNameWithoutExtension(path)}_murked{Path.GetExtension(path)}");
            Console.WriteLine($"Strings have been obfuscated, output file: {filePath}");
            module.Write(filePath);
        }

        private static bool TryHandleMode(string mode, ref ModuleDefinition module)
        {
            Obfuscator obfuscator;
            switch (mode)
            {
                case "replace[glyph]":
                    obfuscator = new ReplaceObfuscator(module);
                    obfuscator.Execute();
                    return true;
                case "replace[simple]":
                    obfuscator = new ReplaceObfuscator(module, ReplaceObfuscator.Mode.Simple);
                    obfuscator.Execute();
                    return true;
                case "remove":
                    obfuscator = new RemoveObfuscator(module);
                    obfuscator.Execute();
                    return true;
                case "combine[glyph]":
                    obfuscator = new RemoveObfuscator(module);
                    obfuscator.Execute();
                    obfuscator = new ReplaceObfuscator(module);
                    obfuscator.Execute();
                    return true;
                case "combine[simple]":
                    obfuscator = new RemoveObfuscator(module);
                    obfuscator.Execute();
                    obfuscator = new ReplaceObfuscator(module, ReplaceObfuscator.Mode.Simple);
                    obfuscator.Execute();
                    return true;
                default:
                    Console.WriteLine(
                        "\nPlease use one of the available modes: replace[glyph], replace[simple], remove, combine[glyph], combine[simple]");
                    return false;
            }
        }
    }
}
