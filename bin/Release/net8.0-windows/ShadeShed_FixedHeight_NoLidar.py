import arcpy
import numpy
import string
import random
import sys
import os
from math import sin, cos, radians, tan, degrees
#====================================================================================
def randomString(stringLength):
    """Generate a random string of fixed length """
    letters = string.ascii_lowercase
    return ''.join(random.choice(letters) for i in range(stringLength))

#====================================================================================
def getNewCoordinates(x,y,az,dist):
    return x + dist*cos(az), y - dist*sin(az)
    


def main():
    arcpy.env.overwriteOutput = True
    inShp = sys.argv[1]
    output = sys.argv[2]
    treeHt = sys.argv[3]

    tempName = randomString(6)
    #inShp = 'F:\\Netrace\\hnh\\Doug_buffers\\HardRock_canopy_stations\\canopy_stations.shp'
    
    #inShp = 'd:\\Netrace\\hnh\\Doug_buffers\\TypeN_Tribs_FeatureVerticesT.shp'
    #table = 'd:\\Netrace\\hnh\\Doug_buffers\\sun_table_8-1.csv'
    outPath = os.path.dirname(output)
    outName = os.path.basename(output)
    
    if arcpy.Exists(outPath + outName):
        arcpy.Delete_management(outPath + outName)
    
    #f = open(table)
    #Lines = f.read().splitlines()
    #f.close()
    
    SunTable6 = [[42.6,112.2],[44.9,116],[47.2,120],[49.3,124.2],[51.4,128.8],[53.3,133.8],[55.1,139.1],[56.6,144.8],[58,150.9],[59.1,157.4],[59.9,164.3],[60.5,171.4],[60.7,178.6],[60.6,185.9],[60.2,193.1],[59.4,200],[58.4,206.7],[57.1,212.9],[55.6,218.8],[53.9,224.3],[52.1,229.3],[50.1,234],[48,238.4],[45.7,242.5],[43.4,246.3]]
    
    
    
    inputPoints = arcpy.MakeFeatureLayer_management(inShp, tempName)
    sr = arcpy.Describe(tempName).spatialReference
    print (sr.linearUnitName)
    points = []
    for row in arcpy.da.SearchCursor(inputPoints, ["SHAPE@XY"]):
        points.append(row[0])
        
    outFC = arcpy.CreateFeatureclass_management(outPath, outName, 'POLYGON', None, None, None, sr)
    arcpy.AddField_management(outFC, "minDist", 'FLOAT', 5, 2, 0)
    arcpy.AddField_management(outFC, "maxDist", 'FLOAT', 5, 2, 0)
    arcpy.AddField_management(outFC, "minSolAlt", 'FLOAT', 5, 2, 0)
    arcpy.AddField_management(outFC, "maxSolAlt", 'FLOAT', 5, 2, 0)
    arcpy.AddField_management(outFC, "minSolAz", 'FLOAT', 5, 2, 0)
    arcpy.AddField_management(outFC, "maxSolAz", 'FLOAT', 5, 2, 0)
    cursor = arcpy.da.InsertCursor(outFC, ["SHAPE@","minDist","maxDist","minSolAlt","maxSolAlt","minSolAz","maxSolAz"])

    treeHeight = float(treeHt)

    ptNum = 0
    ptCount = len(points)
    for point in points:
        ptNum += 1
        array = arcpy.Array()
        x = point[0]
        y = point[1]
        array.add(arcpy.Point(x,y))
        minDist = 500
        maxDist = 0
        maxAz = 0
        minAlt = 2
        maxAlt = 0
        minAz = 3
        maxAz = 0
        
        print ("point number " + str(ptNum) + " out of " + str(ptCount))
        
        for line in SunTable6:
            if not line == '':
                #print "line is " + line
                alt = radians(float(line[0]))
                az = radians((float(line[1])) - 90)
                dist = (treeHeight / tan(alt))
                if dist > maxDist:
                    maxDist = dist
                if dist < minDist:
                    minDist = dist
                if alt > maxAlt:
                    maxAlt = alt
                if alt < minAlt:
                    minAlt = alt
                if az > maxAz:
                    maxAz = az
                if az < minAz:
                    minAz = az                    
                newCoords = getNewCoordinates(x, y, az, dist)
                array.add(arcpy.Point(newCoords[0],newCoords[1]))
        
        polygon = arcpy.Polygon(array, sr)
        minAlt = degrees(minAlt)
        maxAlt = degrees(maxAlt)
        minAz = degrees(minAz) + 90
        maxAz = degrees(maxAz) + 90
        
        cursor.insertRow([polygon,minDist,maxDist,minAlt,maxAlt,minAz,maxAz])
        
    print ("All done.  Output file = " + outPath + outName)


#=================================================================================    
if __name__ == '__main__':
    main()
else:
    print( "")