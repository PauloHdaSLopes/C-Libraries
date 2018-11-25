using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.IO;
using System.Text;
using System.util;

namespace Paulo.Console.util
{
    public class PDFUtil
    {
        public string _pdfPath { get; set; }

        public void extractTextFromArea()
        {
            using (PdfReader reader = new PdfReader(_pdfPath))
            {
                string output;
                //Width 100
                //Account Responsavel: 90,70
                //Codigo SUSEP 300,90
                //Corretora 50,450
                //Distribuição de corretagem 400,450
                //Parcelamento e 1º Parcela para 0,500
                //Detalhes 150,500
                //Vigencia 50, 520 <<--- ajustar
                //

                float x = 300, y = 90;

                do
                {
                    PdfReader PDFreader = new PdfReader(_pdfPath);
                    RectangleJ rect = new RectangleJ(
                           x: x,    //Starting point from the left
                           y: y,    //Starting point from the bottom
                           width: 100f, //Total rectangle to the right
                           height: 25f //Total rectangle downwards
                           );

                    RenderFilter filter = new RegionTextRenderFilter(rect);

                    ITextExtractionStrategy strategy;
                    strategy = new FilteredTextRenderListener(
                         deleg: new LocationTextExtractionStrategy(),
                         filters: new RenderFilter[] { filter }
                         );

                    string foundText = PdfTextExtractor.GetTextFromPage(
                              reader: PDFreader,
                              pageNumber: 1,
                              strategy: strategy
                              );

                    if (foundText.Length > 0)
                    {
                        System.Console.WriteLine(string.Format("{0}, {1}, {2}", x, y, foundText));
                    }

                    if ((x += 10) == 500)
                    {
                        y += 10;
                        x = 0;
                    }
                } while (true);

            }
        }

        public string extractAllText(int page)
        {
            using (PdfReader reader = new PdfReader(_pdfPath))
            {
                return PdfTextExtractor.GetTextFromPage(reader, page);
            }
        }

        public void Decrypt()
        {
            //using (PdfReader reader = new PdfReader(_pdfPath))
            //{
            //    using (PdfStamper stamper = new PdfStamper(reader,new MemoryStream()))
            //    {
            //        reader.
            //    }
            //}
        }

        public void extractObject()
        {
            using (PdfReader reader = new PdfReader(_pdfPath))
            {
                PdfObject obj;
                for (int i = 1; i <= reader.XrefSize; i++)
                {
                    obj = reader.GetPdfObject(i);
                    if (obj != null && obj.IsStream())
                    {
                        PRStream stream = (PRStream)obj;
                        byte[] b;
                        try
                        {
                            b = PdfReader.GetStreamBytes(stream);
                        }
                        catch (Exception e)
                        {
                            b = PdfReader.GetStreamBytesRaw(stream);
                        }
                        System.Console.Clear();
                        System.Console.WriteLine(Encoding.UTF8.GetString(b));
                    }
                }
            }
        }

        public void readFormFields()
        {
            PdfReader pdfReader = new PdfReader(_pdfPath);

            //Get the Form
            AcroFields form = pdfReader.AcroFields;

            //Go thru all fields in the form
            foreach (var field in form.Fields)
            {
                //Get the fields value
                string value = form.GetField(field.Key);

                //Print the result with the field name and it's value
                System.Console.WriteLine("{0}, {1}",
                    field.Key,
                    value);
            }
        }

        public int getNumberPages()
        {
            using (PdfReader reader = new PdfReader(_pdfPath))
            {
                return reader.NumberOfPages;
            }
        }
    }
}