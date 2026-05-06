using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Core.Geoprocessing;

namespace SmartBuffer
{
  /// <summary>
  /// Interaction logic for winSmartBuffer.xaml
  /// </summary>
  public partial class winSmartBuffer : ArcGIS.Desktop.Framework.Controls.ProWindow
  {
    public winSmartBuffer()
    {
      InitializeComponent();


      RefreshForm();

      if (ClassGeneral.FindMe() == false) { return; }
      txtNetMapProgramFiles.Text = ClassGeneral.SMPath;
      txtSBDirectory.Text = ClassGeneral.SMPath;

      this.Dispatcher.Invoke(new Action(() =>
      {
        cboLat.Items.Add("North");
        cboLat.Items.Add("South");
        cboLat.SelectedIndex = 0;
      }));



    }

  

    private async void cmdRefreshReach_Click(object sender, RoutedEventArgs e)
    {
      await QueuedTask.Run(() => RefreshForm());
    }


    private async void RefreshForm()   //need to clear cbo layer list.
    {

      try
      {
        await QueuedTask.Run(() =>
        {
          var mapView = MapView.Active;
          if (mapView == null)
            return;

          Map activeMap = mapView.Map;
          var polylineFeatureLayers = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                      lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline).ToList();
          this.Dispatcher.Invoke(new Action(() =>
                  {
                    cboReachLayer.Items.Clear();
                    foreach (var pline in polylineFeatureLayers)
                    {
                      cboReachLayer.Items.Add(pline);
                    }
                  }));

          string[] fileList = System.IO.Directory.GetFiles(ClassGeneral.SMPath, "*.shp");
          this.Dispatcher.Invoke(new Action(() =>
          {
            lstLayers.Items.Clear();
            foreach (string f in fileList)
            {
              string fname = Path.GetFileName(f);
              if (!fname.Contains("point") & !fname.Contains("polys"))
              {
                lstLayers.Items.Add(fname);
              }
            }
          }));

        });
      }
      catch (Exception ex)
      {
        //ErrorLog(ex.ToString());
        GC.Collect();
        //return null;
      }

    }


    private void btProgramFiles_Click_1(object sender, RoutedEventArgs e)
    {

      try
      {


        Debug.Print("second one");
        string currentDirectory = "C:\\";
        if (txtNetMapProgramFiles.Text != "Browse to select folder")
        {
          currentDirectory = txtNetMapProgramFiles.Text;
        }
        var dlg = new CommonOpenFileDialog();
        dlg.Title = "Select a folder for Smart Buffers:";
        dlg.IsFolderPicker = true;
        dlg.InitialDirectory = currentDirectory;

        dlg.AddToMostRecentlyUsedList = false;
        dlg.AllowNonFileSystemItems = false;
        dlg.DefaultDirectory = currentDirectory;
        dlg.EnsureFileExists = true;
        dlg.EnsurePathExists = true;
        dlg.EnsureReadOnly = false;
        dlg.EnsureValidNames = true;
        dlg.Multiselect = false;
        dlg.ShowPlacesList = true;

        if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
        {
          var folder = dlg.FileName;
          if (folder != null)
          {
            if (!Path.EndsInDirectorySeparator(folder))
            {
              folder += Path.DirectorySeparatorChar;
            }
            txtSBDirectory.Text = folder;

            ClassGeneral.SMPath = folder;
            string inString = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string pathSplit = inString.Split('{')[0];
            string configFile = pathSplit + "config.SB";
            using (StreamWriter writer = new StreamWriter(configFile))
            {
              writer.WriteLine(folder);
            }
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Your file paths have been saved to " + configFile);


          }
          // Do something with selected folder string
        }
      }
      catch (Exception ex)
      {
        //ErrorLog(ex.ToString());
        ClassGeneral.ErrorLog(ex.ToString());
        GC.Collect();
      }


    }

    private async void go_Click(object sender, RoutedEventArgs e)
    {

      try
      {
        string WD = ClassGeneral.SMPath;
        if (cboReachLayer.SelectedItem == null) { return; }

        

        string FeatureLayerName = cboReachLayer.SelectedItem.ToString();

        FeatureLayer FL = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().First(l => l.Name.Equals(FeatureLayerName));
        //bool bPRJ = false;
        //#Debug.Print(NMSpatialReference.Name);

        SpatialReference SRpts = null;

        await QueuedTask.Run(() => SRpts = FL.GetSpatialReference());

        //decimal dTreeHt = (decimal)numTreeHt.Value;
        double sTreeHt = (double)numTreeHt.Value;

        //bool convert = false;
        Debug.Print(SRpts.Unit.ToString());


        if (SRpts.Name.ToString().ToLower().Contains("wgs") | SRpts.Name.ToString().ToLower().Contains("geog"))
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Input layer must be in a coordinate system with X-Y values in meters or feet (not WGS).");
          return;       

        }

        if (SRpts.Unit.ToString().ToLower().Contains("foot"))
        {
          //convert = true;
          //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Input layer must be in a coordinate system with X-Y values in meters.");
          //return;        

          sTreeHt = sTreeHt * 3.281;
        }

        string TreeHt = Convert.ToString(sTreeHt);


        int pointSelCount = FL.SelectionCount;
        if (pointSelCount == 0)
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Need to have at least one reach selected");
          return;
        }
        else
        {
          MessageBoxResult MsgResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Lines selected: " + pointSelCount + ": Proceed?", "Create ShadeSheds", MessageBoxButton.OKCancel, MessageBoxImage.Question);
          if (MsgResult == MessageBoxResult.Cancel)
          {
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("cancelling");
            txtProgress.Text = "Cancelled";
            return;
          }

        }

