# Helpwiz.FastCsvReader
A fast compiled expression based csv reader.

# Capabilities
- Allows custom delimiter characters.
- Allows quoted values with commas inside e.g. "My value, my second value and my third value"
- Allows double escaped quotes e.g. "And his heart sang, like a bird: ""I love you!"""
- Allows backslash escaped quotes e.g. "And his heart sang, like a bird: \\"I love you!\\""

# Limitations
- Does not allow multi-line values
- Requires a header row (but this can be read from a separate file or synthesised if required).

# Performance
Because the property access expressions are compiled, the library will load csv files at the maximum speed possible.

# Getting started
## Basic Usage

If you want the csv data to populate a class, make settable properties of all the header fields in the class, and the reader will
read into that class


```
private string[] testData = new []
{ 
  "A, B, C, D", 
  "1,Two,3.0,"
};

private class TargetType 
{
  public int A { get; set; }
  
  public string B { get; set; }
  
  public double C { get; set; }
  
  public int? D { get; set; }
}

//...

public IEnumerable<TargetType> ReadElements() => FastCsvReader.ReadAs<TargetType>(testData);
```

## Named properties

If you want the properties of the target type to differ in name from the header fields, you can use a [CsvProperty] attribute.
Note header names are trimmed when matching to properties.

```
private string[] testData = new[]
{
  "A property, B property, C property, D property",
  "1,Two,3.0,"
};

private class TargetType 
{
  [CsvProperty("A property")]
  public int A { get; set; }
  
  [CsvProperty("B property")]
  public string B { get; set; }
  
  [CsvProperty("C property")]
  public double C { get; set; }
  
  [CsvProperty("D property")]
  public int? D { get; set; }
}
```

## Data conversion and Custom Delimiters
Sensible defaults for string, int, double, DateTime, int?, double? and DateTime? are provided by default. You can override the default behaviour of this
and extend to other datatypes as follows:

```
  var converter = new DefaultConverterSpec();
  converter.SetConverter<int?>(s =>
  {
    if (string.IsNullOrWhiteSpace(s)) return null;
    if (!int.TryParse(s, out int value)) return null;
    return value;
  });
  
  //Read the file with ¬ as the delimiter character and with custom handling for nullable integers.
  var result = FastCsvReader.ReadAs<DefaultData2>(testData3, '¬', converter).ToArray();
```

## Additional Data
You can also load raw strings into your objects by implementing IAdditionalData on your object e.g.

```
private class TargetType : IAdditionalData
{
  //Data from "A" column loads here.
  public string A { get; set; }
  
  //Additional data loads here (all columns except A)
  public void WriteColumn(string header, string value)
  {
    //...
  }
}

## Helpwiz
[Helpwiz](https://helpwiz.com) is an online marketplace for independent domestic cleaners. We allow clients to find a cleaner and book a clean 
in a simple, one-pass process. We have cleaners in many locations throughout the UK.

We provide this code for free, without any license contraints, but we do hope that, if you make use of the code, you will
link back to us or otherwise give us a shout out.
