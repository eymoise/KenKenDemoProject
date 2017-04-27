using KenKenLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KenKenGUI
{
    public static class MyHelper
    {
        public static void SaveAsXmlFormat(object objGraph, string fileName)
        {
            // Save object to a file named CarData.xml in XML format.
            XmlSerializer xmlFormat = new XmlSerializer(typeof(SerializableCellGroup[]));
            using (Stream fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xmlFormat.Serialize(fStream, objGraph);
            }
        }
    }

    public class SerializableCellGroup
    {
        public SerializableTuple[] SerializableCoordinates { get; set; }

        public string MathematicalOperation { get; set; }
        public int Target { get; set; }

        static public SerializableCellGroup ConvertCellGroup(CellGroup cellGroup)
        {
            var converted = new SerializableCellGroup();
            converted.MathematicalOperation = cellGroup.MathematicalOperation.StringFormat();
            converted.Target = cellGroup.Target;
            converted.SerializableCoordinates = cellGroup.Coordinates.Select(tuple =>
            {
                SerializableTuple s = new SerializableTuple();
                s.X = tuple.Item1;
                s.Y = tuple.Item2;
                return s;
            }).ToArray();

            return converted;
        }
    }

    public struct SerializableTuple
    {
        public int X;
        public int Y;
    }
}
