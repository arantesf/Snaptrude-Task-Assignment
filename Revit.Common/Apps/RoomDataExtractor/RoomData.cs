using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit.Common
{
    class RoomData
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public double Area { get; set; }
        public double Volume { get; set; }
        public double OccupiedVolume { get; set; }
        public double SpaceUtilizationRatio { get; set; }
        public UtilizationCategory UtilizationCategory { get; set; }

        public RoomData(Room room, IEnumerable<FamilyInstance> occupantFamilyInstances)
        {
            Solid roomSolid = room.ClosedShell.First() as Solid;
            Name = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
            Number = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
            Area = room.Area;
            Volume = roomSolid.Volume;
            OccupiedVolume = GetVolumeFromOccupantFamilyInstances(occupantFamilyInstances, roomSolid);
            SetSpaceUtilizationRatioAndCategory();
        }

        private double GetVolumeFromOccupantFamilyInstances(IEnumerable<FamilyInstance> occupantFamilyInstances, Solid roomSolid)
        {
            // I'VE DID SIMPLE IMPLEMENTATIONS OF 2 WAYS TO GET THE ROOMS OF FAMILY INSTANCES IN REVIT API
            // THE CHOICE OF WHICH METHOD TO USE SHOULD BE BASED ON THE QUALITY AND LEVEL OF INFORMATION OF THE REVIT FAMILIES AND PROJECT FILES
            // I'VE ORDER THEM FROM THE FASTEST TO THE SLOWEST

            double occupiedVolume = 0;

            if (!occupantFamilyInstances.Any())
            {
                return occupiedVolume;
            }

            #region 1ST METHOD: ADD OCCUPIED VOLUME FROM THE FAMILY INSTANCES WITH EXPLICIT ROOM DECLARATION IN PROPERTY "Room", THIS IS ARE STOCK REVIT INFORMATION, THAT NEED SOME CONFIGURATION IN EVERY REVIT FAMILY
            IEnumerable<FamilyInstance> familyInstancesWithExplicitRoom = occupantFamilyInstances
                .Where(familyInstance => familyInstance.Room?.get_Parameter(BuiltInParameter.ROOM_NAME).AsString() == Name)
                .ToList();

            occupiedVolume += familyInstancesWithExplicitRoom
                .Select(familyInstance => GeometryHelper.GetElementGeometryVolume(familyInstance))
                .Sum();

            List<FamilyInstance> familyInstancesWithoutExplicitRoom = occupantFamilyInstances
                .Where(familyInstance => familyInstance.Room == null)
                .ToList();

            if (!familyInstancesWithoutExplicitRoom.Any())
            {
                return occupiedVolume;
            }

            #endregion

            #region 2ND METHOD: ADD OCCUPIED VOLUME FROM THE REMAINING FAMILY INSTANCES USING BOOLEAN OPERATIONS OF SOLID INTERSECTIONS
            for (int i = 0; i < familyInstancesWithoutExplicitRoom.Count(); i++)
            {
                FamilyInstance familyInstance = familyInstancesWithoutExplicitRoom[i];
                List<Solid> solids = GeometryHelper.GetElementSolids(familyInstance);
                foreach (Solid solid in solids)
                {
                    try
                    {
                        Solid intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomSolid, solid, BooleanOperationsType.Intersect);
                        if (intersectSolid.Volume > 0)
                        {
                            double volume = GeometryHelper.GetElementGeometryVolume(familyInstance);
                            occupiedVolume += volume;
                            familyInstancesWithoutExplicitRoom.RemoveAt(i);
                            i--;
                            occupantFamilyInstances = occupantFamilyInstances.Where(fi => fi.Id != familyInstance.Id);
                            break;
                        }
                    }
                    catch (System.Exception)
                    {
                        // IN MY EXPERIENCE, THE EXECUTEBOOLEANOPERATION METHOD ONLY GIVES AN EXCEPTION WHEN THERE IS A INTERSECTION
                        double volume = GeometryHelper.GetElementGeometryVolume(familyInstance);
                        occupiedVolume += volume;
                        familyInstancesWithoutExplicitRoom.RemoveAt(i);
                        i--;
                        occupantFamilyInstances = occupantFamilyInstances.Where(fi => fi.Id != familyInstance.Id);
                        break;
                    }
                }
            }
            #endregion

            return occupiedVolume;
        }

        public void SetSpaceUtilizationRatioAndCategory()
        {
            SpaceUtilizationRatio = OccupiedVolume / Volume;
            if (SpaceUtilizationRatio < 0.3)
            {
                UtilizationCategory = UtilizationCategory.UnderUtilized;
            }
            else if (SpaceUtilizationRatio <= 0.8)
            {
                UtilizationCategory = UtilizationCategory.WellUtilized;
            }
            else
            {
                UtilizationCategory = UtilizationCategory.OverUtilized;
            }
        }
    }

    
}
