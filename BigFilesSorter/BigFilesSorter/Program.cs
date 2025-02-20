// See https://aka.ms/new-console-template for more information

using BigFilesSorter;

using var sorter = new Sorter("output-c05ae", deleteIntermediateFiles: true);
await sorter.Sort();