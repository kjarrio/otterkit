# <img width="36" height="36" src="https://raw.githubusercontent.com/otterkit/otterkit/main/Assets/OtterkitIcon.png?sanitize=true&raw=true"> Otterkit COBOL Compiler

Otterkit is a free and open source compiler for the [COBOL Programming Language](https://en.wikipedia.org/wiki/COBOL#History_and_specification) on the .NET platform.

Warning: The project is currently in pre-release, so not all of the standard has been implemented.

## About

COBOL was created in 1959 by the [CODASYL Committee](https://en.wikipedia.org/wiki/CODASYL) (With Rear Admiral Grace Hopper as a technical consultant to the committee), its design follows Grace Hopper's belief that programs should be written in a language that is close to English. It prioritizes readability, reliability, and long-term maintenance. The language has been implemented throughout the decades on many platforms with many dialects. Otterkit COBOL is a free and open source compiler that aims to implement the [ISO/IEC 1989:2023 COBOL Standard](https://www.iso.org/standard/74527.html) (COBOL 2023) on the .NET platform.

## Installation

### Quick Install

Otterkit is available to install on the [Nuget package manager](https://www.nuget.org/packages/Otterkit/) ([.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) is required). To install, type these two lines into the command line:
```
dotnet new install Otterkit.Templates::1.0.45-alpha

dotnet tool install --global Otterkit --version 1.0.50-alpha
```

### Build from Source

First, run the git clone command, with the relevant arguments: 
```
git clone https://github.com/otterkit/otterkit.git --recurse-submodules --remote-submodules
```
The *recurse-submodules* and *remote-submodules* flags are needed to access the [libotterkit](https://github.com/otterkit/libotterkit) submodule inside.

Then, navigate into the `otterkit/src` folder (for the compiler, not libotterkit) and then type `dotnet run` into the command line to run and test if everything is working.

### Build from Source on MacOS (For Intel and Apple Silicon processors)

After clone the git repo navigate into the `src` folder and type:

```
dotnet run new app   
```

For build the COBOL file type:

```
dotnet run build -e ${COBOLFILEPATH} --free       
```

For build and run the COBOL file type:

```
dotnet run build --run -e ${COBOLFILEPATH} --free          
```

## Sponsors and Open Source Support

<h3 align="center">Open Source Support</h3>

<p align="center">
  <a target="_blank" href="https://www.jetbrains.com/community/opensource/">
    <img width="160" src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" alt="JetBrains Logo (Main) logo.">
  </a>
</p>

## Standard Acknowledgement

Any organization interested in reproducing the COBOL standard and specifications in whole or in part,
using ideas from this document as the basis for an instruction manual or for any other purpose, is free
to do so. However, all such organizations are requested to reproduce the following acknowledgment
paragraphs in their entirety as part of the preface to any such publication (any organization using a
short passage from this document, such as in a book review, is requested to mention "COBOL" in
acknowledgment of the source, but need not quote the acknowledgment):

COBOL is an industry language and is not the property of any company or group of companies, or of any
organization or group of organizations.

No warranty, expressed or implied, is made by any contributor or by the CODASYL COBOL Committee
as to the accuracy and functioning of the programming system and language. Moreover, no
responsibility is assumed by any contributor, or by the committee, in connection therewith.

The authors and copyright holders of the copyrighted materials used herein:

- FLOW-MATIC® (trademark of Sperry Rand Corporation), Programming for the 'UNIVAC® I and
  II, Data Automation Systems copyrighted 1958,1959, by Sperry Rand Corporation;
- IBM Commercial Translator Form No F 28-8013, copyrighted 1959 by IBM;
- FACT, DSI 27A5260-2760, copyrighted 1960 by Minneapolis-Honeywell

Have specifically authorized the use of this material in whole or in part, in the COBOL specifications.
Such authorization extends to the reproduction and use of COBOL specifications in programming
manuals or similar publications.
