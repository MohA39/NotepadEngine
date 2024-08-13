using System.Globalization;
using System.Text;

namespace NotepadEngine
{
    internal class TextHelpers
    {
        public static string RotateString(string str, int angle)
        {
            int RoundedAngle = RoundAngle(angle);

            if (RoundedAngle == 0)
            {
                return str;
            }

            List<List<string>> strL = StringToListListString(str);

            switch (RoundAngle(angle))
            {
                case 90:
                case -270:
                    return FlattenListList(Transpose(strL).Select(x => x.AsEnumerable().Reverse().ToList()).ToList());
                case -90:
                case 270:
                    return FlattenListList(Transpose(strL).AsEnumerable().Reverse().ToList());
                case 180:
                case -180:
                    return FlattenListList(strL.Select(x => x.AsEnumerable().Reverse().ToList()).AsEnumerable().Reverse().ToList());
            }

            throw new Exception();
        }
        public static List<List<string>> Transpose(List<List<string>> str)
        {
            return str.SelectMany(inner => inner.Select((item, index) => new { item, index }))
            .GroupBy(i => i.index, i => i.item)
            .Select(g => g.ToList()).ToList(); // Transpose
            ;
        }

        private static int RoundAngle(int angle)
        {
            return (int)(Math.Round((double)(angle) / 90, MidpointRounding.AwayFromZero) * 90) % 360;
        }
        private static List<List<string>> StringToListListString(string str)
        {
            string[] FrameLines = str.Split(Environment.NewLine);
            List<List<string>> FrameArray = new List<List<string>>();
            for (int i = 0; i < FrameLines.Length; i++)
            {
                FrameArray.Add(new List<string>());
                TextElementEnumerator charEnum = StringInfo.GetTextElementEnumerator(FrameLines[i]);
                while (charEnum.MoveNext())
                {
                    FrameArray.Last().Add(charEnum.GetTextElement());
                }
            }
            return FrameArray;
        }
        private static string FlattenListList(List<List<string>> list)
        {
            return list.Aggregate(new StringBuilder(),
                  (sb, a) => sb.AppendLine(String.Join("", a)),
                  sb => sb.ToString());
        }
    }
}
