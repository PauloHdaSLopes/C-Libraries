using Paulo.Console.model.emailUtil;
using Paulo.Console.util.models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Paulo.Console.model;
using System.IO;
using System.Net;
using System.Net.Mime;
using Paulo.Console.util;
using System.Threading;
using System.Timers;

namespace Paulo.WinForm
{
    public partial class frmTester : Form
    {
        string _pedidoReceiveFolder = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Testes\E-mails Recebidos\Pedidos\";
        string _apoliceReceiveFolder = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Testes\E-mails Recebidos\Apolice\";
        string _kitReceiveFolder = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Testes\Kit completo\";

        //string _apoliceFolder = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Arquivos de Apolice\";
        //string _pedidoFolder = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Arquivos de Pedidos\";

        //List<string> _lstSeguradoras = new List<string>();
        string[] _credenciais = new string[3];

        EmailService _em;

        //
        System.Timers.Timer tmrMail;
        System.Timers.Timer tmrFindRename;

        public frmTester()
        {
            // E-Mail Operações RM - Oficial
            // Porta para o Imap Client
            EmailConfiguration eConfig = new EmailConfiguration() { Port = 993, Server = "outlook.office365.com", Username = "operacoesrm@interfile.com.br", Password = "inter@2018" };

            // Porta para o SMTP Client
            //EmailConfiguration eConfig = new EmailConfiguration() { Port = 25, Server = "outlook.office365.com", Username = "proc.automacoes.int@interfile.com.br", Password = "I!34int@gnn" };

            this._em = new EmailService(eConfig);
            InitializeComponent();

            //Cadastro de seguradoras
            //_lstSeguradoras.Add("ZURICH");
            //_lstSeguradoras.Add("Notificación");


            //foreach (var seg in _lstSeguradoras)
            //{
            //    if (!Directory.Exists(_path + seg))
            //        Directory.CreateDirectory(_path + seg);
            //}

            //Timer para execução do serviço de email
            tmrMail = new System.Timers.Timer(60000);
            tmrMail.Elapsed += OnTimerMailEnvent;
            //tmrMail.Start();

            //Timer para execução do serviço de localizar e renomear apolice
            tmrFindRename = new System.Timers.Timer(60000);
            tmrFindRename.Elapsed += OnTimerFindApoliceEnvent;
            //tmrFindRename.Start();

            //
            tmrGeneric.Start();
        }

        private void OnTimerMailEnvent(Object source, ElapsedEventArgs e)
        {
            new Thread(EmailsService).Start();
        }

        private void OnTimerFindApoliceEnvent(Object source, ElapsedEventArgs e)
        {
            new Thread(FindApoliceService).Start();
        }

        private void AtualizarInformacao()
        {
            lblDataHoraAtualizacao.Text = DateTime.Now.ToString();
            lblQntApolice.Text = Directory.GetFiles(_apoliceReceiveFolder).Count().ToString();
            lblQntPedido.Text = Directory.GetFiles(_pedidoReceiveFolder).Count().ToString();
            lblQntKit.Text = Directory.GetFiles(_kitReceiveFolder).Count().ToString();
        }

        private void FindApoliceService()
        {
            // ToDo: Alterar para pegar os pedidos armazenados no banco de dados
            foreach (var pedidoFile in Directory.GetFiles(_pedidoReceiveFolder))
            {
                PDFUtil pUtil = new PDFUtil() { _pdfPath = pedidoFile };

                string pedidoText = "";
                for (int i = 1; i <= pUtil.getNumberPages(); i++)
                {
                    pedidoText += pUtil.extractAllText(i);
                }

                Pedido p = new Pedido(pedidoText);

                var lstApolice = SearchApolice(p);

                //var tmp = _kitReceiveFolder + p.pedidoMarsh + "\\";

                //if (!Directory.Exists(tmp))
                //    Directory.CreateDirectory(tmp);

                ////ToDo:  [Nº do Pedido Marsh]_[Tipo Documento]_CLIENTE ESTRATEGICO_[Prioridade]
                //File.Move(pedidoFile, tmp + string.Format("{0}_{1}.pdf", p.pedidoMarsh, "Pedido"));
                //File.Move(apoliceFile, tmp + string.Format("{0}_{1}.pdf", p.pedidoMarsh, p.tipoDocumento));
            }
        }

