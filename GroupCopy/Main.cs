using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupCopy
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                Reference selectGroup = uidoc.Selection.PickObject(ObjectType.Element,new SelectionFilter(),"Выберете группу");
                Element elemGroup = doc.GetElement(selectGroup);
                Group group = elemGroup as Group;
                XYZ groupCenter = GetElementCenter(group);
                XYZ selectPoint = uidoc.Selection.PickPoint("Выберете точку");
                Room room = GetRoomByPoint(doc, selectPoint);
                XYZ roomCenter = GetElementCenter(room);
                double zCoord = roomCenter.Z-groupCenter.Z;

                XYZ offset = new XYZ(roomCenter.X,roomCenter.Y, groupCenter.Z);
                using (Transaction ts = new Transaction(doc,"Копирование группы"))
                {
                    ts.Start();
                    doc.Create.PlaceGroup(offset, group.GroupType);

                    TaskDialog.Show("Окно",room.Name.ToString());
                    ts.Commit();
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
   
            return Result.Succeeded;
        }
        public XYZ GetElementCenter(Element element)
        {
           BoundingBoxXYZ box = element.get_BoundingBox(null);
            return (box.Max + box.Min) / 2;
        }
        public Room GetRoomByPoint(Document doc,XYZ point)
        {
            FilteredElementCollector coll = new FilteredElementCollector(doc);
            coll.OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element e in coll)
            {
                Room room = e as Room;
                if (room!=null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }
            }
            return null;
        }
    }
}
