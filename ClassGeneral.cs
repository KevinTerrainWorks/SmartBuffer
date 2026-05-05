using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartBuffer
{
  internal class ClassGeneral
  {
    public static string SMPath = "";

    public static void ErrorLog(string logMessage)
    {

      try
      {

        Debug.Print(ClassGeneral.SMPath);


        string LogFile = ClassGeneral.SMPath + "NetMapPro_Errors.log";

        if (ClassGeneral.SMPath == null | ClassGeneral.SMPath.Length == 0)
        {
          LogFile = ClassGeneral.SMPath + "NetMapPro_Errors.log";
        }


        using (StreamWriter w = File.AppendText(LogFile))
        {
          w.Write("\r\nLog Entry : ");
          w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
          w.WriteLine("  :");
          w.WriteLine($"  :{logMessage}");
          w.WriteLine("-------------------------------");
        }

        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error has been logged to " + LogFile);

        //w.Write("\r\nLog Entry : ");
        //w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
        //w.WriteLine("  :");
        //w.WriteLine($"  :{logMessage}");
        //w.WriteLine("-------------------------------");
      }
      catch (Exception)
      {
        //ErrorLog(ex.ToString());
        GC.Collect();
        //return null;
      }

    }
    public static void WriteToLog(string logMessage)
    {

      //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(NetMap_FilePath + "NetMapPro_Events.log");
      try
      {

        //return;
        //string outPath = @"C:\temp\";

        //if (NetMap_FilePath != "")
        //{
        //  outPath = NetMap_FilePath;

        //}
        //else
        //{
        //  return;
        //}

        //string LogFile = outPath + "NetMapPro_Events.log";

        //using (StreamWriter w = File.AppendText(LogFile))
        //{
        //  w.Write("\r\nLog Entry : ");
        //  w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
        //  w.WriteLine("  :");
        //  w.WriteLine($"  :{logMessage}");
        //  w.WriteLine("-------------------------------");
        //}

      }
      catch (Exception)
      {
        //ErrorLog(ex.ToString());
        GC.Collect();
        //return null;
      }

    }

    public async static Task<bool> CreatePoints(string inLines, string outPoints)
    {
      try
      {
        //FileGeodatabaseConnectionPath fileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath + gd));


        //arcpy.management.GeneratePointsAlongLines(
    //    Input_Features = "Hydrography_Statewide_Flow_Line  TW",
    //Output_Feature_Class = r"C:\Users\Kevin1024\Documents\ArcGIS\Projects\SouthSantiam Conflation\Default.gdb\Hydrography_Statewide_Flow_LineTW_GeneratePointsAlongLines",
    //Point_Placement = "DISTANCE",
    //Distance = "1 Meters",
    //Percentage = None,
    //Include_End_Points = "NO_END_POINTS",
    //Add_Chainage_Fields = "NO_CHAINAGE",
    //Distance_Field = None,
    //Distance_Method = "PLANAR"
//)



        var mva1 = Geoprocessing.MakeValueArray(inLines, outPoints, "DISTANCE", "1 Meters", "#", "END_POINTS", "NO_CHAINAGE", "#", "PLANAR");
        var cts = new System.Threading.CancellationTokenSource();
        return await QueuedTask.Run(async () =>
        {
          var gpResult = Geoprocessing.ExecuteToolAsync("GeneratePointsAlongLines_management", mva1, null, cts.Token, null, GPExecuteToolFlags.AddToHistory);

          return true;
        });



      }
      catch (Exception ex)
      {
        ErrorLog(ex.ToString());
        GC.Collect();
        return false;
      }


    }

    public static Boolean FindMe(bool skipL = false)  //public static Boolean FindMe(bool skipL = false)
    {
      try
      {


        //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"popup 2");

        ////ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"message 4");
        //WriteToLog("find me  188");


        var mapView = MapView.Active;

        //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"popup 3");

        if (mapView == null)
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"NetMap requires a Map to be the active view.");
          return false;
        }

        string inString = System.Reflection.Assembly.GetExecutingAssembly().Location;

        bool isFound = false;
        if (Path.Exists(inString))
        {
          isFound = true;
        }

        bool isConfigured = false;

        //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"add-in folder: " + inString + " found: " + isFound.ToString());
        string pathSplit = inString.Split('{')[0];
        string configFile = pathSplit + "config.SB";
        if (System.IO.File.Exists(configFile))
        {
          ////ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"message 6 " + configFile);
          using (StreamReader sr = File.OpenText(configFile))
          {
            SMPath = sr.ReadLine();
            
          }
          isConfigured = true;
          //return true;
        }
        else
        {
          ////ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"message 7");
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Set up default path.");
          isConfigured = false;
          return false;
        }


        return isConfigured;


      }
      catch (Exception ex)
      {
        ErrorLog(ex.ToString());
        GC.Collect();
        return false;
      }


    }

    public static async Task<Layer> AddLayer(string uri, bool newMetadata = false)
    {
      try
      {

        Layer lyr = null;
        return await QueuedTask.Run(() =>
        {
          Map map = MapView.Active.Map;
          lyr = LayerFactory.Instance.CreateLayer(new Uri(uri), map);
          if (newMetadata)
          {
            FeatureLayer fl = (FeatureLayer)lyr; // LayerFactory.Instance.CreateLayer(lyr);

          }
          return lyr;
        });

        //return lyr;

      }
      catch (Exception ex)
      {
        Debug.Print(ex.Message);
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(uri + " cannot be loaded.");
        GC.Collect();
        return null;
      }
    }



  }
}
