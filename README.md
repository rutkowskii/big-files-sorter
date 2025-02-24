# Big files sorter

## Quick start

Build the whole solution, then you will have the exe files for input generation and sorting itself

Input generation example (Note that size is given in bytes):

```Powershell
.\InputGenerator.exe --size 8368709120 --file input8g-shorts --wordsPerStrMin 16 --wordsPerStrMax 22 --wordLenMin 7 --wordLenMax 9
```

Sorting example:

```Powershell
.\BigFilesSorter.exe --file input100g --memBufferSizeMb 100
```