        private List<string> SearchApolice(Pedido p)
        {
            // Instancia do utilitario PDF
            PDFUtil pUtil = new PDFUtil();

            // Declaração da lista de retorno
            List<string> retorno = new List<string>();

            // Varredura na pasta onde contem os arquivos de apolice
            foreach (var apoliceFile in Directory.GetFiles(_apoliceReceiveFolder))
            {
                // indico o caminho do PDF a ser manipulado
                pUtil._pdfPath = apoliceFile;

                // Obter a quantidade de paginas no pdf
                var pagesCount2 = pUtil.getNumberPages();

                // Variavel para receber o texto completo da apolice
                string apoliceText = "";

                // Loop para iterar as paginas e adicionar a variavel do texto
                for (int i = 1; i <= pagesCount2; i++)
                {
                    apoliceText += pUtil.extractAllText(i);
                }

                // Verificação para saber se o arquivo é um boleto ou possui boleto
                switch (IsBoleto(apoliceText))
                {
                    //Apenas Boleto
                    case 1:
                        p.tipoDocumento = "BOLETO";
                        break;
                    //Boleto e Apolice
                    case 2:
                        p.tipoDocumento += " E BOLETO";
                        break;
                    default:
                        break;
                }

                // --> Validação dos campos <-- //

                // Declaração de um dicionario utilizado para receber o percentual de acerto na identificação do documento
                IDictionary<string, bool> percent = new Dictionary<string, bool>();

                //ToDo: Quando for alterado para o arquivo indice
                foreach (var prop in p.GetType().GetProperties())
                {
                    if (apoliceText.ToLower().Contains(prop?.GetValue(p, null).ToString().ToLower()))
                        percent.Add(prop.Name, true);
                }

                // Validação para encontrar o nome do assegurado mesmo que escrito de outra forma
                #region Validacao de nome assegurado
                var splitNomeAssegurado = p.segurado.Split(' ');
                IDictionary<string, int> dicNomeAssegurado = new Dictionary<string, int>();

                foreach (var item in splitNomeAssegurado)
                {
                    if (item.Length > 3)
                        dicNomeAssegurado.Add(item, Regex.Matches(apoliceText, item).Count);
                }

                int totalNaoEncontrado = dicNomeAssegurado.Where(w => w.Value == 0).Count();
                int totalEncontrado = dicNomeAssegurado.Where(w => w.Value > 0).Count();

                if (((totalNaoEncontrado + totalEncontrado) * totalNaoEncontrado) >= 50)
                    percent.Add("assegurado", true);
                #endregion

                if (percent.Count >= 7 || percent.ContainsKey("pedidoMarsh"))
                {
                    retorno.Add(apoliceFile);
                }
            }
            return retorno;
        }

