using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace Paulo.Console.util.models
{
    public class OcrUtil
    {
        public string _imgPath { get; set; }

        public string GetAllText()
        {
            string texto = "ERRO";
            try
            {
                using (var ocr = new TesseractEngine(@"tessdata", "por", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(_imgPath))
                    {
                        using (var page = ocr.Process(img))
                        {
                            texto = page.GetText();
                        }
                    }
                }
                return texto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetTextFromRect(int x, int y, int width, int height)
        {
            string texto = "ERRO";

            try
            {
                using (var ocr = new TesseractEngine(@"tessdata", "por", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(_imgPath))
                    {
                        using (var page = ocr.Process(img, new Tesseract.Rect(x,y,width,height)))
                        {
                            texto = page.GetText();
                        }
                    }
                }
                return texto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
