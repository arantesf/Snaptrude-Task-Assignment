using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit.Common
{
    public class RevitGeometryObject
    {
        public string Name { get; set; }
        public List<List<int>> FacesIndexes { get; set; } = new List<List<int>>();
        public List<GeometryObject> GeometryObjects { get; set; } = new List<GeometryObject>();
        public RevitGeometryObject(string name)
        {
            Name = name;
        }

        public void BuildGeometryObjects(OBJParser oBJParser)
        {
            TessellatedShapeBuilder shapeBuilder = new TessellatedShapeBuilder();
            shapeBuilder.OpenConnectedFaceSet(true);
            //FacesIndexes = FacesIndexes.Select(indexes => indexes.Select(index => index -= PointStartIndex).ToList()).ToList();
            foreach (var faceIndexes in FacesIndexes)
            {
                List<XYZ> points = faceIndexes.Select(index => oBJParser.Points[index - 1]).ToList();
                TessellatedFace tessellatedFace = new TessellatedFace(new List<IList<XYZ>> { points }, new ElementId(-1));
                shapeBuilder.AddFace(tessellatedFace);
            }
            shapeBuilder.CloseConnectedFaceSet();
            shapeBuilder.Build();
            TessellatedShapeBuilderResult result = shapeBuilder.GetBuildResult();
            GeometryObjects = result.GetGeometricalObjects().ToList();
        }
    }
}
