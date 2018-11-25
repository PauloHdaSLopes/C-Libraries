using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.util;
using System.Text.RegularExpressions;
using Paulo.Console.util;
using Paulo.Console.model;

namespace Paulo.Console
{
    class MarshProgram
    {
        static void Main(string[] args)
        {
            
        }
        //static void Main(string[] args)
        //{
        //    //  Caminho do PDF
        //    //string _pdfPath = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\PROPERTY\53031196\Pedido\Seg_PROPERTY_53031196.pdf";
        //    //string _pdfPath = @"C: \Users\mt11201\Documents\Especificações\Marsh Risk\MARINE\53004294\Pedido\Seg_MARINE_53004294.pdf";
        //    string _pdfPath = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Teste Pedidos\Seg_AUTO_53067224.pdf";

        //    foreach (var item in Directory.GetFiles(@"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Teste Pedidos"))
        //    {
        //        _pdfPath = item;

        //        List<string> textosPdf = new List<string>();

        //        for (int i = 1; i <= PDFUtil.getNumberPages(_pdfPath); i++)
        //        {
        //            textosPdf.Add(PDFUtil.extractAllText(_pdfPath, i));
        //        }

        //        //Extrair as informações do pdf em string da primeira Página
        //        System.Console.WriteLine("\n\nArquivo:" + Path.GetFileName(_pdfPath));

        //        Pedido p = new Pedido(textosPdf);

        //        p.ToString();

        //        string text;
        //        foreach (var document in Directory.GetFiles(@"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Documentos"))
        //        {
        //            text = "";

        //            for (int i = 1; i <= PDFUtil.getNumberPages(document); i++)
        //            {
        //                text += PDFUtil.extractAllText(document,i);
        //            }

        //            //Neste caso incluir a validação dos campos do pedido
        //            if (text.Contains(p._pedidoMarsh))
        //            {
        //                System.Console.WriteLine(string.Format("Segurado: {0} : {1}",p._segurado, document));

        //            }
        //        }
        //    }
        ////}
    }
}