        if (txtOutput.Text == "")
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Provide an output name");
          return;
        }

        string outName = txtOutput.Text;

        string outFile = ClassGeneral.SMPath + txtOutput.Text + ".shp";
        if (File.Exists(outFile))
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Output file exists");
          return;
        }

        this.IsEnabled = false;
        this.Cursor = System.Windows.Input.Cursors.Arrow;

        string outPoints = ClassGeneral.SMPath + txtOutput.Text + "_points.shp";
        string outPolys = ClassGeneral.SMPath + txtOutput.Text + "_polys.shp";

        txtProgress.Text = "Making points....";
        await QueuedTask.Run(async () =>
        {


          Selection fs = FL.GetSelection();

          if (fs.GetCount() > 0)
          {

            var mva1 = Geoprocessing.MakeValueArray(FL, outPoints, "DISTANCE", "1 Meters", "#", "END_POINTS", "NO_CHAINAGE", "#", "PLANAR");
            var cts = new System.Threading.CancellationTokenSource();
            await QueuedTask.Run(async () =>
            {
              var gpResult = await Geoprocessing.ExecuteToolAsync("GeneratePointsAlongLines_management", mva1, null, cts.Token, null, GPExecuteToolFlags.AddToHistory);

              return true;
            });

          }

        });


        txtProgress.Text = "Making Smart Buffers....";

        string inString = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string pathSplit = inString.Replace("SmartBuffer.dll", "");
        //string configFile = pathSplit + "config.SB";
        //if (System.IO.File.Exists(configFile))

        string lat = cboLat.SelectedItem.ToString();        

        string workingDir = ClassGeneral.SMPath;

        //var myArguments = "-c \"from Test import Main;Main('Hallo')\"";
        string scriptPath = pathSplit + "ShadeShed_FixedHeight_NoLidar.py";
        var myArguments = $@"""{scriptPath}"" ""{outPoints}"" ""{outPolys}""  ""{TreeHt}"" ""{lat}""";

        string pythonExe = Path.Combine(
             Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
             @"ArcGIS\Pro\bin\Python\envs\arcgispro-py3\python.exe");

        var process = new RunProcess();
        var processOutcome = process.RunProcessGrabOutput(pythonExe, myArguments, workingDir);

        if (!File.Exists(outPolys))
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Failure in making shadesheds, step 2.");
          this.IsEnabled = true;
          this.Cursor = System.Windows.Input.Cursors.Arrow;
          return;
        }

        txtProgress.Text = "Dissolving output....";
        var mva1 = Geoprocessing.MakeValueArray(outPolys, outFile, "#", "#", "MULTI_PART");
        var cts = new System.Threading.CancellationTokenSource();
        await QueuedTask.Run(async () =>
        {
          var gpResult = await Geoprocessing.ExecuteToolAsync("Dissolve_management", mva1, null, cts.Token, null, GPExecuteToolFlags.AddToHistory);

          return true;
        });

        txtProgress.Text = "Calculating area in hectares....";

        //add area_km2 field
        var mva2 = Geoprocessing.MakeValueArray(outFile, "Area_ha", "FLOAT");
        var cts2 = new System.Threading.CancellationTokenSource();
        await QueuedTask.Run(async () =>
        {
          var gpResult2 = await Geoprocessing.ExecuteToolAsync("AddField_management", mva2, null, cts2.Token, null, GPExecuteToolFlags.AddToHistory);
          return true;
        });

        mva2 = Geoprocessing.MakeValueArray(outFile, "Area_ha AREA", "#", "HECTARES"); //, classLayers.NMSpatialReference);
        cts2 = new System.Threading.CancellationTokenSource();
        await QueuedTask.Run(async () =>
        {
          var gpResult2 = await Geoprocessing.ExecuteToolAsync("CalculateGeometryAttributes_management", mva2, null, cts2.Token, null, GPExecuteToolFlags.AddToHistory);
          return true;
        });
        //Debug.Print("test?");


        if (File.Exists(outFile))
        {
          await ClassGeneral.AddLayer(outFile);

          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Output file " + outFile + " has been added to map.");
          this.IsEnabled = true;
          this.Cursor = System.Windows.Input.Cursors.Arrow;
          RefreshForm();
          txtProgress.Text = "Done with " + outFile;

          return;

        }
        else
        {
          //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Some kind of error occurred.");
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
       $"Exit {processOutcome.ErrCode}\n\nSTDERR:\n{processOutcome.Error}\n\nSTDOUT:\n{processOutcome.Output}");
          RefreshForm();
          this.IsEnabled = true;
          this.Cursor = System.Windows.Input.Cursors.Arrow;
          return;
        }

      }
      catch (Exception ex)
      {
        this.IsEnabled = true;
        this.Cursor = System.Windows.Input.Cursors.Arrow;
        ClassGeneral.ErrorLog(ex.ToString());
        GC.Collect();
        //return null;
      }



    }

    private async void cmdLoad_Click(object sender, RoutedEventArgs e)
    {
      try
      {

        foreach (var selecteditem in lstLayers.SelectedItems)
        {
          string sel = selecteditem.ToString().Replace(".shp", "");

          var lyrExists = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(f => f.Name == sel);


          //FeatureLayer fl = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().First(l => l.Name.Equals(sel));
          if (!lyrExists)
          {
            string lURI = ClassGeneral.SMPath + sel + ".shp";
            Layer llake = await ClassGeneral.AddLayer(lURI, true);

          }


        }

        RefreshForm();
      }



      catch (Exception ex)
      {
        ClassGeneral.ErrorLog(ex.ToString());
        GC.Collect();
        //return null;
      }
    }
  }
}