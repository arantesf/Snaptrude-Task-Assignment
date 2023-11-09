using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using Autodesk.Revit.DB.Architecture;

namespace Revit.Common
{
    /// <summary>
    /// Window that shows the progress of the command
    /// </summary>
    public partial class StatusWindow : Window
    {
        public StatusWindow MainView;
        public StatusWindowViewModel StatusWindowViewModel { get; set; }
        public StatusWindow()
        {
            InitializeComponent();
        }

        public Action teste;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            teste();
        }
    }
    public partial class RoomDataExtractionStatusWindow : StatusWindow
    {
        public List<Room> AllRooms { get; set; }
        public List<FamilyInstance> FamilyInstances { get; set; }
        public string FilePath { get; set; }

        public RoomDataExtractionStatusWindow(int maxValue, List<Room> allRooms, List<FamilyInstance> familyInstances, string filePath)
        {
            MainView = this;
            AllRooms = allRooms;
            FamilyInstances = familyInstances;
            FilePath = filePath;
            StatusWindowViewModel = new StatusWindowViewModel("Extract Room Data", maxValue);
            InitializeComponent();
            teste = ExtractData;
        }
        private void ExtractData()
        {
            // -------------------- GET ALL ROOMS DATA
            List<RoomData> roomsData = new List<RoomData>();
            foreach (var room in AllRooms)
            {
                roomsData.Add(new RoomData(room, FamilyInstances));
                StatusWindowViewModel.ProgressBarValue++;
            }

            // -------------------- WRITE CSV FILE
            using (StreamWriter streamWriter = new StreamWriter(Path.GetFullPath(FilePath)))
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
            Process.Start(Path.GetFullPath(FilePath));
            Close();
        }
    }
}
