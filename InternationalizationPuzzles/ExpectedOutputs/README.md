# Expected Outputs

This directory contains expected outputs for the example inputs provided in the [Inputs](Inputs/) directory.

## Disclaimer

The inputs provided in this repository are bound to the original creator's license.

Quoting from the [Terms of Use](https://i18n-puzzles.com/about/):

>The puzzles, including input files, are licensed under the Creative Commons Atribution license [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/). You may copy any part of this site, as long as you attribute Matijn van Iersel and i18n-puzzles.com.



## Structure

The outputs are structured w.r.t. seasons, with each season having its own .exout file named as SeasonX, containing the puzzle and test case identifiers followed by the expected output.

### Grammar

The file contains a list of identifiers followed by the string version of the expected output, as it would be printed in the console after running the puzzle solver. The format for each entry is "ID: Output". Any whitespace is tolerated. Lines not beginning with a digit are ignored.

The file may begin with a Season X line, denoting the season it involves. If none is provided, it defaults to Season 1. It may be optionally inferred from the file name in the future.

The validator in the runner does **not** require that all puzzle identifiers be specified; if any is missing, the output will not be compared to an expected output.

A TextMate grammar is available for .exout files located [here](exout.tmLanguage.json), which can also be [loaded on Visual Studio](https://learn.microsoft.com/en-us/visualstudio/ide/adding-visual-studio-editor-support-for-other-languages?view=vs-2022#add-support-for-non-supported-languages) and Visual Studio Code.

### Examples

- `Season1.exout`: Season 1 expected outputs
  -	Contents:
  ```
  Season 1
  
  1T1: 31
  1: 107989

  2T1: 2019-06-05T12:15:00+00:00
  2: 2020-10-25T01:30:00+00:00
  ```
