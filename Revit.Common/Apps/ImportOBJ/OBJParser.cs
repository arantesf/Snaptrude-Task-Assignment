using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Revit.Common
{
    enum UpAxis
    {
        Y,
        Z
    }

    public class OBJParser
    {
        public List<XYZ> Points { get; set; } = new List<XYZ>();
        private RevitGeometryObject CurrentRevitObject { get; set; } = null;
        private List<RevitGeometryObject> RevitObjects { get; set; } = new List<RevitGeometryObject>();
        private UpAxis UpAxis { get; set; } = UpAxis.Y;

        public List<RevitGeometryObject> Parse(string filePath)
        {
            ReadOnlySpan<string> lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
                ProcessLine(lines[i]);
            RevitObjects.Add(CurrentRevitObject);
            RevitObjects.ForEach(r => r.BuildGeometryObjects(this));

            return RevitObjects;
        }

        private void CreateNewObject(string name)
        {
            if (CurrentRevitObject == null)
            {
                CurrentRevitObject = new RevitGeometryObject(name);
            }
            else
            {
                RevitObjects.Add(CurrentRevitObject);
                CurrentRevitObject = new RevitGeometryObject(name);
            }
        }

        private void ProcessLine(string line)
        {
            if (line == "")
                return;
            switch (line[0])
            {
                case 'o':
                    CreateNewObject(line.Substring(2));
                    break;
                case 'g':
                    CreateNewObject(line.Substring(2));
                    break;
                case 'v':
                    {
                        switch (line[1])
                        {
                            case ' ':
                                {
                                    Points.Add(ParsePoint(line.Substring(2)));
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                    break;
                case 'f':
                    CurrentRevitObject.FacesIndexes.Add(ParseFace(line.Substring(2)));
                    return;
                case '#':
                    return;
            }
        }

        private XYZ ParsePoint(string slice)
        {
            string[] coordinateStrings = slice.Split(' ').Where(s => s != "").ToArray();
            double resX = double.Parse(coordinateStrings[0].Replace('.', ','));
            double resY = double.Parse(coordinateStrings[1].Replace('.', ','));
            double resZ = double.Parse(coordinateStrings[2].Replace('.', ','));
            return UpAxis == UpAxis.Z ? new XYZ(resX, resY, resZ) : new XYZ(resX, -resZ, resY);
        }

        private List<int> ParseFace(string slice)
        {
            List<int> pointIndexes = new List<int>();
            string[] facePointsStrings = slice.Split(' ').Where(s => s != "").ToArray();
            foreach (var facePointString in facePointsStrings)
            {
                if (!facePointString.Contains("/"))
                {
                    pointIndexes.Add(int.Parse(facePointString));
                }
                else if (facePointString.Contains("//"))
                {
                    string[] separator = { "//" };
                    int pointIndex = int.Parse(facePointString.Split(separator, StringSplitOptions.None).First());
                    pointIndexes.Add(pointIndex);
                }
                else
                {
                    int pointIndex = int.Parse(facePointString.Split('/').First());
                    pointIndexes.Add(pointIndex);
                }
            }
            return pointIndexes;
        }
    }
}
