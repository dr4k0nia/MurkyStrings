# MurkyStrings - stealthy string obfuscation

MurkyStrings is a string obfuscator for .NET applications, built to evade static string analysis. It does not rely on encryption or encoding to evade entropy-based detections. Instead, it transforms strings into a murky mess by inserting special characters and random words. Removing them again on runtime.



## Usage



```

MurkyStrings.exe <file path> [--mode=<mode>]

```



Available modes:

| name | functionality |
|---|---|
|replace[glyph]|Insert a variety of homoglyph characters that look identical to alphabetical characters|
|replace[simple]|Insert random amounts of a special character in between all actual characters | |remove|Insert random method names and spaces into the original string|
|combine[glyph]|Combines remove and replace[glyph]|
|combine[simple]|Combines remove and replace[simple]|




## Compatibility



MurkyStrings is by default build for .NET Framework however, you can compile it targeting .NET Core and it should work just fine.


## How does it work?



I wrote a detailed blog post for that feel free to [check it out](https://dr4k0nia.github.io/posts/String-Obfuscation-The-Malware-Way/)



Very quickly explained: We insert junk data into the original strings and then remove the junk data on runtime.
