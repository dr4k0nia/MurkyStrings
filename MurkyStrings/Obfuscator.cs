using AsmResolver.DotNet;

namespace MurkyStrings;

public abstract class Obfuscator
{
    protected Obfuscator()
    {
    }

    public abstract void Execute();
}
