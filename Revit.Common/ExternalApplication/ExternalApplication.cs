using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SnaptrudeResources;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Revit.Common
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonButtons(application);

            return Result.Succeeded;
        }

        private void AddRibbonButtons(UIControlledApplication application)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string executingAssemblyPath = assembly.Location;
            Debug.Print(executingAssemblyPath);
            string executingAssemblyName = assembly.GetName().Name;
            Console.WriteLine(executingAssemblyName);
            string tabName = "FCA Task Assignment";

            try { application.CreateRibbonTab(tabName); } catch (Exception) { }

            RibbonPanel dataPanel = application.CreateRibbonPanel(tabName, "Data Extraction");
            RibbonPanel importPanel = application.CreateRibbonPanel(tabName, "Import");

            BitmapImage bitmapImage = ResourceImage.GetIcon("Snaptrude-32px.png");
            
            PushButtonData roomDataExtractorPBD = new PushButtonData("RoomDataExtractor", "Room Data\nExtractor", executingAssemblyPath, typeof(RoomDataExtractionEC).FullName)
            {
                ToolTip = "Extracts rooms data and save as a .csv file.",
                LargeImage = bitmapImage
            };
            dataPanel.AddItem(roomDataExtractorPBD);

            PushButtonData objGeometryImporterPBD = new PushButtonData("OBJImporter", "OBJ Import", executingAssemblyPath, typeof(ImportOBJEC).FullName)
            {
                ToolTip = "Import OBJ files as Revit Model In Place elements.",
                LargeImage = bitmapImage
            };
            importPanel.AddItem(objGeometryImporterPBD);
        }
    }
}