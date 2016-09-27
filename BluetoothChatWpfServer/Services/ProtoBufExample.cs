using ProtoBuf;
using System; // for DateTime
using System.Text; // for StringBuilder
using System.Threading; // for Interlocked
using System.Collections.Generic; // for List

namespace BluetoothChatWpfServer.Services
{
    public class ProtoBufExample
    {
        public enum eType : byte { eError = 0, eFeedback, eBook, eFable };

        [ProtoContract]
        public class Header
        {
            [ProtoMember(1)]
            public eType objectType;
            [ProtoMember(2)]
            public readonly int serialMessageId;

            public object data;
            private static int _HeaderSerialId = 0;

            public Header(object xData, eType xObjectType, int xSerialMessageId = 0)
            {
                data = xData;
                serialMessageId = (xSerialMessageId == 0) ? Interlocked.Increment(ref _HeaderSerialId) : xSerialMessageId;
                objectType = xObjectType; // we could use "if typeof(T) ...", but it would be slower, harder to maintain and less legible
            } // constructor

            // parameterless constructor needed for Protobuf-net
            public Header()
            {
            } // constructor

        } // class

        [ProtoContract]
        public class ErrorMessage
        {
            [ProtoMember(1)]
            public string Text;
        } // class

        [ProtoContract]
        public class Book
        {
            [ProtoMember(1)]
            public string author;
            [ProtoMember(2, DataFormat = DataFormat.Group)]
            public List<Fable> stories;
            [ProtoMember(3)]
            public DateTime edition;
            [ProtoMember(4)]
            public int pages;
            [ProtoMember(5)]
            public double price;
            [ProtoMember(6)]
            public bool isEbook;

            public override string ToString()
            {
                StringBuilder s = new StringBuilder();
                s.Append("by "); s.Append(author);
                s.Append(", edition "); s.Append(edition.ToString("dd MMM yyyy"));
                s.Append(", pages "); s.Append(pages);
                s.Append(", price "); s.Append(price);
                s.Append(", isEbook "); s.Append(isEbook);
                s.AppendLine();
                if (stories != null) foreach (Fable lFable in stories)
                    {
                        s.Append("title "); s.Append(lFable.title);
                        s.Append(", rating "); s.Append(lFable.customerRatings.ToString()); // Average() is an extension method of "using System.Linq;"
                        s.AppendLine();
                    }

                return s.ToString();
            } //
        } // class

        [ProtoContract]
        public class Fable
        {
            [ProtoMember(1)]
            public string title;
            [ProtoMember(2, DataFormat = DataFormat.Group)]
            public double[] customerRatings;

            public override string ToString()
            {
                return "title " + title + ", rating " + customerRatings.ToString();
            } //
        } // class

        public static Book GetData()
        {
            return new Book
            {
                author = "Aesop",
                price = 1.99,
                isEbook = false,
                edition = new DateTime(1975, 03, 13),
                pages = 203,
                stories = new List<Fable>(new Fable[] {
                new Fable{ title = "The Fox & the Grapes", customerRatings = new double[]{ 0.7, 0.7, 0.8} },
                new Fable{ title = "The Goose that Laid the Golden Eggs", customerRatings = new double[]{ 0.6, 0.75, 0.5, 1.0} },
                new Fable{ title = "The Cat & the Mice", customerRatings = new double[]{ 0.1, 0.0, 0.3} },
                new Fable{ title = "The Mischievous Dog", customerRatings = new double[]{ 0.45, 0.5, 0.4, 0.0, 0.5} }
            })
            };
        } //
    } // class
}
