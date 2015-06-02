# Finding a Buddy #

Your buddy must be locatable by retrieving a URL that results in one of the recognized formats:

  * [Navizon](http://www.navizon.com/) public XML link: log in, visit "Display the location of users in my group in Browser" then "Publish location to public URL" then copy the link at the end of the sentence: "You can also access your location in XML format here." NOTE: Do not use the link labeled Hyperlink or Button. Sample URL:

http://my.navizon.com/Webapps/UserAdmin/UserService.asmx/GetUserLocation?user=XXXX

  * OpenDMTP csv point including at least ServerDate,ServerTime,Label,Latitude,Longitude.
```
Format of OpenDMTP csv point:
ServerDate,ServerTime,Label,Latitude,Longitude,Knots,Heading,Elevation,GPStime,CellID
Example:
2008/05/14,07:56:39,TestPoint,33.77080,-84.31358,27.0,300.7,300.0,2009-05-18T17:45:08.000Z,31999.4524.310.410
```

# Letting your buddies find you #

Options:
  * Run InstaMapper to send your location
  * Run Navizon to send your location
  * Use the upload option in VataviaMap Mobile to send to a server that makes your points available in a supported format. TODO: Document how the development server accepts and publishes points.