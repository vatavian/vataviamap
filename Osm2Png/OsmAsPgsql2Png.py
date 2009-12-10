#!\python25\python
import subprocess
import mapnik

renderer = ['osm'] #,'decatur']
scale = ['240000','120000','60000','24000','12000','6000','2400','1200','600','240']
paperSize = ['Tab','Ltr','D','E'] 
centerX = -9383800
centerY =  3998600
adjMercMeters = 1.25
dpi = 96.0
inchesPerMeter = 39.37

for p in paperSize:
  if p == 'Ltr'  :  widthPaperX = 7.5; heightPaperY = 10.0   # 8.5x11
  elif p == 'Tab':  widthPaperX = 10.0; heightPaperY = 16.0  # 11x17
  elif p == 'D'  :  widthPaperX = 23.0; heightPaperY = 35.0  # 24x36
  elif p == 'E'  :  widthPaperX = 35.0; heightPaperY = 47.0  # 36x48

  for r in renderer:
    mapSizeX = int(dpi * widthPaperX)
    mapSizeY = int(dpi * heightPaperY)
    m = mapnik.Map(mapSizeX , mapSizeY)
    mapnik.load_map(m, r + '.xml')
    for s in scale:
      sFactor = 0.5 * float(s) * adjMercMeters / inchesPerMeter
      dX = int(widthPaperX  * sFactor)
      dY = int(heightPaperY * sFactor)
      bbox = mapnik.Envelope(mapnik.Coord(centerX - dX, centerY - dY), mapnik.Coord(centerX + dX, centerY + dY))
      m.zoom_to_box(bbox)
    
      f= "decatur_" + p + "_" + r + "_" + s + ".png"
      mapnik.render_to_file(m, f)

      #p = subprocess.Popen(["C:\Program Files (x86)\IrfanView\i_view32.exe",f])
