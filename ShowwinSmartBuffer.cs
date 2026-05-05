using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuffer
{
    internal class ShowwinSmartBuffer : Button
    {

        private winSmartBuffer _winsmartbuffer = null;

        protected override void OnClick()
        {
            //already open?
            if (_winsmartbuffer != null)
                return;
            _winsmartbuffer = new winSmartBuffer();
            _winsmartbuffer.Owner = FrameworkApplication.Current.MainWindow;
            _winsmartbuffer.Closed += (o, e) => { _winsmartbuffer = null; };
            _winsmartbuffer.Show();
            //uncomment for modal
            //_winsmartbuffer.ShowDialog();
        }

    }
}
