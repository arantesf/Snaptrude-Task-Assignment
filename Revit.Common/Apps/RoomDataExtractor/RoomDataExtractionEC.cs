using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using CsvHelper;
using CsvHelper.Configuration;

namespace Revit.Common
{
    [Transaction(TransactionMode.Manual)]
    public class RoomDataExtractionEC : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // -------------------- GET ACTIVE DOCUMENT
                UIApplication uiapp = commandData.Application;
                Document doc = uiapp.ActiveUIDocument.Document;

                // -------------------- GET ALL BUILTINCATEGORIES THAT ARE NOT DOORS OR WINDOWS
                List<BuiltInCategory> occupantObjectsCategories = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_Furniture,
                    BuiltInCategory.OST_FurnitureSystems,
                    BuiltInCategory.OST_MechanicalEquipment,
                    BuiltInCategory.OST_GenericModel,
                    BuiltInCategory.OST_PlumbingFixtures,
                    BuiltInCategory.OST_Casework,
                    BuiltInCategory.OST_SpecialityEquipment
                };

                // -------------------- GET ALL FAMILYINSTANCES THAT ARE NOT DOORS OR WINDOWS
                IEnumerable<FamilyInstance> familyInstances = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(FamilyInstance))
                    .WherePasses(new ElementMulticategoryFilter(occupantObjectsCategories))
                    .Cast<FamilyInstance>();

                // -------------------- GET ALL ROOMS
                List<Room> allRooms = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .Cast<Room>()
                    .Where(room => room.Area != 0)
                    .ToList();

                // -------------------- SHOW SAVE FILE DIALOG TO SELECT THE CSV FILEPATH
                using (SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "CSV File|*.csv", Title = "Export Room Data", FileName = $"{doc.Title} - RoomDataExtraction" })
                {
                    switch (saveFileDialog.ShowDialog())
                    {
                        case DialogResult.OK:
                            List<RoomData> roomsData = new List<RoomData>();
                            foreach (var room in allRooms)
                            {
                                roomsData.Add(new RoomData(room, familyInstances));
                            }
                            // -------------------- WRITE CSV FILE
                            using (StreamWriter streamWriter = new StreamWriter(Path.GetFullPath(saveFileDialog.FileName)))
                            {
                                var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                                {
                                    HasHeaderRecord = true,
                                };
                                using (CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration))
                                {
                                    csvWriter.WriteRecords(roomsData);
                                }
                            }
                            Process.Start(Path.GetFullPath(saveFileDialog.FileName));
                            break;
                        default:
                            break;
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"That was an error extracting the room data. {ex.Message} {ex.StackTrace}");
                return Result.Cancelled;
            }
        }
    }
}
