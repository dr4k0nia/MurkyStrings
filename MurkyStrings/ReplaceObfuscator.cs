using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace MurkyStrings
{
    public class ReplaceObfuscator : Obfuscator
    {
        private readonly ModuleDefinition _module;
        private readonly Mode _mode;

        private readonly Random _random;

        public enum Mode
        {
            Simple,
            Homoglyph
        }

        public ReplaceObfuscator(ModuleDefinition module, Mode mode = Mode.Homoglyph)
        {
            _module = module;
            _mode = mode;
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public override void Execute()
        {
            var importer = new ReferenceImporter(_module);
            foreach (var type in _module.GetAllTypes().Where(t => t.Methods.Count != 0))
            {
                foreach (var method in type.Methods)
                {
                    if (method.CilMethodBody == null)
                        continue;

                    var instructions = method.CilMethodBody.Instructions;

                    for (int i = 0; i < instructions.Count; i++)
                    {
                        if (instructions[i].OpCode != CilOpCodes.Ldstr)
                            continue;

                        if ((string) instructions[i].Operand == string.Empty)
                            continue;

                        instructions[i].Operand = ObfuscateString((string) instructions[i].Operand);

                        var implant = new List<CilInstruction>();
                        var replaceMethod =
                            importer.ImportMethod(typeof(string).GetMethod("Replace",
                                new[] {typeof(string), typeof(string)}) ?? throw new InvalidDataException());

                        if (_mode == Mode.Homoglyph)
                        {
                            string[] glyphs = {"а", "е", "і", "о", "с"};

                            string[] ordered = glyphs.OrderBy(c => _random.Next()).ToArray();

                            for (int j = 0; j < ordered.Length; j++)
                            {
                                implant.Add(new CilInstruction(CilOpCodes.Ldstr, ordered[j]));
                                implant.Add(new CilInstruction(CilOpCodes.Ldnull));

                                if (j == 0)
                                {
                                    implant.Add(new CilInstruction(CilOpCodes.Call, replaceMethod));
                                    continue;
                                }

                                implant.Add(new CilInstruction(CilOpCodes.Callvirt, replaceMethod));
                            }
                        }
                        else
                        {
                            implant.Add(new CilInstruction(CilOpCodes.Ldstr, @"\"));
                            implant.Add(new CilInstruction(CilOpCodes.Ldnull));
                            implant.Add(new CilInstruction(CilOpCodes.Call, replaceMethod));
                        }

                        instructions.InsertRange(i + 1, implant);

                        i += implant.Count;
                    }

                    instructions.OptimizeMacros();
                }
            }
        }

        private string ObfuscateString(string input)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in input)
            {
                // randomize if the glyphs are inserted before or after the original character
                if (_random.Next(0, 2) == 0)
                {
                    result.Append(_mode == Mode.Homoglyph
                        ? new string(GetHomoglyph(c), _random.Next(4))
                        : new string((char) 0x5C, _random.Next(16)));
                    result.Append(c);
                }
                else
                {
                    result.Append(c);
                    result.Append(_mode == Mode.Homoglyph
                        ? new string(GetHomoglyph(c), 1)
                        : new string((char) 0x5C, _random.Next(16)));
                }
            }

            return result.ToString();
        }

        // Use characters that look like standard alphabetical
        // but are actually from a different alphabet (Homoglyphs)
        private char GetHomoglyph(char input)
        {
            char[] glyphs = {'а', 'е', 'і', 'о', 'с'};
            switch (input)
            {
                case 'a':
                    return glyphs[0];
                case 'e':
                    return glyphs[1];
                case 'i':
                    return glyphs[2];
                case 'o':
                    return glyphs[3];
                default:
                    return glyphs[_random.Next(glyphs.Length)];
            }
        }
    }
}
