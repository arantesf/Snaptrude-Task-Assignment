using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CsvHelper.Configuration;
using CsvHelper;

namespace Revit.Common
{
    [Transaction(TransactionMode.Manual)]
    public class ImportOBJEC : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // -------------------- GET ACTIVE DOCUMENT
                UIApplication uiapp = commandData.Application;
                Document doc = uiapp.ActiveUIDocument.Document;

                OBJParser oBJParser = new OBJParser();

                // -------------------- SHOW OPEN FILE DIALOG TO SELECT THE OBJ FILEPATH
                using (OpenFileDialog saveFileDialog = new OpenFileDialog() { Filter = "OBJ file|*.obj", Title = "Import OBJ" })
                {
                    switch (saveFileDialog.ShowDialog())
                    {
                        case DialogResult.OK:
                            // -------------------- PARSE THE OBJ FILE
                            List<RevitGeometryObject> revitGeometryObjects = oBJParser.Parse(Path.GetFullPath(saveFileDialog.FileName));

                            // -------------------- CREATE REVIT ELEMENTS MODEL IN PLACE
                            using (Transaction trans = new Transaction(doc, "Import OBJ"))
                            {
                                trans.Start();
                                foreach (var revitGeometryObject in revitGeometryObjects)
                                {
                                    DirectShape directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                                    directShape.SetShape(revitGeometryObject.GeometryObjects);
                                    directShape.Name = revitGeometryObject.Name;
                                }
                                trans.Commit();
                            }
                            break;
                        default:
                            break;
                    }

                    return Result.Succeeded;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"That was an error importing the OBJ geometry. {ex.Message} {ex.StackTrace}");
                return Result.Cancelled;

            }
        }
    }
}