        private void EmailsService()
        {
            var emails = _em.GetAllNew();

            foreach (var email in emails)
            {
                if (email.Subject.ToLower().Contains("arquivo indice"))
                {
                    SeleniumRobot.Marsh.Robot.GetFileFromOWEXCHANGE();
                    _em.MoveEmail(email.UID, "Arquivos Indice");
                }
                else if (email.attachments.Count > 0)
                {
                    foreach (var item in email.attachments)
                    {
                        //ToDo: Alterar validação caso os emails da marsh com pedido nao conter a palavra no assunto
                        using (var fs = File.Create((email.Subject.ToLower().Contains("pedido") ? _pedidoReceiveFolder : _apoliceReceiveFolder) + "\\" + item.Name))
                        {
                            item.Archive.WriteTo(fs);
                            item.Archive.Close();
                            item.Archive.Flush();
                        }
                    }
                    _em.MoveEmail(email.UID, "Arquivos Seguradoras");
                }
                // Caso seja Email com link no corpo
                else if (HasPdfLink(email))
                {
                    //ToDo: Tratar E-Mail
                    var links = GetPdfLink(email);

                    //ToDo: Incluir padrão de nomenclatura do arquivo
                    foreach (var link in links)
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(link, string.Format("{0}\\{1}.pdf", email.Subject.ToLower().Contains("pedido") ? _pedidoReceiveFolder : _apoliceReceiveFolder, Regex.Match(email.Subject.Replace('/', ' '), @"\d{1,99} \d{1,99}")));
                        }
                    }
                }
                //Caso seja E-Mail com credenciais
                else if (email.Subject.StartsWith("Notificación"))
                {
                    extrairCredenciaisEmail(email);
                }
            }
        }

        private void TstSftp()
        {
            using (FTPUtil fUtil = new FTPUtil("test.rebex.net", "demo", "password"))
            {
                fUtil.Connect();
                fUtil.ListAllDirectories();
                fUtil.Disconnect();
            }
        }

        //Função de teste para o envio de E-Email
        private void EnviarEmail()
        {
            EmailMessage message = new EmailMessage();
            List<EmailAddress> lsToAddresses = new List<EmailAddress>();
            List<EmailAddress> lsFromAddresses = new List<EmailAddress>();

            EmailAddress toAddress = new EmailAddress();
            EmailAddress fromAddress = new EmailAddress();

            toAddress.Address = "mt11201@interfile.com.br";
            toAddress.Name = "Paulo Lopes";

            lsToAddresses.Add(toAddress);

            fromAddress.Address = "proc.automacoes.int@interfile.com.br";
            fromAddress.Name = "Automação";

            lsFromAddresses.Add(fromAddress);

            message.ToAddresses = lsToAddresses;
            message.FromAddresses = lsFromAddresses;
            message.Subject = "Teste de Serviço Emails";
            message.Content = "Este email esta sendo enviado como Teste!!";

            _em.Send(message);
        }

        private void extrairCamposApoliceEmail(EmailMessage email)
        {
            try
            {
                HtmlUtil hUtil = new HtmlUtil();
                hUtil.SetHtmlDocument(email.Content);

                var docInnerText = hUtil.GetInnerText();
                var numeroApolice = Regex.Match(docInnerText, "(?<=Documento: ).+").ToString().Replace('\r', ' ');
                var segurado = Regex.Match(docInnerText, "(?<=Segurado: ).+").ToString().Replace('\r', ' ');

                var linkArquivo = hUtil.GetElementAttribute("a", "href").Where(w => w.Contains(".pdf")).FirstOrDefault();

                //return new Apolice(numeroApolice.Split('/')[0], numeroApolice.Split('/')[1], segurado, linkArquivo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void extrairCredenciaisEmail(EmailMessage email)
        {
            HtmlUtil hUtil = new HtmlUtil();
            hUtil.SetHtmlDocument(email.Content);

            var docInnerText = hUtil.GetInnerText();

            if (_credenciais[0] == null)
            {
                _credenciais[0] = Regex.Match(docInnerText, ".+(?<=.com)").ToString();
                _credenciais[1] = Regex.Match(docInnerText, "(?<=Contraseña: ).+").ToString().Replace('\r', ' ');
            }
            _credenciais[2] = Regex.Match(docInnerText, "(?=https:).+").ToString();

            if (_credenciais[2] != "")
            {
                SeleniumRobot.Marsh.Zurich.Robot.GetFileFromZurich(_apoliceReceiveFolder, _credenciais);
            }
        }

        private bool HasPdfLink(EmailMessage email)
        {
            HtmlUtil hUtil = new HtmlUtil();
            hUtil.SetHtmlDocument(email.Content);

            return hUtil.GetElementAttribute("a", "href").Where(w => w.Contains(".pdf")).ToList().Count > 0 ? true : false;
        }

        private List<string> GetPdfLink(EmailMessage email)
        {
            HtmlUtil hUtil = new HtmlUtil();
            hUtil.SetHtmlDocument(email.Content);

            return hUtil.GetElementAttribute("a", "href").Where(w => w.Contains(".pdf")).ToList();
        }

        private List<string> GetLink(EmailMessage email)
        {
            HtmlUtil hUtil = new HtmlUtil();
            hUtil.SetHtmlDocument(email.Content);

            return hUtil.GetElementAttribute("a", "href").ToList();
        }

        private int IsBoleto(string apolice)
        {
            var qntCodBar = Regex.Matches(apolice, @"\d{5}\.\d{5}\ \d{5}\.\d{6}\ \d{5}\.\d{6}\ \d{1}\ \d{14}").Count;
            if (qntCodBar == 0)
                qntCodBar = Regex.Matches(apolice, @"\d{44}").Count;

            if (qntCodBar > 0)
                return 1;

            return 0;
        }

        private void tmrGeneric_Tick(object sender, EventArgs e)
        {
            AtualizarInformacao();
        }


        // TESTES DE IMAGEM

        Point startCoordinates = new Point();
        Point finalCoordinates = new Point();

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.ShowDialog();
            txtPath.Text = op.FileName;

            var img = Image.FromFile(txtPath.Text);
            pictureBox8.Width = img.Width;
            pictureBox8.Height = img.Height;
            pictureBox8.Image = img;
            //MessageBox.Show(new OcrUtil() { _imgPath = @"C:\Users\mt11201\Pictures\ocr_test.PNG" }.GetTextFromRect(28, 438, 221, 27));
        }

        private void GetCoordinate(object sender, MouseEventArgs e)
        {
            int x;
            int y;

            x =e.Location.X;
            y = e.Location.Y;

            txtPosX.Text = x.ToString();
            txtPosY.Text = y.ToString();

        }

        private void pictureBox8_MouseDown(object sender, MouseEventArgs e)
        {
            startCoordinates = e.Location;
        }

        private void pictureBox8_MouseUp(object sender, MouseEventArgs e)
        {
            finalCoordinates = e.Location;

            //double width = Math.Sqrt(Math.Pow(finalCoordinates.X - startCoordinates.X, 2) + Math.Pow(finalCoordinates.Y - startCoordinates.Y, 2));
            //double height = Math.Sqrt(Math.Pow(finalCoordinates.X - startCoordinates.X, 2) + Math.Pow(finalCoordinates.Y - startCoordinates.Y, 2));

            int width = finalCoordinates.X - startCoordinates.X;
            int height = finalCoordinates.Y - startCoordinates.Y;
            txtWidth.Text = (width).ToString();
            txtHeight.Text = (height).ToString();

            Graphics g = pictureBox8.CreateGraphics();

            g.DrawRectangle(new Pen(Brushes.Black),
            new Rectangle(startCoordinates,new Size(Convert.ToInt32(width), Convert.ToInt32(height))));

            txtExtractedText.Text = new OcrUtil() {_imgPath = txtPath.Text }.GetTextFromRect(startCoordinates.X, startCoordinates.Y, width, height);
        }

    }
}