# Introduction #

Once the project can be built without errors, execution will arise new problems

# Culture/Encoding problems #
ArcPad will automatically recognize a shapefile’s codepage if a supported Language Driver ID (LDID) is found in the header of the shapefile’s DBF table (in the 29th byte). If the codepage is available on the system, it will be used to translate the attribute contents and field names into displayable characters. If a valid LDID is not found in the header of the shapefile’s DBF table, then ArcPad will look in the following places to determine
the codepage (see http://downloads.esri.com/support/documentation/pad_/ArcPad_RefGuide_1105.pdf)

The .NET Compact Framework uses Unicode internally to represent character data. Unicode is a useful way to store character data because it provides a way to identify the characters used in any language in the world. You can use encoding to map Unicode characters to other character representations, which can be useful if your application must provide character data as an array of bytes specific to a code page such as ASCII or Windows-1252.

You can use the GetEncoding static method of the Encoding class to create an Encoding
object for a specific code page. The Encoding class is defined in the System.Text namespace. The Encoding class provides the GetBytes method, which can be used to convert Unicode data to a code page–specific byte array. The Encoding class also provides the GetChars method, which can be used to convert a byte array to an array of Unicode characters.

SharpMap\Data\Providers\Shapefile\DbaseEncodingRegistry.cs is used as a dictionary for storing LCID values in the dbf file, and associate them with a Culture+Encoding pair. These will be used later in DbaseFile.CreateDbaseFile() for writting the LDID value in the dbf header. Also, the Encoding bound to each LCID will be used to get a Encoding object for reading (bytes) and writting (characters) (DbaseFile, DbaseReader, DbaseWritter)

The GetEncoding method relies on the underlying platform to support most code pages. However, system support is supplied for the following cases: Specify code page 0 for the default encoding, that is, the encoding specified in the regional settings for the computer executing this method; 1200 for little-endian Unicode (UTF-16LE); 1201 for big-endian Unicode (UTF-16BE); 1252 for Windows operating system (windows-1252); 65000 for UTF-7; 65001 for UTF-8; 20127 for ASCII, and 54936 for GB18030 (Chinese Simplified).

Some unsupported code pages cause ArgumentException, while others cause NotSupportedException.

DbaseEncodingRegistry.setupDbaseToEncodingMap() tries to get encodings with codepages not supported by the PPC, so **NotSupportedException** is thrown.

