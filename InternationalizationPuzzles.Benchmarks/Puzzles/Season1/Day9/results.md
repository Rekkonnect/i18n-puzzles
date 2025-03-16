# Season 1 Day 09 - Benchmarks

This puzzle was implemented using [Rekkon.UmbraString](https://github.com/Rekkonnect/UmbraString). The choice for UmbraString was experimental around the theoretical hypothesis of performance gains over not using it. Below are benchmarks comparing two distinct implementations of the solver, one using `string` and the other using `UmbraString`.

There are no custom implementations for the dictionary holding UmbraString keys, so it is possible even better performance can be achieved through such a custom dictionary type.

## Example Results

The results below were tested against the real-case input of the puzzle.

```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5608/22H2/2022Update)
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK 9.0.201
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 [AttachedDebugger]
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
```

| Method                   | Mean      | Error    | StdDev   | Ratio |
|------------------------- |----------:|---------:|---------:|------:|
| RegularStringFullRun     | 102.82 us | 0.708 us | 0.662 us |  1.00 |
| UmbraStringFullRun       |  90.78 us | 1.017 us | 0.951 us |  0.88 |
| UmbraStringFullRunUtf8   |  88.51 us | 0.793 us | 0.742 us |  0.86 |
| RegularStringLoadInput   |  60.31 us | 0.471 us | 0.417 us |  0.59 |
| UmbraStringLoadInput     |  56.78 us | 0.591 us | 0.553 us |  0.55 |
| UmbraStringLoadInputUtf8 |  56.84 us | 0.266 us | 0.249 us |  0.55 |
| RegularStringSolve       |  33.95 us | 0.253 us | 0.224 us |  0.33 |
| UmbraStringSolve         |  28.36 us | 0.177 us | 0.165 us |  0.28 |
| UmbraStringSolveU8       |  28.60 us | 0.463 us | 0.433 us |  0.28 |


| Method                   | Mean     | Error    | StdDev   | Ratio |
|------------------------- |---------:|---------:|---------:|------:|
| RegularStringFullRun     | 91.83 us | 0.738 us | 0.690 us |  1.00 |
| UmbraStringFullRun       | 98.82 us | 0.741 us | 0.693 us |  1.08 |
| UmbraStringFullRunUtf8   | 84.67 us | 0.955 us | 0.847 us |  0.92 |
| RegularStringLoadInput   | 48.34 us | 0.564 us | 0.500 us |  0.53 |
| UmbraStringLoadInput     | 56.17 us | 0.695 us | 0.616 us |  0.61 |
| UmbraStringLoadInputUtf8 | 58.72 us | 0.851 us | 0.796 us |  0.64 |
| RegularStringSolve       | 34.40 us | 0.508 us | 0.476 us |  0.37 |
| UmbraStringSolve         | 27.61 us | 0.169 us | 0.158 us |  0.30 |
| UmbraStringSolveU8       | 27.57 us | 0.332 us | 0.310 us |  0.30 |


| Method                   | Mean     | Error    | StdDev   | Ratio | RatioSD |
|------------------------- |---------:|---------:|---------:|------:|--------:|
| RegularStringFullRun     | 92.46 us | 0.970 us | 0.907 us |  1.00 |    0.01 |
| UmbraStringFullRun       | 92.33 us | 1.560 us | 1.459 us |  1.00 |    0.02 |
| UmbraStringFullRunUtf8   | 83.32 us | 0.751 us | 0.702 us |  0.90 |    0.01 |
| RegularStringLoadInput   | 63.24 us | 0.638 us | 0.597 us |  0.68 |    0.01 |
| UmbraStringLoadInput     | 59.89 us | 0.741 us | 0.693 us |  0.65 |    0.01 |
| UmbraStringLoadInputUtf8 | 50.54 us | 0.946 us | 0.885 us |  0.55 |    0.01 |
| RegularStringSolve       | 34.19 us | 0.344 us | 0.321 us |  0.37 |    0.00 |
| UmbraStringSolve         | 28.24 us | 0.176 us | 0.165 us |  0.31 |    0.00 |
| UmbraStringSolveU8       | 27.87 us | 0.310 us | 0.290 us |  0.30 |    0.00 |

## Observations

Sometimes `RegularStringFullRun` averages 92us, some others 102us on the same machine.
Some other methods behave the same. This could be caused by thermal throttling.

Unicode-based UmbraString construction might be slower, but never faster for this input.
The largest name in the input is Alessandro, spanning 10 characters, or 20 bytes
in the Unicode encoding. In UTF-8 encoding, all names fit within the UmbraString's
in-place content.

The all-around optimal solution is always the UTF-8-based UmbraString, offering
up to 10% performance for this real-case input.

## Larger Inputs

Larger inputs have not been tested so far. An input generator must be employed first.
Those larger inputs will be compared against the real case's observations.

## Notes

The UmbraString solver has a `UTF16_UMBRA_STRING` preprocessing symbol which conditionally enables Unicode-based UmbraString values to be loaded. However, it crashed the `UmbraStringSolve` with AVE during testing. It's only present for experimentation purposes, and the performance is expected to be worse than the best provided by UTF-8.
