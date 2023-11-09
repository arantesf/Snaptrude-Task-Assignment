using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit.Common
{
    public static class GeometryHelper
    {
        public static double GetElementGeometryVolume(Element element)
        {
            double volume = 0;

            Options opt = new Options
            {
                IncludeNonVisibleObjects = false,
            };

            GeometryElement geometryElement = element.get_Geometry(opt);

            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is Solid)
                    volume += (geometryObject as Solid).Volume;
                else if (geometryObject is GeometryInstance geometryInstance)
                {
                    foreach (GeometryObject geoInstanceObject in (geometryInstance).GetInstanceGeometry())
                    {
                        if (geoInstanceObject is Solid)
                            volume += (geoInstanceObject as Solid).Volume;
                    }
                }
            }

            return volume;
        }

        public static List<Solid> GetElementSolids(Element element)
        {
            List<Solid> solids = new List<Solid>();

            Options opt = new Options
            {
                IncludeNonVisibleObjects = false,
            };

            GeometryElement geometryElement = element.get_Geometry(opt);

            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is Solid solid)
                {
                    if (!solids.Any())
                    {
                        solids.Add(solid);
                    }
                    else
                    {
                        bool unionSuccedded = false;
                        for (int i = 0; i < solids.Count; i++)
                        {
                            try
                            {
                                solids[i] = BooleanOperationsUtils.ExecuteBooleanOperation(solids[i], solid, BooleanOperationsType.Union);
                                unionSuccedded = true;
                            }
                            catch (System.Exception)
                            {
                            }
                            if (unionSuccedded)
                            {
                                break;
                            }
                        }
                        if (!unionSuccedded)
                        {
                            solids.Add(solid);
                        }
                    }
                }
                else if (geometryObject is GeometryInstance geometryInstance)
                {
                    foreach (GeometryObject geoInstanceObject in (geometryInstance).GetInstanceGeometry())
                    {
                        if (geometryObject is Solid geoInstanceSolid)
                        {
                            if (!solids.Any())
                            {
                                solids.Add(geoInstanceSolid);
                            }
                            else
                            {
                                bool unionSuccedded = false;
                                for (int i = 0; i < solids.Count; i++)
                                {
                                    try
                                    {
                                        solids[i] = BooleanOperationsUtils.ExecuteBooleanOperation(solids[i], geoInstanceSolid, BooleanOperationsType.Union);
                                        unionSuccedded = true;
                                    }
                                    catch (System.Exception)
                                    {
                                    }
                                    if (unionSuccedded)
                                    {
                                        break;
                                    }
                                }
                                if (!unionSuccedded)
                                {
                                    solids.Add(geoInstanceSolid);
                                }
                            }
                        }
                    }
                }
            }

            return solids;
        }
    }

}
