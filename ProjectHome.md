#### VataviaMap Desktop ####

[Download latest executable](http://vataviamap.googlecode.com/svn/trunk/VataviaMap/bin/Release/VataviaMapDesktop.exe) This is not a package, just download and run. (requires [.Net framework version 2.0](http://www.microsoft.com/downloads/details.aspx?FamilyId=0856EACB-4362-4B0D-8EDD-AAB15C5E04F5&displaylang=en) or later.)

**Features**
  * Choice of map service ([OpenStreetMap](http://www.openstreetmap.org/index.html), [MapQuest Open](http://open.mapquest.com/), [Yahoo! Maps](http://maps.yahoo.com/), [Bing Maps](http://bing.com/maps/))
  * Displays GPS tracks from [.gpx files](http://en.wikipedia.org/wiki/GPS_eXchange_Format)
  * Displays points of interest such as [geocaches](http://www.geocaching.com) from .gpx or .loc files
  * After viewing an area once, caching makes viewing that same area again very fast
  * Pan by dragging map or arrow keys
  * [Find buddies](FindBuddies.md) who are uploading their positions
  * Multi-threaded so user interface remains responsive while downloading maps
  * Designed for use on desktop or laptop computers with mouse or trackpad
  * Zoom with scroll wheel or Zoom menu
  * Open .gpx or .loc files by dragging files onto map or File/Add Layer menu
  * Automatically pan and zoom to extents of new layer
  * Save current view as .png
  * Open web browser with [OpenStreetMap](http://www.openstreetmap.org/index.html) or other map websites zoomed to current location
  * Open [JOSM](http://josm.openstreetmap.de/) ([OpenStreetMap](http://www.openstreetmap.org/index.html) editor) with current .gpx file(s)
  * Developed and tested on Windows
  * Some successful tests completed on Linux and OS X via Mono

![http://vataviamap.googlecode.com/svn/trunk/VataviaMap/Images/VataviaMapDesktop.png](http://vataviamap.googlecode.com/svn/trunk/VataviaMap/Images/VataviaMapDesktop.png)

---

**Source Code**

All source code is available as open source.
VataviaMap is written in VB.Net.

VataviaMap Desktop was originally developed using the free Visual Basic 2005 Express Edition. Newer versions of VB.Net Express and Visual Studio will also work.

This project was formerly hosted on the [MapWindow svn server](http://svn.mapwindow.org/svnroot/OSM_VBnet) under the name OSM\_VBnet. Versions before 17 May 2009 may still be found there.


---


**VataviaMap Mobile**

This version runs on Windows Mobile 5 or 6 devices which are now obsolete.

<img src='http://vataviamap.googlecode.com/svn/trunk/VataviaMap/Images/VataviaMapMobile.png' align='right'>

<ul><li><a href='http://vataviamap.googlecode.com/svn/trunk/VataviaMap/VataviaMapMobileInstaller/Release/VataviaMapMobileInstaller.CAB'>Download latest CAB installer</a>
</li><li>Can record cell tower ID on devices with active GSM phone<br>
</li><li>Devices with access to a GPS can use GPS Features:<br>
<ul><li>Current position can be marked on the map<br>
</li><li>Map can be scrolled automatically to keep current position in view<br>
</li><li>Track of journey can be recorded to .gpx file<br>
</li><li>Can send current position to a web site<br>
</li></ul></li><li>Tested devices:<br>
<ul><li>Developed using AT&T Tilt<br>
</li><li>Tested on AT&T Fuze (high screen resolution makes map details smaller)</li></ul></li></ul>

VataviaMap Mobile is developed with Visual Studio 2005 Standard Edition. Express Edition will not work. Also required is the Windows Mobile 5.0 SDK for Smartphone <a href='http://www.microsoft.com/downloads/details.aspx?familyid=DC6C00CB-738A-4B97-8910-5CD29AB5F8D9&displaylang=en'>downloadable from Microsoft</a>

GPS_API is a C# library used to provide GPS and other Windows Mobile functions. GPS_API is based on example code from Microsoft named Microsoft.WindowsMobile.Samples.Location. The GPS Intermediate Driver, introduced in Windows Mobile 5, is used. This driver allows sharing of a GPS by multiple applications. A big change would be needed to support GPS devices that cannot be accessed through the intermediate driver.<br>
