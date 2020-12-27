# JSONSchema2POCO
A library to create POCO from a JSON Schema

## Content

- JSONSchema : an implementation of a draft-04 JSON Schema. Includes a constructor with a JsonDocument (from system.text.json) and some utility methods
- ClassGenerator : the implementation of the generator from a JSONSchema object to .NET source code
- ClassGeneratorApp : a console application that can read a json schema and produce a set of files to a destination folder
- JSONSchema2POCO.Tests : a test project for all the other projects

## ClassGeneratorApp

### Usage

ClassGeneratorApp.exe {{inputfile}} {{outputFolder}}
