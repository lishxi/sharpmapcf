## Localization Considerations for Devices ##

http://msdn2.microsoft.com/en-us/library/x5b31f9d.aspx

Unlike the full .NET Framework, the .NET Compact Framework has a limited space for providing resources to localize or globalize applications. These resources include sort tables, date format converters, string comparers, and other globalization utilities.

Developers should determine if the operating system on the device can render their application appropriately. There can be differences between an application written for the full .NET Framework on a personal computer and an application written for the .NET Compact Framework on a device because globalization requirements and capabilities.

The .NET Compact Framework returns an ArgumentException if you create an instance of a CultureInfo that represents a culture unsupported by .NET Compact Framework or the device operating system.

Whenever possible, the .NET Compact Framework uses the native operating system to render content appropriate for the locale, such as using an appropriate font. It also defers to the device operating system for culturally correct string comparison and character casing, such as when you use Compare, ToUpper and ToLower.
Current Culture Settings

You cannot set current culture programmatically on a device. They are set by the device manufacturer or manually configurable by the device user, such as with Regional Settings on a Pocket PC running Windows CE or Windows CE .NET, current culture settings are on a per-device basis.

An application uses the device locale setting when it starts. Its value is reflected by the CurrentCulture and CurrentUICulture properties. These properties are read-only in the .NET Compact Framework.

If the device operating system supports Multilingual User Interface (MUI), the .NET Compact Framework accommodates the separate UI language setting and reflects its value in CurrentUICulture If the device does not support MUI, CurrentUICulture defaults to CurrentCulture.

The .NET Compact Framework does not support the CurrentCulture and CurrentUICulture properties for a Thread, as culture settings are per-device and not per-thread.
Localization Design Considerations for Devices

You should consider the following support and behaviors when localizing smart device applications.

  * 

> Calendars

> The .NET Compact Framework supports only Gregorian-based calendars and uses the Gregorian calendar by default. Hebrew or Hijri calendars are not supported.
  * 

> String comparisons

> In some cases, strings can be compared differently from the full .NET Framework because of device operating system differences. The IndexOf, LastIndexOf, IsPrefix and IsSuffix of a CompareInfo object can evaluate incorrectly if the passed strings contain compression characters.
  * 

> User overrides

> Some .NET Compact Framework default values, obtained from internal globalization tables, differ from defaults specified by operating system registry settings. For example, the .NET Compact Framework and full .NET Framework use four-digit years for U.S. English (us-EN) and Windows CE .NET uses two digits. The.NET Compact Framework default values take precedence over device operating system default values.

> An application obtains override values during its initialization, so any value changes after that moment are ignored.
  * 

> Encoding

> The .NET Compact Framework supports character encoding on all devices: Unicode (BE and LE), UTF8, UTF7, and ASCII.

> There is limited support for code page encoding and only if the encoding is recognized by the operating system of the device.

> The .NET Compact Framework throws a PlatformNotSupportedException if the a required encoding is not available on the device.

> If the optional component Mlang.dll is on the device, the following code pages are supported: CP 51932 (EUC-JP), CP 50220 (ISO2022JP), and CP 50221 (cslSO2022JP).
  * 

> Surrogate pairs, changing case

> The .NET Compact Framework uses native Windows CE functions to change characters to upper or to lower case; unlike the full .NET Framework which provides this functionality in the Framework.

> Windows CE does not provide changing the case of surrogate pairs, so this feature is not supported in the .NET Compact Framework.
  * 

> Sorting

> The .NET Compact Framework uses native Windows CE functions to perform sort operations instead of managed code algorithms. This may produce different results when compared to a desktop application for that locale.

See Also
Other Resources

