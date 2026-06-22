import arcpy
import arcpy.management
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
    region = sys.argv[4]

    tempName = randomString(6)
    #inShp = 'F:\\Netrace\\hnh\\Doug_buffers\\HardRock_canopy_stations\\canopy_stations.shp'
    
    #inShp = 'd:\\Netrace\\hnh\\Doug_buffers\\TypeN_Tribs_FeatureVerticesT.shp'
    #table = 'd:\\Netrace\\hnh\\Doug_buffers\\sun_table_8-1.csv'
    outPath = os.path.dirname(output)
    outName = os.path.basename(output)
    
    #inShp
    outSR = arcpy.SpatialReference(26910)

    if arcpy.Exists(outPath + outName):
        arcpy.Delete_management(outPath + outName)
    
    #f = open(table)
    #Lines = f.read().splitlines()
    #f.close()
    
    if region == "North":
      SunTable6 = [[42.861092,113.502283],
                  [45.11092,117.338017],
                  [47.281642,121.416079],
                  [49.356688,125.768309],
                  [51.317044,130.426688],
                  [53.141083,135.421103],
                  [54.80459,140.775934],
                  [56.281113,146.505348],
                  [57.542793,152.607524],
                  [58.561803,159.058612],
                  [59.312417,165.808035],
                  [59.773511,172.777282],
                  [59.931079,179.864069],
                  [59.780129,186.952238],
                  [59.325397,193.925459],
                  [58.580668,200.680977],
                  [57.5669,207.139637],
                  [56.309721,213.250188],
                  [54.836917,218.988182],
                  [53.176349,224.351337],
                  [51.354504,229.353509],
                  [49.395645,234.018892],
                  [47.321452,238.377284],
                  [45.150992,242.460649],
                  [42.900881,246.300847]]
    elif region == "South":
        SunTable6= [[43.122483,111.048356],	
                  [45.504625,114.746924],	
                  [47.814437,118.69564],	
                  [50.035411,122.932372],	
                  [52.148208,127.497172],	
                  [54.130288,132.43028],	
                  [55.955683,137.768653],	
                  [57.595093,143.540559],	
                  [59.016512,149.758083],	
                  [60.186655,156.408156],	
                  [61.073324,163.443986],	
                  [61.648657,170.78016],	
                  [61.892724,178.295183],	
                  [61.796575,185.843432],	
                  [61.363735,193.274574],	
                  [60.609604,200.454538],	
                  [59.558982,207.281252],	
                  [58.242586,213.691252],	
                  [56.693556,219.657628],	
                  [54.944673,225.18259],	
                  [53.026528,230.288353],	
                  [50.966573,235.00886],	
                  [48.788829,239.383446],	
                  [46.513988,243.452565],	
                  [44.159737,247.255211]]
    
    
    inputPoints = arcpy.MakeFeatureLayer_management(inShp, tempName)
    sr = arcpy.Describe(tempName).spatialReference
    print (sr.linearUnitName)

    points = []
    for row in arcpy.da.SearchCursor(inputPoints, ["SHAPE@XY"]):
        points.append(row[0])
        
    outFC = arcpy.CreateFeatureclass_management(outPath, outName, 'POLYGON', None, None, None, outSR)
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